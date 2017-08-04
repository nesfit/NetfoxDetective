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
using Netfox.Core.Enums;
using Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.TCP
{
    internal class TCPFlowTracker
    {
        private readonly TimeSpan _tcpSessionAliveTimeout; // = 600; //s

        public TCPFlowTracker(
            L4Conversation l4Conversation,
            FlowStore flowStore,
            TimeSpan tcpSessionAliveTimeout,
            int tcpSessionMaxDataLooseOnTCPLoop,
            DaRFlowDirection flowDirection)
        {
            this.FlowStore = flowStore;
            this.FlowDirection = flowDirection;
            this.L4Conversation = l4Conversation;
            this._tcpSessionAliveTimeout = tcpSessionAliveTimeout;
            this.TCPFlowReassembler = new TCPFlowReassembler(this.L4Conversation, this.FlowStore, this._tcpSessionAliveTimeout, tcpSessionMaxDataLooseOnTCPLoop);
        }

        private FramesSequenceNumberSortedCollection CurrentTCPFlow { get; set; } = new FramesSequenceNumberSortedCollection();
        private FlowStore FlowStore { get; }
        private DaRFlowDirection FlowDirection { get; }
        private TCPFlowReassembler TCPFlowReassembler { get; }
        private L4Conversation L4Conversation { get; }

        public void Complete() { this.ProcessCurrentTCPFlow(); }

        public IEnumerable<L7Conversation> ProcessPmFrame(PmFrameBase frame)
        {
            lock(this.TCPFlowReassembler)
            {
                IEnumerable<L7Conversation> l7Conversations = null;
                if(this.CurrentTCPFlow.LastSeen == null || frame.TimeStamp.Subtract((DateTime) this.CurrentTCPFlow.LastSeen).Duration() < this._tcpSessionAliveTimeout) {}
                else
                {
                    this.ProcessCurrentTCPFlow();
                    this.CurrentTCPFlow = new FramesSequenceNumberSortedCollection();
                    l7Conversations = this.FlowStore.PairFlowsAndCreateAndAddConversations();
                }
                this.CurrentTCPFlow.Add(frame);
                return l7Conversations;
            }
        }

        private void ProcessCurrentTCPFlow()
        {
            var tcpFlow = this.CurrentTCPFlow;
            this.CurrentTCPFlow = null;
            this.TCPFlowReassembler.ProcessTCPSession(tcpFlow, this.FlowDirection);
        }
    }
}