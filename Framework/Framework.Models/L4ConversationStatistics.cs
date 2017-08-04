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

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Framework.Models
{
    public class L4ConversationStatistics : L3ConversationStatistics
    {
        protected L4ConversationStatistics() { }
        
        internal L4ConversationStatistics(L4ConversationStatistics upFlowStatistic, L4ConversationStatistics downFlowStatistic):base(upFlowStatistic.L4Conversation)
        {
            this.UPFlowStatistic = upFlowStatistic;
            this.DownFlowStatistic = downFlowStatistic;
            this.L4Conversation = upFlowStatistic.L4Conversation;
            this.L4ConversationRefId = upFlowStatistic.L4ConversationRefId;
        }

        internal L4ConversationStatistics(DaRFlowDirection flowDirection, L4Conversation l4Conversation):base(flowDirection,l4Conversation)
        {
            this.L4Conversation = l4Conversation;
            this.L4ConversationRefId = l4Conversation.Id;
        }

        protected L4ConversationStatistics(DaRFlowDirection flowDirection, ILxConversation conversation) : base(flowDirection, conversation){ }

        protected L4ConversationStatistics(ILxConversation l7Conversation):base(l7Conversation){}

        public Guid? L4ConversationRefId { get; set; }
        [ForeignKey(nameof(L4ConversationRefId))]
        public virtual L4Conversation L4Conversation { get; set; }

        #region Overrides of Object
        public override string ToString() { return base.ToString(); }

        public override bool Equals(object obj)
        {
            var conversationStatistics = obj as ConversationStatisticsBase;
            return conversationStatistics != null && this.Equals(conversationStatistics);
        }
        public override int GetHashCode() { return this.Id.GetHashCode(); }
#endregion
    }
}
