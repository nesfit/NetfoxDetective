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

namespace Netfox.Snoopers.SnooperMessenger.Models.Events
{
    public class MessengerEventTyping : MessengerEventBase
    {
        public string To { get; private set; }
        public int State { get; private set; }
        private MessengerEventTyping() : base() {}

        public MessengerEventTyping(SnooperExportBase exportBase, Byte[] payload) : base(exportBase)
        {
            /*  
            {
                "to" : "100010419973288",
                "state" : 1
            }
            */
            var jsonPayload = DecompressJSONPayload(payload);
            this.To = (string)jsonPayload["to"];
            this.State = (int)jsonPayload["state"];
        }

        public override string ToString() { return "Typing:" + this.State + " To:" + this.To; }
    }
}