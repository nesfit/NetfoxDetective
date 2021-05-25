// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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

using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperMinecraft.Models
{
    public class SnooperExportedMinecraftMessage : SnooperExportedObjectBase, IChatMessage
    {
        public string Sender { get; set; }  = string.Empty;
        public string Receiver { get; set; }  = string.Empty;
        public string Text { get; set; } = string.Empty; // pure text of message
        public string Message { get; set; } = string.Empty; // complete unparsed message
        public MinecraftMessageType Type { get; set; } = MinecraftMessageType.Broadcast; // broadcast is default and used mainly
        private SnooperExportedMinecraftMessage() : base() { } //EF
        public SnooperExportedMinecraftMessage(SnooperExportBase exportBase) : base(exportBase) { }
    }
}