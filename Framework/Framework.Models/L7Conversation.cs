// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Core.Database;
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Services;
using Netfox.NBARDatabase;
using PacketDotNet;
using PostSharp.Patterns.Model;

namespace Netfox.Framework.Models
{
    /// <summary>
    ///     This class holds BidirectionalFlow data about all IP and TCP conversations with common IP.SRC,
    ///     IP.DST and TCP.SRC, TCP.DST and L2 ProtocolType
    /// </summary>
    [DataContract]
    [Serializable]
    [Persistent]
    public class L7Conversation : ILxConversation, IWindsorContainerChanger
    {
        public ICollection<L7PDU> _L7PDUs;

        private PersistableJsonSerializableString _applicationTags;
        private string _name;
        protected L7Conversation() { } //EF

        public L7Conversation(FsUnidirectionalFlow flow, DaRFlowDirection flowDirection, IWindsorContainer localWindsorContainer)
        {
            this.InvestigationWindsorContainer = localWindsorContainer;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    this.InitializeConversation(flow, null);
                    break;
                case DaRFlowDirection.down:
                    this.InitializeConversation(null, flow);
                    break;
                case DaRFlowDirection.non:
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }
        }
        public L7Conversation(FsUnidirectionalFlow upFlow, FsUnidirectionalFlow downFlow, IWindsorContainer localWindsorContainer)
        {
            this.InvestigationWindsorContainer = localWindsorContainer;
            this.InitializeConversation(upFlow, downFlow);
        }

        private void InitializeConversation(FsUnidirectionalFlow upFlow, FsUnidirectionalFlow downFlow)
        {
            this.L4Conversation = upFlow?.L4Conversation ?? downFlow?.L4Conversation;
            this.UnorderedL7PDUs = this.MergeUpDownPdus(upFlow?.PDUs, downFlow?.PDUs);

            this.LinkL7ConversationAndFlows(upFlow, downFlow);

            this.Frames = this.GetAllFramesFromFlows(upFlow, downFlow);
            foreach(var frame in this.Frames) { frame.L7ConversationRefId = this.Id; }

            this.Captures = new WeakConccurentCollection<PmCaptureBase>(this.GetAllCapturesFromFlows(upFlow, downFlow));

            this.ConversationFlowStatistics = new WeakConccurentCollection<L7ConversationStatistics>();

            this.UpConversationStatistic = new L7ConversationStatistics(upFlow, this, DaRFlowDirection.up);
            this.DownConversationStatistic = new L7ConversationStatistics(downFlow, this, DaRFlowDirection.down);
        }

        private void LinkL7ConversationAndFlows(FsUnidirectionalFlow upFlow, FsUnidirectionalFlow downFlow)
        {
            this.LinkL7ConversationAndFlow(upFlow);
            this.LinkL7ConversationAndFlow(downFlow);
        }

        private void LinkL7ConversationAndFlow(FsUnidirectionalFlow flow)
        {
            if(flow != null) { flow.L7Conversation = this; }
        }

        private WeakConccurentCollection<PmFrameBase> GetAllFramesFromFlows(FsUnidirectionalFlow upFlow, FsUnidirectionalFlow downFlow)
        {
            var frames = new WeakConccurentCollection<PmFrameBase>();
            if(upFlow == null && downFlow != null) frames.AddRange(this.L7PDUs.SelectMany(p => p.FrameList).Concat(downFlow.NonDataFrames));
            else if(upFlow != null && downFlow == null) frames.AddRange(this.L7PDUs.SelectMany(p => p.FrameList).Concat(upFlow.NonDataFrames));
            else frames.AddRange(this.L7PDUs.SelectMany(p => p.FrameList).Concat(downFlow.NonDataFrames).Concat(upFlow.NonDataFrames));
            return frames;
        }

        private HashSet<PmCaptureBase> GetAllCapturesFromFlows(FsUnidirectionalFlow upFlow, FsUnidirectionalFlow downFlow)
        {
            var captures = new HashSet<PmCaptureBase>();
            GetCapturesFromFlow(upFlow, captures);
            GetCapturesFromFlow(downFlow, captures);
            return captures;
        }

        private static void GetCapturesFromFlow(FsUnidirectionalFlow flow, HashSet<PmCaptureBase> captures)
        {
            if(flow == null) return;
            foreach(var capture in flow.L4Conversation.Captures) { captures.Add(capture); }
        }

        public CypherKey Key { get; set; } = new CypherKey();

        public ConversationCipherSuite CipherSuite { get; set; }

        [ForeignKey(nameof(L4ConversationRefId))]
        public virtual L4Conversation L4Conversation
        {
            get { return this._l4Conversation; }
            set
            {
                this._l4Conversation = value;
                this.L4ConversationRefId = value.Id;
                if (value.L3Conversation != null)
                    this.L3Conversation = value.L3Conversation;
                
            }
        }

        [Required]
        public Guid L4ConversationRefId { get; private set; }

        [ForeignKey(nameof(L3ConversationRefId))]
        public virtual L3Conversation L3Conversation
        {
            get { return this._l3Conversation; }
            set
            {
                this._l3Conversation = value;
                this.L3ConversationRefId = value.Id;
            }
        }
        [Required]
        public Guid L3ConversationRefId { get; private set; }
        [NotMapped]
        public L7ConversationStatistics UpConversationStatistic
        {
            get { return this._upConversationStatistic ?? (this._upConversationStatistic = this.ConversationFlowStatistics.FirstOrDefault(s => s.FlowDirection == DaRFlowDirection.up)); }
            set
            {
                this._upConversationStatistic = value;
                this.ConversationFlowStatistics.Add(value);
            }
        }
        [NotMapped]
        public L7ConversationStatistics DownConversationStatistic
        {
            get { return this._downConversationStatistic ?? (this._downConversationStatistic = this.ConversationFlowStatistics.FirstOrDefault(s => s.FlowDirection == DaRFlowDirection.down)); }
            set
            {
                this._downConversationStatistic = value;
                this.ConversationFlowStatistics.Add(value);
            }
        }
        [InverseProperty(nameof(global::Netfox.Framework.Models.L7ConversationStatistics.L7Conversation))]
        public virtual ICollection<L7ConversationStatistics> ConversationFlowStatistics { get; protected set; } 


        public IEnumerable<PmFrameBase> FramesData => this.Frames.Where(f => f.L7Pdu != null);
        
        [IgnoreAutoChangeNotification]
        public IEnumerable<PmFrameBase> VirtualFrames
        {
            get
            {
                return this.Frames.Where(f => f.GetType() == typeof(PmFrameVirtualBlank) || f.GetType() == typeof(PmFrameVirtual));
            }
        }

        [IgnoreAutoChangeNotification]
        public IEnumerable<L7PDU> L7PDUs => this.UnorderedL7PDUs.OrderBy(i => i.OrderingKey);
        public virtual ICollection<L7PDU> UnorderedL7PDUs { get; protected set; }
        
        [IgnoreAutoChangeNotification]
        public IEnumerable<L7PDU> UpFlowPDUs =>  this.L7PDUs?.Where(l7PDU => l7PDU.FlowDirection == DaRFlowDirection.up);
        
        [IgnoreAutoChangeNotification]
        public IEnumerable<L7PDU> DownFlowPDUs => this.L7PDUs?.Where(l7PDU => l7PDU.FlowDirection == DaRFlowDirection.down);

        
     

       
        [IgnoreAutoChangeNotification]
        public L7ConversationStatistics ConversationStats => this.L7ConversationStatistics;

        [NotMapped]
        public L7ConversationStatistics L7ConversationStatistics => this._l7ConversationStatistics ?? (this._l7ConversationStatistics = new L7ConversationStatistics(this.UpConversationStatistic, this.DownConversationStatistic));

       
        public AddressFamily L3ProtocolType => this.DestinationEndPoint.AddressFamily;
        
        public IPProtocolType L4ProtocolType => this.L4Conversation.L4ProtocolType;
        public IPProtocolType ProtocolType => this.L4ProtocolType;

      
        [NotMapped]
        public IPEndPoint DestinationEndPoint => this.L4Conversation?.DestinationEndPoint;

        [NotMapped]
        public IPEndPoint SourceEndPoint => this.L4Conversation?.SourceEndPoint;

      

        ILxConversationStatistics ILxConversation.ConversationStats => this.ConversationStats;

        ILxConversationStatistics ILxConversation.DownConversationStatistic => this.DownConversationStatistic;
        
        [IgnoreAutoChangeNotification]
        [NotMapped]
        public IEnumerable<PmFrameBase> DownFlowFrames
        {
            get
            {
                return this.Frames.Where(f => Equals(f.SrcAddress, this.DestinationEndPoint.Address));
            }
        }
        
        private DateTime? _firstSeen;
        private DateTime? _lastSeen;
        public DateTime FirstSeen
        {
            get { return (DateTime)(this._firstSeen ?? (this._firstSeen = this.L7PDUs?.FirstOrDefault()?.FirstSeen ?? this.Frames?.FirstOrDefault()?.TimeStamp ?? DateTime.MinValue)); }
            set { this._firstSeen = value; }
        }

        public DateTime LastSeen
        {
            get { return (DateTime)(this._lastSeen ?? (this._lastSeen = this.L7PDUs?.LastOrDefault()?.LastSeen ?? this.Frames?.LastOrDefault()?.TimeStamp ?? DateTime.MaxValue)); }
            set { this._lastSeen = value; }
        }

        IEnumerable<PmFrameBase> ILxConversation.Frames => this.Frames;

        [IgnoreAutoChangeNotification]
        public virtual ICollection<PmFrameBase> Frames { get; protected set; }



        [DataMember]
        public string Name => this._name ?? (this._name = $"{this.SourceEndPoint} - {this.DestinationEndPoint}");

        IPProtocolType ILxConversation.ProtocolType => this.L4Conversation.L4ProtocolType;

        ILxConversationStatistics ILxConversation.UpConversationStatistic => this.UpConversationStatistic;
        
        public IEnumerable<PmFrameBase> UpFlowFrames
        {
            get
            {
                return this.Frames.Where(f => Equals(f.SrcAddress, this.SourceEndPoint.Address));
            }
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        #region Implementation of IWindsorContainerChanger
        [NotMapped]
        public IWindsorContainer InvestigationWindsorContainer { get; set; }
        #endregion

       
        public Boolean IsXyProtocolConversation(String applicationTag) => this.ApplicationTags.Contains(applicationTag, StringComparer.OrdinalIgnoreCase);

        public Boolean IsXyProtocolConversation(String[] applicationTag)
        {
            return applicationTag.Any(appTag => this.ApplicationTags.Contains(appTag, StringComparer.OrdinalIgnoreCase));
        }

        public IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeApplicationProtocols(IApplicationRecognizer applicationRecognizer)
        {
            var appReco = applicationRecognizer as ApplicationRecognizerBase;
            if(appReco == null) { throw new ArgumentException("applicationRecognizer", @"Selected application recognizer do not exists."); }
            return appReco.RecognizeConversation(this);
        }

        public IReadOnlyList<String> RecognizeApplicationProtocolsTags(IApplicationRecognizer applicationRecognizer)
        {
            var toxanomies = this.RecognizeApplicationProtocols(applicationRecognizer).ToList();
            toxanomies.Sort(NBARProtocolPortDatabase.ProtocolTaxonomyPortComparison);
            return toxanomies.Select(t => t.name).ToList();
        }
        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append("AppTags> ");
            foreach(var applicationTag in this.ApplicationTags)
            {
                sb.Append(applicationTag);
                sb.Append(", ");
            }
            sb.Append(this.L4ProtocolType);
            sb.Append(" ");
            sb.Append(this.SourceEndPoint);
            sb.Append(" ");
            sb.Append(this.DestinationEndPoint);
            sb.Append(" UpPDUs> ");
            sb.Append(this.UpFlowPDUs?.Count() ?? 0);
            sb.Append(" DownPDUs> ");
            sb.Append(this.DownFlowPDUs?.Count() ?? 0);
            return sb.ToString();
        }
        private List<L7PDU> MergeUpDownPdus(IEnumerable<L7PDU> upFlowPdusEnumerable, IEnumerable<L7PDU> downFlowPDUseEnumerable)
        {
            List<L7PDU> l7Pdus;
            if (upFlowPdusEnumerable == null && downFlowPDUseEnumerable != null) l7Pdus= downFlowPDUseEnumerable.ToList();
            else if(downFlowPDUseEnumerable == null && upFlowPdusEnumerable != null) l7Pdus= upFlowPdusEnumerable.ToList();
            else
            {
                l7Pdus = new List<L7PDU>();
                Int32 upIndex = 0, downIndex = 0;
                var upFlowPDUs = upFlowPdusEnumerable?.ToList() ?? new List<L7PDU>();
                var downFlowPDUs = downFlowPDUseEnumerable?.ToList() ?? new List<L7PDU>();
                for(var i = 0; i < upFlowPDUs.Count + downFlowPDUs.Count; i++)
                {
                    if(upFlowPDUs.Count <= upIndex)
                    {
                        l7Pdus.AddRange(from pdu in downFlowPDUs.Skip(downIndex)
                            select pdu);
                        break;
                    }
                    if(downFlowPDUs.Count <= downIndex)
                    {
                        l7Pdus.AddRange(from pdu in upFlowPDUs.Skip(upIndex)
                            select pdu);
                        break;
                    }

                    if(upFlowPDUs.ElementAt(upIndex).FirstSeen < downFlowPDUs.ElementAt(downIndex).FirstSeen)
                    {
                        l7Pdus.Add(upFlowPDUs.ElementAt(upIndex));
                        upIndex++;
                    }
                    else
                    {
                        l7Pdus.Add(downFlowPDUs.ElementAt(downIndex));
                        downIndex++;
                    }
                }
            }
            var pduOrderingKey = 0;
            foreach(var l7Pdu in l7Pdus)
            {
                l7Pdu.OrderingKey = pduOrderingKey++;
            }
            
            return l7Pdus;
        }

        [NotMapped]
        public IReadOnlyList<NBAR2TaxonomyProtocol> ApplicationProtocols
        {
            get
            {
                if (this._applicationProtocols == null)
                {
                    var appRecognizer = this.InvestigationWindsorContainer.Resolve<IApplicationRecognizer>();
                    this._applicationProtocols = this._applicationTags?.Where(tag => tag != string.Empty).Select(appTag => appRecognizer.GetNbar2TaxonomyProtocol(appTag)).ToList();
                }
                return this._applicationProtocols;
            }
            set { this._applicationProtocols = value; }
        }

        [IgnoreAutoChangeNotification]
        [DataMember]
        public PersistableJsonSerializableString ApplicationTags
        {
            get
            {
                return this._applicationTags
                       ?? (this._applicationTags =
                       (this.ApplicationProtocols != null && this.ApplicationProtocols.Any()
                           ? new PersistableJsonSerializableString(this.ApplicationProtocols.Select(proto => proto?.name))
                           : new PersistableJsonSerializableString()));
            }
            set { this._applicationTags = value; }
        }

        public PmFrameBase FirstFrame => this.Frames?.FirstOrDefault();

        public String AppTag
        {
            get
            {
                if (this._appTag != null) return this._appTag;
                this._appTag = this.AppTagProvider?.GetAppTagShort(this.Captures, this.FirstFrame);
                return this._appTag;
            }
        }

        public string AppTagShort => this.AppTagProvider.GetAppTagShort(this.AppTag);

        public IAppTagProvider AppTagProvider { get; set; }

        #region Database.SQL 
        [NotMapped]
        public virtual IEnumerable<PmFrameBase> FramesNonData => this.Frames.Where(f => f.L7Pdu == null);

        private IReadOnlyList<NBAR2TaxonomyProtocol> _applicationProtocols;
        private string _appTag;
        private L4Conversation _l4Conversation;
        private L3Conversation _l3Conversation;
        private L7ConversationStatistics _l7ConversationStatistics;
        private L7ConversationStatistics _upConversationStatistic;
        private L7ConversationStatistics _downConversationStatistic;
        
        public virtual ICollection<PmCaptureBase> Captures { get; protected set; }
        #endregion

        #region Hash and Equals
        public override Int32 GetHashCode() => (this.SourceEndPoint).GetHashCode() ^ (this.DestinationEndPoint).GetHashCode()^this.FirstSeen.GetHashCode()^this.LastSeen.GetHashCode();

        public override Boolean Equals(Object obj)
        {
            var l7Conversation = obj as L7Conversation;
            return l7Conversation != null && this.Equals(l7Conversation);
        }

        public Boolean Equals(L7Conversation other)
        {
            var e1 = Equals(this.SourceEndPoint, other.SourceEndPoint);
            var e2 = Equals(this.DestinationEndPoint, other.DestinationEndPoint);
            var e3 = Equals(this.FirstSeen, other.FirstSeen);
            var e4 = Equals(this.LastSeen, other.LastSeen);
            return e1 && e2 &&e3&&e4;
        }
        #endregion
    }
}