﻿// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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

using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperMessenger.Protocol;

namespace Netfox.Snoopers.SnooperMessenger.Models.Events
{
    public class MessengerEventReceiveMessage : MessengerEventBase
    {
        public string From { get; private set; }
        public string Body { get; private set; }

        private MessengerEventReceiveMessage() { } //EF

        public MessengerEventReceiveMessage(SnooperExportBase exportBase, MNMessagesSyncDeltaNewMessage deltaNewMessage) : base(exportBase)
        {
            this.From = deltaNewMessage.MessageMetadata.ThreadKey.OtherUserFbId.ToString();
            this.Body = deltaNewMessage.Body;
        }

        public override string ToString() { return "From:" + this.From + " Body:" + this.Body; }
    }
}