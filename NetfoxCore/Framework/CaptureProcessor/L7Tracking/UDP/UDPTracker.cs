using System;
using System.Collections.Generic;
using Netfox.Core.Enums;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;


namespace Netfox.Framework.CaptureProcessor.L7Tracking.UDP
{
    internal class UDPTracker : IL7ConversationTracker
    {
        private static readonly TimeSpan UDPSessionAliveTimeout = new TimeSpan(0, 0, 0, 600); //s

        public UDPTracker(IFlowStoreFactory flowStoreFactory, L4Conversation l4Conversation)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStoreFactory.Create(l4Conversation, UDPSessionAliveTimeout);
            this.UpFlowUDPFlowTracker = new UDPFlowTracker(this.L4Conversation, this.FlowStore, UDPSessionAliveTimeout,
                DaRFlowDirection.up);
            this.DownFlowUDPFlowTracker = new UDPFlowTracker(this.L4Conversation, this.FlowStore,
                UDPSessionAliveTimeout, DaRFlowDirection.down);
        }

        public FlowStore FlowStore { get; }

        public UDPFlowTracker DownFlowUDPFlowTracker { get; }

        public UDPFlowTracker UpFlowUDPFlowTracker { get; }

        public IEnumerable<L7Conversation> ProcessPmFrame(PmFrameBase frame)
        {
            switch (this.L4Conversation.DeterminFrameDirection(frame))
            {
                case DaRFlowDirection.up:
                    return this.UpFlowUDPFlowTracker.ProcessPmFrame(frame);
                case DaRFlowDirection.down:
                    return this.DownFlowUDPFlowTracker.ProcessPmFrame(frame);
                case DaRFlowDirection.non:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Implementation of IL7ConversationTracker

        public L4Conversation L4Conversation { get; }

        #region Implementation of IL7ConversationTracker

        public IEnumerable<L7Conversation> Complete()
        {
            this.UpFlowUDPFlowTracker.Complete();
            this.DownFlowUDPFlowTracker.Complete();
            return this.FlowStore.PairFlowsAndCreateAndAddConversations(true);
        }

        #endregion

        #endregion
    }
}