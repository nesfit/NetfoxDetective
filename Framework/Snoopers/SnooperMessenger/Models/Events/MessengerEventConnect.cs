// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperMessenger.Protocol;

namespace Netfox.SnooperMessenger.Models.Events
{
    public class MessengerEventConnect : MessengerEventBase
    {
        public string UserId { get; protected set; }

        private MessengerEventConnect() { } //EF

        public MessengerEventConnect(SnooperExportBase exportBase, Byte[] payload) : base(exportBase)
        {
            var connectMessage = new ConnectMessage();
            connectMessage.Read(CompactProtocolForPayload(DecompressPayload(payload), 0));

            this.UserId = connectMessage.ClientInfo.UserId.ToString();
        }

        public override string ToString() { return "UserConnected:" + this.UserId; }
    }
}