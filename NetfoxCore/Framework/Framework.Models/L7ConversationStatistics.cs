using System;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Enums;

namespace Netfox.Framework.Models
{
    public class L7ConversationStatistics : L4ConversationStatistics
    {
        private long? _extractedBytes;
        private long? _missingBytes;
        private long? _missingFrames;

        internal L7ConversationStatistics(L7ConversationStatistics upFlowStatistic,
            L7ConversationStatistics downFlowStatistic) : base(upFlowStatistic.L7Conversation)
        {
            base.UPFlowStatistic = upFlowStatistic;
            base.DownFlowStatistic = downFlowStatistic;
            this.L7Conversation = upFlowStatistic.L7Conversation;
            this.L7ConversationRefId = upFlowStatistic.L7ConversationRefId;
        }

        public L7ConversationStatistics(FsUnidirectionalFlow flow, L7Conversation l7conversation,
            DaRFlowDirection flowDirection) : base(flowDirection, l7conversation)
        {
            this.L7Conversation = l7conversation;
            this.L7ConversationRefId = l7conversation.Id;

            if (flow == null) return;

            foreach (var frame in flow.RealFrames)
            {
                base.ProcessFrame(frame);
                this.ExtractedBytes += frame.L7PayloadLength;
            }

            foreach (var frame in flow.VirtualFrames)
            {
                base.ProcessFrame(frame);
                this.MissingFrames++;
                this.MissingBytes += frame.L7PayloadLength;
            }
        }

        protected L7ConversationStatistics()
        {
        }

        public Guid? L7ConversationRefId { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(this.ExtractedBytes)}: {this.ExtractedBytes}, {nameof(this.MissingBytes)}: {this.MissingBytes}, {nameof(this.MissingFrames)}: {this.MissingFrames}";
        }

        [ForeignKey(nameof(L7ConversationRefId))]
        public virtual L7Conversation L7Conversation { get; set; }

        public Int64 ExtractedBytes
        {
            get
            {
                return (long) (this._extractedBytes ?? (this._extractedBytes =
                    this.UPFlowStatistic?.ExtractedBytes + this.DownFlowStatistic?.ExtractedBytes ?? 0));
            }
            set { this._extractedBytes = value; }
        }

        public Int64 MissingBytes
        {
            get
            {
                return (long) (this._missingBytes ?? (this._missingBytes =
                    this.UPFlowStatistic?.MissingBytes + this.DownFlowStatistic?.MissingBytes ?? 0));
            }
            set { this._missingBytes = value; }
        }

        public Int64 MissingFrames
        {
            get
            {
                return (long) (this._missingFrames ?? (this._missingFrames =
                    this.UPFlowStatistic?.MissingFrames + this.DownFlowStatistic?.MissingFrames ?? 0));
            }
            set { this._missingFrames = value; }
        }

        #region Overrides of ConversationStatisticsBase

        public new L7ConversationStatistics DownFlowStatistic => base.DownFlowStatistic as L7ConversationStatistics;
        public new L7ConversationStatistics UPFlowStatistic => base.UPFlowStatistic as L7ConversationStatistics;

        #endregion
    }
}