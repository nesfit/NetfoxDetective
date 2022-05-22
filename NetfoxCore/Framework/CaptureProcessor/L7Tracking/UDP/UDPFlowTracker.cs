﻿using System;
using System.Collections.Generic;
using Netfox.Core.Enums;
using Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.UDP
{
    internal class UDPFlowTracker
    {
        private readonly TimeSpan _udpSessionAliveTimeout; // = 600; //s

        public UDPFlowTracker(L4Conversation l4Conversation, FlowStore flowStore, TimeSpan udpSessionAliveTimeout,
            DaRFlowDirection flowDirection)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStore;
            this._udpSessionAliveTimeout = udpSessionAliveTimeout;
            this.UDPFlowReassembler = new UDPFlowReassembler(this.L4Conversation, this.FlowStore, flowDirection,
                this._udpSessionAliveTimeout);
        }

        public UDPFlowReassembler UDPFlowReassembler { get; }

        public L4Conversation L4Conversation { get; set; }
        public FlowStore FlowStore { get; }

        private FramesFirstSeenSortedCollection CurrentUDPFlow { get; set; } = new FramesFirstSeenSortedCollection();

        public void Complete()
        {
            this.ProcessCurrentUDPFlow();
        }

        public IEnumerable<L7Conversation> ProcessPmFrame(PmFrameBase frame)
        {
            lock (this.UDPFlowReassembler)
            {
                IEnumerable<L7Conversation> l7Conversations = null;
                if (this.CurrentUDPFlow.LastSeen == null ||
                    frame.TimeStamp.Subtract((DateTime) this.CurrentUDPFlow.LastSeen).Duration() <
                    this._udpSessionAliveTimeout)
                {
                }
                else
                {
                    this.ProcessCurrentUDPFlow();
                    this.CurrentUDPFlow = new FramesFirstSeenSortedCollection();
                    l7Conversations = this.FlowStore.PairFlowsAndCreateAndAddConversations();
                }

                this.CurrentUDPFlow.Add(frame);
                return l7Conversations;
            }
        }

        private void ProcessCurrentUDPFlow()
        {
            var udpFlow = this.CurrentUDPFlow;
            this.CurrentUDPFlow = null;
            this.UDPFlowReassembler.ProcessUDPFlow(udpFlow);
        }
    }
}