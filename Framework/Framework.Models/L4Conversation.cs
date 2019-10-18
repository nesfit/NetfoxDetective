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
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using Castle.Core;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Core.Database;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.NBARDatabase;
using PacketDotNet;

namespace Netfox.Framework.Models
{
    //public class PacketWithNoL4Header : ArgumentException { }

    /// <summary> A bt bidirectional flow.</summary>
    [DataContract]
    [Serializable]
    [PersistentNonInherited]
    public class L4Conversation : ILxConversation, IWindsorContainerChanger
    {
        public Object L4FlowMTULock = new Object();
        private IPEndPoint _destinationEndPoint;
        private ICollection<L7Conversation> _l7Conversations;
        private IPEndPoint _sourceEndPoint;
        private L3Conversation _l3Conversation;
        private L4ConversationStatistics _l4ConversationStatistics;
        private L4ConversationStatistics _upConversationStatistic;
        private L4ConversationStatistics _downConversationStatistic;

        protected L4Conversation() { }

        public L4Conversation(IWindsorContainer container, IPProtocolType ipProtocol, L3Conversation l3Conversation, IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint, long l4FlowMTU)
        {
            this.InvestigationWindsorContainer = container;
            this.L4ProtocolType = ipProtocol;
            this.L3Conversation = l3Conversation;
            this.SourceEndPoint = sourceEndPoint;
            this.DestinationEndPoint = destinationEndPoint;
            this.L4FlowMTU = l4FlowMTU;
            this.Frames = new WeakConccurentCollection<PmFrameBase>();

            this.Captures = new WeakConccurentCollection<PmCaptureBase>();
            this.L7Conversations = new WeakConccurentCollection<L7Conversation>();
            this.ConversationFlowStatistics = new WeakConccurentCollection<L4ConversationStatistics>();

            this.UpConversationStatistic  = new L4ConversationStatistics(DaRFlowDirection.up, this);
            this.DownConversationStatistic = new L4ConversationStatistics(DaRFlowDirection.down, this);
        }

        [NotMapped]
        public L4ConversationStatistics UpConversationStatistic
        {
            get { return this._upConversationStatistic ?? (this._upConversationStatistic = this.ConversationFlowStatistics.FirstOrDefault(s => s.FlowDirection == DaRFlowDirection.up)); }
            set
            {
                this._upConversationStatistic = value;
                this.ConversationFlowStatistics.Add(value);
            }
        }
        [NotMapped]
        public L4ConversationStatistics DownConversationStatistic
        {
            get { return this._downConversationStatistic ?? (this._downConversationStatistic = this.ConversationFlowStatistics.FirstOrDefault(s => s.FlowDirection == DaRFlowDirection.down)); }
            set
            {
                this._downConversationStatistic = value;
                this.ConversationFlowStatistics.Add(value);
            }
        }
        [InverseProperty(nameof(global::Netfox.Framework.Models.L4ConversationStatistics.L4Conversation))]
        public virtual ICollection<L4ConversationStatistics> ConversationFlowStatistics { get; protected set; } 
        
        public L4ConversationStatistics ConversationStats => this.L4ConversationStatistics;

        public Boolean IsWithoutHeader { get; } = false;

        public Guid L3ConversationRefId { get; private set; }

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

        [DataMember]
        public Int64 L4FlowMTU { get; set; }

        public AddressFamily L3ProtocolType => this.SourceEndPoint?.AddressFamily ?? AddressFamily.Ipx;

        [DataMember]
        public IPProtocolType L4ProtocolType { get; set; }

        public IPProtocolType ProtocolType => this.L4ProtocolType;
        [NotMapped]
        public ICollection<L7Conversation> L7Conversations
        {
            get
            {
                if(this._l7Conversations != null) { return this._l7Conversations; }

                Expression<Func<L7Conversation, Boolean>> func = f => f.L4ConversationRefId == this.Id;
                this._l7Conversations = this.InvestigationWindsorContainer?.Resolve<VirtualizingObservableDBSetPagedCollection<L7Conversation>>(new
                {
                    filter = func
                });
                return this._l7Conversations;
            }
            set { this._l7Conversations = value; }
        }

        [DataMember]
        public Int32 DestinationEndPointPort { get; private set; }

        [DataMember, MaxLength(16)]
        public byte[] DestinationEndPointIPAddressData { get; private set; }

        [DataMember, MaxLength(16)]
        public byte[] SourceEndPointIPAddressData { get; private set; }

        [DataMember]
        public Int32 SourceEndPointPort { get; private set; }
        [NotMapped]
        public L4ConversationStatistics L4ConversationStatistics => this._l4ConversationStatistics ?? (this._l4ConversationStatistics = new L4ConversationStatistics(this.UpConversationStatistic, this.DownConversationStatistic));

        public Guid? CaptureL4RefId { get; set; }
        [ForeignKey(nameof(CaptureL4RefId))]
        public CaptureL4 CaptureL4 { get; set; }
        
        public virtual ICollection<PmCaptureBase> Captures { get; protected set; } 

        [NotMapped]
        public IPEndPoint DestinationEndPoint
        {
            get
            {
                return this._destinationEndPoint ?? (this._destinationEndPoint = new IPEndPoint(new IPAddress(this.DestinationEndPointIPAddressData), this.DestinationEndPointPort));
            }
            set
            {
                this._destinationEndPoint = value;
                this.DestinationEndPointIPAddressData = value.Address.GetAddressBytes();
                this.DestinationEndPointPort =  value.Port;
            }
        }

        [NotMapped]
        public IPEndPoint SourceEndPoint
        {
            get { return this._sourceEndPoint ?? (this._sourceEndPoint = new IPEndPoint(new IPAddress(this.SourceEndPointIPAddressData), this.SourceEndPointPort)); }
            set
            {
                this._sourceEndPoint = value;
                this.SourceEndPointIPAddressData = value.Address.GetAddressBytes();
                this.SourceEndPointPort = value.Port;
            }
        }

        public IReadOnlyList<NBAR2TaxonomyProtocol> ApplicationProtocols => new List<NBAR2TaxonomyProtocol>();

        ILxConversationStatistics ILxConversation.ConversationStats => this.ConversationStats;

        ILxConversationStatistics ILxConversation.DownConversationStatistic => this.DownConversationStatistic;

        public IEnumerable<PmFrameBase> DownFlowFrames =>this.Frames.Where(f => this.DeterminFrameDirection(f) == DaRFlowDirection.down);

        private DateTime? _firstSeen;
        private DateTime? _lasttSeen;

        public DateTime FirstSeen
        {
            get { return (DateTime)(this._firstSeen ?? (this._firstSeen = this.L4ConversationStatistics.FirstSeen)); }
            set { this._firstSeen = value; }
        }

        public DateTime LastSeen
        {
            get { return (DateTime)(this._lasttSeen ?? (this._lasttSeen = this.L4ConversationStatistics.LastSeen)); }
            set { this._lasttSeen = value; }
        }

        IEnumerable<PmFrameBase> ILxConversation.Frames => this.Frames;
        
        public virtual ICollection<PmFrameBase> Frames { get; protected set; }

        public string Name => this.ToString();

        IPProtocolType ILxConversation.ProtocolType => this.L4ProtocolType;

        ILxConversationStatistics ILxConversation.UpConversationStatistic => this.UpConversationStatistic;

        public IEnumerable<PmFrameBase> UpFlowFrames => this.Frames.Where(f => this.DeterminFrameDirection(f) == DaRFlowDirection.up);

        public DaRFlowDirection DeterminFrameDirection(PmFrameBase frame) { return frame.DstPort == this.DestinationEndPointPort? DaRFlowDirection.up : DaRFlowDirection.down; }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        #region Implementation of IWindsorContainerChanger
        [NotMapped]
        [DoNotWire]
        public IWindsorContainer InvestigationWindsorContainer { get; set; }
        #endregion

        #region Overrides of Object
        public override Int32 GetHashCode() => (this.SourceEndPoint).GetHashCode() ^ (this.DestinationEndPoint).GetHashCode();

        public override Boolean Equals(Object obj)
        {
            var l4Conversation = obj as L4Conversation;
            return l4Conversation != null && this.Equals(l4Conversation);
        }

        public Boolean Equals(L4Conversation other)
        {
            var e1 = Equals(this.SourceEndPoint, other.SourceEndPoint);
            var e2 = Equals(this.DestinationEndPoint, other.DestinationEndPoint);
            return (e1 && e2);
        }
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override String ToString()
        {
            if(this.SourceEndPoint == null) { return base.ToString(); }
            if(this.UpFlowFrames == null) { return base.ToString(); }
            var sb = new StringBuilder();
            sb.Append("IP version> ")
                .AppendLine(this.L3ProtocolType.ToString())
                .Append("L4 version> ")
                .AppendLine(this.L4ProtocolType.ToString())
                .Append("Src> ")
                .Append(this.SourceEndPoint.Address)
                .Append(" ")
                .AppendLine(this.SourceEndPoint.Port.ToString())
                .Append("Dst> ")
                .Append(this.DestinationEndPoint.Address)
                .Append(" ")
                .AppendLine(this.DestinationEndPoint.Port.ToString())
                .Append("U>")
                .AppendLine(this.UpFlowFrames.Count().ToString())
                .Append("D>")
                .AppendLine(this.DownFlowFrames.Count().ToString());
            return sb.ToString();
        }
        #endregion
    }
}