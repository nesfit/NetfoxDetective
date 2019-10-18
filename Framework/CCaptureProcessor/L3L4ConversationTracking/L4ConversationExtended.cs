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
using System.Diagnostics;
using System.Net;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.L3L4ConversationTracking
{
    public class L4ConversationExtended : L4Conversation
    {
        public L4ConversationExtended(IWindsorContainer container, IL7ConversationTrackerFactory conversationTrackerFactory, IPProtocolType ipProtocol, L3Conversation l3Conversation, IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint, long l4FlowMTU)
            : base(container,ipProtocol, l3Conversation, sourceEndPoint, destinationEndPoint, l4FlowMTU)
        {
            switch (this.L4ProtocolType)
            {
                case IPProtocolType.TCP: this.L7ConversationTracker = conversationTrackerFactory.CreateTCPTracker(this); break;
                case IPProtocolType.UDP: this.L7ConversationTracker = conversationTrackerFactory.CreateUDPTracker(this); break;
                default:
                    Debugger.Break();
                    throw new ArgumentOutOfRangeException();
            }
        }
        public IL7ConversationTracker L7ConversationTracker { get; }
        public ConcurrentObservableHashSet<PmCaptureBase> CapturesHashSet { get; } = new ConcurrentObservableHashSet<PmCaptureBase>();

    }
}