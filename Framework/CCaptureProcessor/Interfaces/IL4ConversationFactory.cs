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

using System.Net;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.Models;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.Interfaces {
    public interface IL4ConversationFactory
    {L4ConversationExtended Create(IPProtocolType ipProtocol, L3Conversation l3Conversation, IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint, long l4FlowMTU);
    }
}