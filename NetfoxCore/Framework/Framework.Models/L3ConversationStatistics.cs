using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Framework.Models
{
    public class L3ConversationStatistics : ConversationStatisticsBase
    {
        protected L3ConversationStatistics()
        {
        }

        protected L3ConversationStatistics(ILxConversation l4Conversation) : base(l4Conversation)
        {
        }

        internal L3ConversationStatistics(L3ConversationStatistics upFlowStatistic,
            L3ConversationStatistics downFlowStatistic) : base(upFlowStatistic?.L3Conversation ??
                                                               downFlowStatistic?.L3Conversation)
        {
            if (upFlowStatistic == null || downFlowStatistic == null) Debugger.Break();
            this.UPFlowStatistic = upFlowStatistic;
            this.DownFlowStatistic = downFlowStatistic;
            this.L3Conversation = upFlowStatistic.L3Conversation;
            this.L3ConversationRefId = upFlowStatistic.L3ConversationRefId;
        }

        public L3ConversationStatistics(DaRFlowDirection flowDirection, L3Conversation conversation) : base(
            flowDirection, conversation)
        {
            this.L3Conversation = conversation;
            this.L3ConversationRefId = conversation.Id;
        }

        protected L3ConversationStatistics(DaRFlowDirection flowDirection, ILxConversation conversation) : base(
            flowDirection, conversation)
        {
        }

        public Guid? L3ConversationRefId { get; set; }

        [ForeignKey(nameof(L3ConversationRefId))]
        public virtual L3Conversation L3Conversation { get; set; }
    }
}