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

namespace Netfox.SnooperMessenger.Models.Events
{
    public class MessengerEventForegroundState : MessengerEventBase
    {
        public int ForegroundState { get; private set; }
        public int KeepaliveTimeout { get; private set; }
        private MessengerEventForegroundState() { } //EF

        public MessengerEventForegroundState(SnooperExportBase exportBase, Byte[] payload) : base(exportBase)
        {
            /*  
            {
                "foreground" : 1,
                "keepalive_timeout" : 60
            }
            */
            var jsonPayload = DecompressJSONPayload(payload);
            this.ForegroundState = (int) jsonPayload["foreground"];
            this.KeepaliveTimeout = (int) jsonPayload["keepalive_timeout"];
        }

        public override string ToString() { return "ForegroundState:" + this.ForegroundState; }
    }
}