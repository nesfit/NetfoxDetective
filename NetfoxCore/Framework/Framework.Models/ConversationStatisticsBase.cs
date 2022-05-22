using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.Models
{
    public abstract class ConversationStatisticsBase : ILxConversationStatistics
    {
        [Key] public virtual Guid Id { get; set; } = Guid.NewGuid();

        [NotMapped] public virtual ConversationStatisticsBase DownFlowStatistic { get; protected set; }

        [NotMapped] public virtual ConversationStatisticsBase UPFlowStatistic { get; protected set; }

        public DaRFlowDirection FlowDirection { get; protected set; } = DaRFlowDirection.non;
        private readonly ILxConversation _converation;

        private Int64? _bytes;

        private DateTime _firstSeen = DateTime.MaxValue;

        private Int64? _frames;

        private DateTime _lastSeen = DateTime.MinValue;

        private Int64? _malformedFrames;

        protected ConversationStatisticsBase()
        {
        }

        protected ConversationStatisticsBase(ILxConversation conversation)
        {
            this._converation = conversation;
        }

        protected ConversationStatisticsBase(DaRFlowDirection flowDirection, ILxConversation conversation)
        {
            this._converation = conversation;
            this.FlowDirection = flowDirection;
            this.Bytes = 0;
            this.Frames = 0;
            this.MalformedFrames = 0;
        }

        public virtual void ProcessFrame(PmFrameBase frame)
        {
            if (this.IsStatisticOfConversation) //Base class l3conversation
            {
                var conversationStatistics =
                    this._converation.SourceEndPoint.Address.Equals(frame.SourceEndPoint.Address)
                        ? this.UPFlowStatistic
                        : this.DownFlowStatistic;
                conversationStatistics.ProcessFrame(frame);
                return;
            }

            this.Bytes += frame.IncludedLength;
            this.Frames++;
            if (frame.IsMalformed) this.MalformedFrames++;
            if (frame.TimeStamp < this.FirstSeen)
            {
                this.FirstSeen = frame.TimeStamp;
            }

            if (frame.TimeStamp > this.LastSeen)
            {
                this.LastSeen = frame.TimeStamp;
            }
        }


        protected Boolean IsStatisticOfConversation => this.FlowDirection == DaRFlowDirection.non;

        public Int64 Bytes
        {
            get
            {
                return (uint) (this._bytes ??
                               (this._bytes = this.UPFlowStatistic.Bytes + this.DownFlowStatistic.Bytes));
            }
            set { this._bytes = value; }
        }

        public DateTime FirstSeen
        {
            get
            {
                if (!this.IsStatisticOfConversation)
                {
                    return this._firstSeen;
                }

                if (this.UPFlowStatistic != null && this.DownFlowStatistic != null)
                {
                    return
                        (this._firstSeen =
                            (this.UPFlowStatistic.FirstSeen < this.DownFlowStatistic.FirstSeen)
                                ? this.UPFlowStatistic.FirstSeen
                                : this.DownFlowStatistic.FirstSeen);
                }

                this._firstSeen = this.UPFlowStatistic?.FirstSeen ??
                                  this.DownFlowStatistic?.FirstSeen ?? DateTime.MinValue;
                return this._firstSeen;
            }
            set { this._firstSeen = value; }
        }

        public Int64 Frames
        {
            get
            {
                return (uint) (this._frames ??
                               (this._frames = this.UPFlowStatistic.Frames + this.DownFlowStatistic.Frames));
            }
            set { this._frames = value; }
        }

        public DateTime LastSeen
        {
            get
            {
                if (!this.IsStatisticOfConversation)
                {
                    return this._lastSeen;
                }

                if (this.UPFlowStatistic != null && this.DownFlowStatistic != null)
                {
                    return (this._lastSeen = (this.UPFlowStatistic.LastSeen > this.DownFlowStatistic.LastSeen)
                        ? this.UPFlowStatistic.LastSeen
                        : this.DownFlowStatistic.LastSeen);
                }

                this._lastSeen = this.UPFlowStatistic?.LastSeen ??
                                 this.DownFlowStatistic?.LastSeen ?? DateTime.MaxValue;
                return this._lastSeen;
            }

            set { this._lastSeen = value; }
        }

        public Int64 MalformedFrames
        {
            get
            {
                return (uint) (this._malformedFrames ?? (this._malformedFrames =
                    this.UPFlowStatistic.MalformedFrames + this.DownFlowStatistic.MalformedFrames));
            }
            set { this._malformedFrames = value; }
        }

        #region Implementation of ILxStatisticsCalculated

        public TimeSpan Duration => this.LastSeen - this.FirstSeen;

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return
                $"{nameof(this.FirstSeen)}: {this.FirstSeen}, {nameof(this.LastSeen)}: {this.LastSeen}, {nameof(this.Duration)}: {this.Duration}, {nameof(this.FlowDirection)}: {this.FlowDirection}, {nameof(this.Bytes)}: {this.Bytes}, {nameof(this.Frames)}: {this.Frames}, {nameof(this.MalformedFrames)}: {this.MalformedFrames}";
        }

        public override bool Equals(object obj)
        {
            var conversationStatistics = obj as ConversationStatisticsBase;
            return conversationStatistics != null && this.Equals(conversationStatistics);
        }

        public Boolean Equals(ConversationStatisticsBase other)
        {
            return this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion
    }
}