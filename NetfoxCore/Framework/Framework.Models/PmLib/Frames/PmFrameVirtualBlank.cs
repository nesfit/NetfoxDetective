using System;

namespace Netfox.Framework.Models.PmLib.Frames
{
    public sealed class PmFrameVirtualBlank : PmFrameBase
    {
        /// <summary>
        ///     Constructor used when creating new one or indexing existing
        /// </summary>
        public PmFrameVirtualBlank(PmFrameBase template, Int64 fraLength, DateTime dateTime)
            : base(template.PmCapture, SupportedTypes.PmLinkType.Null, dateTime, fraLength, SupportedTypes.PmFrameType.VirtualBlank,
                template.FrameIndex, fraLength)
        {
            this.L7PayloadLength = fraLength;
            this.FrameOffset = 0;
            this.L2Offset = 0;
            this.L3Offset = 0;
            this.L4Offset = 0;
            this.L7Offset = 0;
            this.DstAddress = template.DstAddress;
            this.SrcAddress = template.SrcAddress;
        }

        private PmFrameVirtualBlank() : base()
        {
        }

        /// <summary>
        ///     Retrieves fake zeroed L7 data that could be use as binary stuffing
        /// </summary>
        /// <returns>Byte array containing only 0</returns>
        public override Byte[] L7Data() => new Byte[this.IncludedLength];

        #region Functions not applicable for VirtualFrame

        /// <summary>
        ///     Function returns ALWAYS null for VirtualFrame
        /// </summary>
        public override Byte[] L2Data() => null;

        /// <summary>
        ///     Function returns ALWAYS null for VirtualFrame
        /// </summary>
        public override Byte[] L3Data() => null;

        /// <summary>
        ///     Function returns ALWAYS null for VirtualFrame
        /// </summary>
        public override Byte[] L4Data() => null;

        #endregion
    }
}