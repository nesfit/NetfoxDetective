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
using System.Diagnostics;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Framework.Models
{
    public class L3ConversationStatistics : ConversationStatisticsBase
    {
        protected L3ConversationStatistics() { }
        protected L3ConversationStatistics(ILxConversation l4Conversation):base(l4Conversation) { }

        internal L3ConversationStatistics(L3ConversationStatistics upFlowStatistic, L3ConversationStatistics downFlowStatistic):base(upFlowStatistic?.L3Conversation ?? downFlowStatistic?.L3Conversation)
        {
            if(upFlowStatistic == null || downFlowStatistic == null) Debugger.Break();
            this.UPFlowStatistic = upFlowStatistic;
            this.DownFlowStatistic = downFlowStatistic;
            this.L3Conversation = upFlowStatistic.L3Conversation;
            this.L3ConversationRefId = upFlowStatistic.L3ConversationRefId;
        }

        public L3ConversationStatistics(DaRFlowDirection flowDirection, L3Conversation conversation):base(flowDirection, conversation)
        {
            this.L3Conversation = conversation;
            this.L3ConversationRefId = conversation.Id;
        }
        protected L3ConversationStatistics(DaRFlowDirection flowDirection, ILxConversation conversation) : base(flowDirection, conversation) { }

        public Guid? L3ConversationRefId { get; set; }
        [ForeignKey(nameof(L3ConversationRefId))]
        public virtual L3Conversation L3Conversation { get; set; }
    }
}
