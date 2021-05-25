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
using Netfox.Snoopers.SnooperSPDY.Models;

namespace Netfox.Snoopers.SnooperTwitter.Models.Events
{
    public class TwitterEventSendMessage : TwitterEventBase
    {
        public string Recipient { get; private set; }
        public string Sender { get; private set; }
        public string Text { get; private set; }
        private TwitterEventSendMessage() { }
        public TwitterEventSendMessage(SnooperExportBase exportBase, SPDYMsg spdyMsg) : base(exportBase, spdyMsg)
        {
            var spdyMsgContent = spdyMsg.Content;
            if(spdyMsgContent == null) throw  new Exception("Missing message body");

            var fields = spdyMsgContent.GetFormUrlEncodedData();

            // ConversationID is in format of RecipientID-SenderID
            var conversationId = fields["conversation_id"];
            var participants = conversationId.Split('-');
            this.Recipient = participants[0];
            this.Sender = participants[1];

            this.Text = fields["text"];
        }
    }
}