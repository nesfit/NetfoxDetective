// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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

using System.Collections.Generic;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperWebmails.Models.WebmailEvents
{
    /// <summary>
    ///     List Folder webmail event.
    ///     In best case it contains List of MailMsgs. In worst case it contains unparsed string content of message.
    /// </summary>
   public class EventListFolder : WebmailEventBase
    {
        public string UnparsedContent { get; private set; }
        private EventListFolder() : base() { } //EF

        public EventListFolder(SnooperExportBase exportBase, List<MailMsg> msgList) : base(exportBase) { this.Messages = msgList; }

        public EventListFolder(SnooperExportBase exportBase, string content) : base(exportBase)
        {
            this.UnparsedContent = content;
            this.Messages = new List<MailMsg>();
        }

        public List<MailMsg> Messages { get; private set; }
    }
}