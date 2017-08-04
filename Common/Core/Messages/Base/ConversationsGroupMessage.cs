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

namespace Netfox.Core.Messages.Base
{
    public class ConversationsGroupMessage : DetectiveMessage
    {
        public enum MessageType
        {
            GroupVmSelected
        }

        private ConversationsGroupMessage() { this.Sender = "ConversationsGroupMessage"; }
        public MessageType Type { get; set; }
        public object ConversationsGroupVm { get; set; }

        public static void SendConversationsGroupMessage(object conversationsGroupVm, MessageType type)
        {
            var newConversationsGroupMessage = new ConversationsGroupMessage
            {
                ConversationsGroupVm = conversationsGroupVm,
                Type = type
            };

            AsyncSendMessage(newConversationsGroupMessage);
        }
    }
}