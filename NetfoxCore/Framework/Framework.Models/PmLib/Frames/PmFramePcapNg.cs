using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    [Serializable]
    [DataContract]
    public class PmFramePcapNg : PmFrameBase
    {
        private FrameBLockType _blockType; // holds this packet data
        private PcapNgInterface _iface; // holds link to interface to which frame belongs

        /// <summary>
        ///     Constructor used when indexing
        /// </summary>
        public PmFramePcapNg(
            PmCaptureBase pmCapture,
            Int64 fraIndex,
            Int64 fraOffset,
            PmLinkType pmLinkType,
            DateTime timeStamp,
            Int64 oriLength,
            Int64 incLength,
            FrameBLockType type,
            PcapNgInterface iface) : base(pmCapture, pmLinkType, timeStamp, incLength, PmFrameType.PcapNg, fraIndex,
            oriLength)
        {
            this.FrameOffset = fraOffset;
            this._blockType = type;
            this._iface = iface;

            UInt32 startOffset = 0;
            switch (this._blockType)
            {
                case FrameBLockType.EnhancedPacket:
                    startOffset = 28;
                    break;
                case FrameBLockType.SimplePacket:
                    startOffset = 12;
                    break;
                case FrameBLockType.PacketBlock:
                    startOffset = 28;
                    break;
            }

            this.L2Offset = this.FrameOffset + startOffset;
        }

        public PmFramePcapNg() : base()
        {
        }

        // optional values :
        [NotMapped] public Byte[] EpbFlags { get; set; }
        [NotMapped] public Byte[] EpbHash { get; set; }
        public UInt64 Dropcount { get; set; }

        /// <summary>
        ///     Block type must be known to get raw packet data
        /// </summary>
        public enum FrameBLockType
        {
            PacketBlock,
            SimplePacket,
            EnhancedPacket
        }
    }
}