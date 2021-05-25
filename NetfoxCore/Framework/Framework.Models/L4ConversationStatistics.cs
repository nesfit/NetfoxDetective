using System;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Framework.Models
{
    public class L4ConversationStatistics : L3ConversationStatistics
    {
        protected L4ConversationStatistics()
        {
        }

        internal L4ConversationStatistics(L4ConversationStatistics upFlowStatistic,
            L4ConversationStatistics downFlowStatistic) : base(upFlowStatistic.L4Conversation)
        {
            this.UPFlowStatistic = upFlowStatistic;
            this.DownFlowStatistic = downFlowStatistic;
            this.L4Conversation = upFlowStatistic.L4Conversation;
            this.L4ConversationRefId = upFlowStatistic.L4ConversationRefId;
        }

        internal L4ConversationStatistics(DaRFlowDirection flowDirection, L4Conversation l4Conversation) : base(
            flowDirection, l4Conversation)
        {
            this.L4Conversation = l4Conversation;
            this.L4ConversationRefId = l4Conversation.Id;
        }

        protected L4ConversationStatistics(DaRFlowDirection flowDirection, ILxConversation conversation) : base(
            flowDirection, conversation)
        {
        }

        protected L4ConversationStatistics(ILxConversation l7Conversation) : base(l7Conversation)
        {
        }

        public Guid? L4ConversationRefId { get; set; }

        [ForeignKey(nameof(L4ConversationRefId))]
        public virtual L4Conversation L4Conversation { get; set; }

        #region Overrides of Object

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            var conversationStatistics = obj as ConversationStatisticsBase;
            return conversationStatistics != null && this.Equals(conversationStatistics);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion
    }
}