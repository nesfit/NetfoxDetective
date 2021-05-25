using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using Castle.Core.Internal;
using Netfox.Core.Database;
using Netfox.Core.Enums;
using Netfox.Core.Extensions;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.PmLib.Frames;
using PostSharp.Patterns.Model;

namespace Netfox.Framework.Models
{
    [Serializable]
    public class L7PDU : IExportSource, IOrdereableEntity
    {

        private DateTime _firstSeen = DateTime.MinValue;

        private L7Conversation _l7Conversation;
        private Guid? _l7ConversationRefId;

        private DateTime _lastSeen = DateTime.MaxValue;

        public L7PDU(FsUnidirectionalFlow baseFlow)
        {
            this.BaseFlow = baseFlow;
            this.IsContainingCorruptedData = false;
        }

        public L7PDU(FsUnidirectionalFlow baseFlow, L7PDU l7Pdu) : this(baseFlow)
        {
            this.CopyL7Pdu(l7Pdu);
        }

        private void CopyL7Pdu(L7PDU l7Pdu)
        {
            this.UnorderedFrameList.AddRange(l7Pdu.UnorderedFrameList.Select(f => new PmFrameVirtual(f)));
            foreach (var frame in this.UnorderedFrameList)
            {
                frame.L7PduRefId = this.Id;
            }

            this.ExtractedBytes = l7Pdu.ExtractedBytes;
            this.FlowDirection = l7Pdu.FlowDirection;
            this.IsContainingCorruptedData = l7Pdu.IsContainingCorruptedData;
            this.LowestTCPSeq = l7Pdu.LowestTCPSeq;
            this.MissingFrameSequences = l7Pdu.MissingFrameSequences;
            this.OrderingKey = l7Pdu.OrderingKey;
        }

        protected L7PDU()
        {
        }

        public Int64 OrderingKey { get; set; }
        public Int64 NextFrameOrderingKey { get; set; } = 0;

        public DateTime LastSeen
        {
            get
            {
                return this._lastSeen == DateTime.MaxValue
                    ? (this._lastSeen = !this.UnorderedFrameList.IsNullOrEmpty()
                        ? this.UnorderedFrameList.Max(frame => frame.TimeStamp)
                        : DateTime.MaxValue)
                    : this._lastSeen;
            }
            set { this._lastSeen = value; }
        }

        [DataMember]
        public virtual IList<PmFrameBase> UnorderedFrameList { get; private set; } = new List<PmFrameBase>();

        public virtual IEnumerable<PmFrameBase> FrameList => this.UnorderedFrameList.OrderBy(i => i.OrderingKey);

        public Guid? L7ConversationRefId
        {
            get { return this._l7ConversationRefId ?? (this._l7ConversationRefId = this.L7Conversation.Id); }
            set { this._l7ConversationRefId = value; }
        }

        [ForeignKey(nameof(L7ConversationRefId))]
        public virtual L7Conversation L7Conversation
        {
            get { return this._l7Conversation ?? (this._l7Conversation = this.BaseFlow?.L7Conversation); }
            set { this._l7Conversation = value; }
        }

        public Int64 ExtractedBytes { get; set; }

        public DaRFlowDirection FlowDirection { get; set; }

        public Boolean IsContainingCorruptedData { get; set; }

        public Int64? LowestTCPSeq { get; set; }

        public Int64 MissingBytes { get; set; }

        public Int32 MissingFrameSequences { get; set; }

        public Byte[] PDUByteArr
        {
            get
            {
                var frames = this.FrameList.ToList();
                frames.RemoveAll(pmFrame => pmFrame.L7PayloadLength < 1);
                var len = frames.Sum(pmFrame => pmFrame.L7PayloadLength);
                var ms = new MemoryStream(new Byte[len], 0, (Int32)len, true, true);
                foreach (var data in frames.Select(pmFrame => pmFrame.L7Data())) ms.Write(data, 0, data.Length);
                return ms.GetBuffer();
            }
        }

        public Int64 PDULength => this.MissingBytes + this.ExtractedBytes;

        private FsUnidirectionalFlow BaseFlow { get; set; }

        [IgnoreAutoChangeNotification]
        public IPEndPoint DestinationEndPoint
        {
            get
            {
                if (this.L7Conversation == null)
                {
                    return null;
                }

                switch (this.FlowDirection)
                {
                    case DaRFlowDirection.up:
                        return this.L7Conversation.DestinationEndPoint;
                    case DaRFlowDirection.down:
                        return this.L7Conversation.SourceEndPoint;
                    case DaRFlowDirection.non:
                    default:
                        return this.L7Conversation?.DestinationEndPoint;
                }
            }
        }

        [IgnoreAutoChangeNotification]
        public IPEndPoint SourceEndPoint
        {
            get
            {
                if (this.L7Conversation == null)
                {
                    return null;
                }

                switch (this.FlowDirection)
                {
                    case DaRFlowDirection.up:
                        return this.L7Conversation.SourceEndPoint;
                    case DaRFlowDirection.down:
                        return this.L7Conversation.DestinationEndPoint;
                    case DaRFlowDirection.non:
                    default:
                        return this.L7Conversation?.SourceEndPoint;
                }
            }
        }

        public DateTime FirstSeen
        {
            get
            {
                return this._firstSeen == DateTime.MinValue
                    ? (this._firstSeen = !this.UnorderedFrameList.IsNullOrEmpty()
                        ? this.UnorderedFrameList.Min(frame => frame.TimeStamp)
                        : DateTime.MinValue)
                    : this._firstSeen;
            }
            set { this._firstSeen = value; }
        }

        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        public void AddFrame(PmFrameBase frame)
        {
            frame.OrderingKey = this.NextFrameOrderingKey++;
            if (!this.UnorderedFrameList.Any())
            {
                this.UnorderedFrameList.Add(frame);
                this.BaseFlow.AddL7PDU(this);
            }
            else
            {
                this.UnorderedFrameList.Add(frame);
            }

            frame.L7Pdu = this;
        }

        public void AddFrameRange(IEnumerable<PmFrameBase> frames)
        {
            if (!this.UnorderedFrameList.Any())
            {
                this.BaseFlow.AddL7PDU(this);
            }

            var pmFrameBases = frames as PmFrameBase[] ?? frames.ToArray();
            foreach (var frame in pmFrameBases)
            {
                frame.OrderingKey = this.NextFrameOrderingKey++;
                frame.L7Pdu = this;
            }

            this.UnorderedFrameList.AddRange(pmFrameBases);
        }

        public PmFrameBase RemoveLastFrame()
        {
            this.NextFrameOrderingKey--;
            var removedFrame = this.UnorderedFrameList[this.UnorderedFrameList.Count - 1];
            this.ExtractedBytes -= removedFrame.L7PayloadLength;
            this.UnorderedFrameList.RemoveAt(this.UnorderedFrameList.Count - 1);
            if (!this.UnorderedFrameList.Any())
            {
                this.BaseFlow.RemoveL7PDU(this);
            }

            removedFrame.L7Pdu = null;
            return removedFrame;
        }

        public override string ToString()
        {
            return
                $"{this.SourceEndPoint}-{this.DestinationEndPoint}, {this.FirstSeen}-{this.LastSeen}, {nameof(this.ExtractedBytes)}: {this.ExtractedBytes}, {nameof(this.FlowDirection)}: {this.FlowDirection}, {nameof(this.IsContainingCorruptedData)}: {this.IsContainingCorruptedData}, {nameof(this.MissingBytes)}: {this.MissingBytes}, {nameof(this.MissingFrameSequences)}: {this.MissingFrameSequences}, {nameof(this.PDULength)}: {this.PDULength}{Environment.NewLine}{nameof(this.L7Conversation)}: {this.L7Conversation}";
        }
    }
}