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
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;

namespace Netfox.Framework.CaptureProcessor.L3L4ConversationTracking
{
    public class L3ConversationExtended : L3Conversation
    {
        public L3ConversationExtended(IWindsorContainer container, IPAddress ipAddress1, IPAddress ipAddress2) : base(container,ipAddress1, ipAddress2) {}
        public L3ConversationExtended(IWindsorContainer container, L3Conversation conversationKey) : base(container, conversationKey) {}
        public ConcurrentObservableHashSet<PmCaptureBase> CapturesHashSet { get; } = new ConcurrentObservableHashSet<PmCaptureBase>();
    }
}