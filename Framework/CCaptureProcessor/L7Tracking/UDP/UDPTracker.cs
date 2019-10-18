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
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.UDP
{
    public class UDPTracker : IL7ConversationTracker
    {
        private static readonly TimeSpan UDPSessionAliveTimeout = new TimeSpan(0, 0, 0, 600); //s

        public UDPTracker(IFlowStoreFactory flowStoreFactory,L4Conversation l4Conversation)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStoreFactory.Create(l4Conversation, UDPSessionAliveTimeout);
            this.UpFlowUDPFlowTracker = new UDPFlowTracker(this.L4Conversation, this.FlowStore, UDPSessionAliveTimeout, DaRFlowDirection.up);
            this.DownFlowUDPFlowTracker = new UDPFlowTracker(this.L4Conversation, this.FlowStore, UDPSessionAliveTimeout, DaRFlowDirection.down);
        }

        public FlowStore FlowStore { get; }

        public UDPFlowTracker DownFlowUDPFlowTracker { get; }

        public UDPFlowTracker UpFlowUDPFlowTracker { get; }

        public IEnumerable<L7Conversation> ProcessPmFrame(PmFrameBase frame)
        {
            switch(this.L4Conversation.DeterminFrameDirection(frame))
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