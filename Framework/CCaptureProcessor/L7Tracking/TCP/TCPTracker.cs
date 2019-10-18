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
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.TCP
{
    public class TCPTracker : IL7ConversationTracker
    {
        private static readonly TimeSpan TCPSessionAliveTimeout = new TimeSpan(0, 0, 0, 600); //s
        private readonly Int32 _tcpSessionMaxDataLooseOnTCPLoop = 3800; //B

        public TCPTracker(IFlowStoreFactory flowStoreFactory, L4Conversation l4Conversation)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStoreFactory.Create(l4Conversation, TCPSessionAliveTimeout);
            this.UpFlowTCPFlowTracker = new TCPFlowTracker(this.L4Conversation, this.FlowStore, TCPSessionAliveTimeout, this._tcpSessionMaxDataLooseOnTCPLoop, DaRFlowDirection.up);
            this.DownFlowTCPFlowTracker = new TCPFlowTracker(this.L4Conversation, this.FlowStore, TCPSessionAliveTimeout, this._tcpSessionMaxDataLooseOnTCPLoop, DaRFlowDirection.down);
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
            switch(this.L4Conversation.DeterminFrameDirection(frame))
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