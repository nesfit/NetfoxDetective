using System;
using System.Collections.Generic;
using Netfox.Core.Enums;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.TCP
{
    internal class TCPTracker : IL7ConversationTracker
    {
        private static readonly TimeSpan TCPSessionAliveTimeout = new TimeSpan(0, 0, 0, 600); //s
        private readonly Int32 _tcpSessionMaxDataLooseOnTCPLoop = 3800; //B

        public TCPTracker(IFlowStoreFactory flowStoreFactory, L4Conversation l4Conversation)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStoreFactory.Create(l4Conversation, TCPSessionAliveTimeout);
            this.UpFlowTCPFlowTracker = new TCPFlowTracker(this.L4Conversation, this.FlowStore, TCPSessionAliveTimeout,
                this._tcpSessionMaxDataLooseOnTCPLoop, DaRFlowDirection.up);
            this.DownFlowTCPFlowTracker = new TCPFlowTracker(this.L4Conversation, this.FlowStore,
                TCPSessionAliveTimeout, this._tcpSessionMaxDataLooseOnTCPLoop, DaRFlowDirection.down);
        }

        public TCPFlowTracker DownFlowTCPFlowTracker { get; }

        public TCPFlowTracker UpFlowTCPFlowTracker { get; }

        public FramesSequenceNumberSortedCollection CurrentTCPSession { get; set; }
        private FlowStore FlowStore { get; }

        public IEnumerable<L7Conversation> Complete()
        {
            this.UpFlowTCPFlowTracker.Complete();
            this.DownFlowTCPFlowTracker.Complete();
            return this.FlowStore.PairFlowsAndCreateAndAddConversations(true);
        }

        public L4Conversation L4Conversation { get; }

        public IEnumerable<L7Conversation> ProcessPmFrame(PmFrameBase frame)
        {
            switch (this.L4Conversation.DeterminFrameDirection(frame))
            {
                case DaRFlowDirection.up:
                    return this.UpFlowTCPFlowTracker.ProcessPmFrame(frame);
                case DaRFlowDirection.down:
                    return this.DownFlowTCPFlowTracker.ProcessPmFrame(frame);
                case DaRFlowDirection.non:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}