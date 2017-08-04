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
using PostSharp.Patterns.Model;

namespace Netfox.Framework.Models
{

    [DataContract]
    [Serializable]
    [PersistentNonInherited]
    public class L3Conversation : ILxConversation, IWindsorContainerChanger
    {
        internal static readonly IPEndPoint NullEndPoint = new IPEndPoint(0, 0);

        [NotMapped]
        private ICollection<PmFrameBase> _nonL4Frames;

        protected L3Conversation()
        {
            this.Captures = new List<PmCaptureBase>();
            this.L4Conversations = new List<L4Conversation>();
            this.L7Conversations = new List<L7Conversation>();
            this.ConversationFlowStatistics = new List<L3ConversationStatistics>();
        }

        public L3Conversation(IWindsorContainer container, IPAddress[] ipAddresses) : this(container,ipAddresses[0], ipAddresses[1]) { }

        public L3Conversation(IWindsorContainer container, IPAddress ipAddress1, IPAddress ipAddress2)
        {
            this.InvestigationWindsorContainer = container;
            this.IPAddress1 = ipAddress1;
            this.IPAddress2 = ipAddress2;
            
            this.Frames = new WeakConccurentCollection<PmFrameBase>();
            this.NonL4Frames = new WeakConccurentCollection<PmFrameBase>();

            this.Captures = new WeakConccurentCollection<PmCaptureBase>();
            this.L4Conversations  = new WeakConccurentCollection<L4Conversation>();
            this.L7Conversations  = new WeakConccurentCollection<L7Conversation>();
            this.ConversationFlowStatistics = new WeakConccurentCollection<L3ConversationStatistics>();

            this.UpConversationStatistic = new L3ConversationStatistics(DaRFlowDirection.up, this);
            this.DownConversationStatistic = new L3ConversationStatistics(DaRFlowDirection.down, this);
        }

        protected L3Conversation(IWindsorContainer container, L3Conversation conversationKey) : this(container,conversationKey.IPAddresses[0], conversationKey.IPAddresses[1]) { }

        [DataMember]
        [MaxLength(16)]
        public byte[] IPAddress1Data { get; private set; }

        [DataMember]
        [NotMapped]
        public IPAddress IPAddress1
        {
            get { return this._ipAddress1 ?? (this._ipAddress1 = new IPAddress(this.IPAddress1Data)); }
            set
            {
                this._ipAddress1 = value;
                this.IPAddress1Data = value.GetAddressBytes();
            }
        }

        [DataMember]
        [NotMapped]
        public IPAddress IPAddress2
        {
            get { return this._ipAddress2 ?? (this._ipAddress2 = new IPAddress(this.IPAddress2Data)); }
            set
            {
                this._ipAddress2 = value;
                this.IPAddress2Data = value.GetAddressBytes();
            }
        }

        [DataMember]
        [MaxLength(16)]
        public byte[] IPAddress2Data { get; private set; }
        [NotMapped]//wtf cannot be deleted, EF generates shitstorm of PmFrames refid is l3conversation in DB
        public ICollection<PmFrameBase> NonL4Frames
        {
            get
            {
                if(this._nonL4Frames != null) { return this._nonL4Frames; }
                this._nonL4Frames = new List<PmFrameBase>(this.Frames.Where(f=> !(f.IpProtocol == IPProtocolType.TCP || f.IpProtocol == IPProtocolType.UDP)));
               return this._nonL4Frames;
            }
            set { this._nonL4Frames = value; }
        }

        [IgnoreAutoChangeNotification]
        public AddressFamily L3ProtocolType => this.IPAddress1?.AddressFamily ?? this.L3ProtocolType;

        [IgnoreAutoChangeNotification]
        public IPAddress[] IPAddresses => new[]
        {
            this.IPAddress1,
            this.IPAddress2
        };

        public virtual ICollection<PmCaptureBase> Captures { get; protected set; } 
        [InverseProperty(nameof(L4Conversation.L3Conversation))]
        public virtual ICollection<L4Conversation> L4Conversations { get; protected set; }
        [InverseProperty(nameof(L7Conversation.L3Conversation))]
        public virtual ICollection<L7Conversation> L7Conversations { get; protected set; }
        [NotMapped]
        public L3ConversationStatistics UpConversationStatistic
        {
            get { return this._upConversationStatistic ?? (this._upConversationStatistic = this.ConversationFlowStatistics.FirstOrDefault(s=>s.FlowDirection == DaRFlowDirection.up)); }
            set
            {
                this._upConversationStatistic = value;
                this.ConversationFlowStatistics.Add(value);
            }
        }
        [NotMapped]
        public L3ConversationStatistics DownConversationStatistic
        {
            get { return this._downConversationStatistic ?? (this._downConversationStatistic = this.ConversationFlowStatistics.FirstOrDefault(s => s.FlowDirection == DaRFlowDirection.down)); }
            set
            {
                this._downConversationStatistic = value;
                this.ConversationFlowStatistics.Add(value);
            }
        }
        [InverseProperty(nameof(global::Netfox.Framework.Models.L3ConversationStatistics.L3Conversation))]
        public virtual ICollection<L3ConversationStatistics> ConversationFlowStatistics { get; set; }

        public L3ConversationStatistics ConversationStats => this.L3ConversationStatistics;

        [IgnoreAutoChangeNotification]
        public IEnumerable<PmFrameBase> DataFrames => this.L7Conversations.SelectMany(c => c.FramesData);

        [IgnoreAutoChangeNotification]
        public IPEndPoint DestinationEndPoint => new IPEndPoint(this.IPAddress2, 0);

        [IgnoreAutoChangeNotification]
        public IPEndPoint SourceEndPoint => new IPEndPoint(this.IPAddress1, 0);

        [IgnoreAutoChangeNotification]
        public IReadOnlyList<NBAR2TaxonomyProtocol> ApplicationProtocols { get; } = new List<NBAR2TaxonomyProtocol>();

        ILxConversationStatistics ILxConversation.ConversationStats => this.ConversationStats;

        ILxConversationStatistics ILxConversation.DownConversationStatistic => this.DownConversationStatistic;

        public IEnumerable<PmFrameBase> DownFlowFrames => this.Frames.Where(f => this.DeterminFrameDirection(f) == DaRFlowDirection.down);

        private DateTime? _firstSeen;
        private DateTime? _lasttSeen;
        public DateTime FirstSeen
        {
            get { return (DateTime) (this._firstSeen ??(this._firstSeen =this.L3ConversationStatistics.FirstSeen)); }
            set { this._firstSeen = value; }
        }

        public DateTime LastSeen
        {
            get { return (DateTime)(this._lasttSeen ?? (this._lasttSeen = this.L3ConversationStatistics.LastSeen)); }
            set { this._lasttSeen = value; }
        }

        [DataMember]
        public string Name => this._name ?? (this._name = $"{this?.SourceEndPoint} - {this?.DestinationEndPoint}");

        [IgnoreAutoChangeNotification]
        public IPProtocolType ProtocolType
        {
            get
            {
                switch(this.L3ProtocolType)
                {
                    case AddressFamily.InterNetwork:
                        return IPProtocolType.IP;
                    case AddressFamily.InterNetworkV6:
                        return IPProtocolType.IPV6;
                    default:
                        return IPProtocolType.NONE;
                }
            }
        }

        ILxConversationStatistics ILxConversation.UpConversationStatistic => this.UpConversationStatistic;
        public IEnumerable<PmFrameBase> UpFlowFrames => this.Frames.Where(f => this.DeterminFrameDirection(f) == DaRFlowDirection.up);

        public DaRFlowDirection DeterminFrameDirection(PmFrameBase frame)
        {
            if(this._firstFrame == null) this._firstFrame = this.Frames.FirstOrDefault();
            if(this._firstFrame == null) return DaRFlowDirection.non;

            return frame.SourceEndPoint.Equals(this._firstFrame.SourceEndPoint)? DaRFlowDirection.up : DaRFlowDirection.down;
        }
        private PmFrameBase _firstFrame;
        [Key]
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        #region Implementation of IWindsorContainerChanger
        [NotMapped]
        public IWindsorContainer InvestigationWindsorContainer { get; set; }
        #endregion

        
        private String _name;

        IEnumerable<PmFrameBase> ILxConversation.Frames => this.Frames;

       [NotMapped]
        public ICollection<PmFrameBase> Frames
        {
            get
            {
                if(this._frames == null)
                {
                    Expression<Func<PmFrameBase, Boolean>> func = f => f.L3ConversationRefId == this.Id;
                    this._frames = this.InvestigationWindsorContainer?.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>(new
                    { 
                        filter = func
                    });
                }
                return this._frames;
            }
            private set { this._frames = value; }
        }
        
        private IPAddress _ipAddress1;
        private IPAddress _ipAddress2;
        [NotMapped]
        private ICollection<PmFrameBase> _frames;
        private L3ConversationStatistics _l3ConversationStatistics;
        private L3ConversationStatistics _upConversationStatistic;
        private L3ConversationStatistics _downConversationStatistic;
        

        [NotMapped]
        public L3ConversationStatistics L3ConversationStatistics => this._l3ConversationStatistics ?? (this._l3ConversationStatistics = new L3ConversationStatistics(this.UpConversationStatistic,this.DownConversationStatistic));
        
        #region Hash and Equals
        public override Int32 GetHashCode() => (this.IPAddresses[0] ?? NullEndPoint.Address).GetHashCode()^(this.IPAddresses[1] ?? NullEndPoint.Address).GetHashCode();

        public override Boolean Equals(Object obj)
        {
            var l3Conversation = obj as L3Conversation;
            return l3Conversation != null && this.Equals(l3Conversation);
        }

        public Boolean Equals(L3Conversation other)
        {
            var e1 = Equals(this.IPAddresses[0], other.IPAddresses[0]);
            var e2 = Equals(this.IPAddresses[1], other.IPAddresses[1]);
            var e3 = Equals(this.IPAddresses[0], other.IPAddresses[1]);
            var e4 = Equals(this.IPAddresses[1], other.IPAddresses[0]);
            return (e1 && e2) || (e3 && e4);
        }
        #endregion
    }
}