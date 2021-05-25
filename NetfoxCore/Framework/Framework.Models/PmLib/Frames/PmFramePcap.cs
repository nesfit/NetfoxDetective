using System;
using System.Runtime.Serialization;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    [Serializable]
    [DataContract]
    public class PmFramePcap : PmFrameBase
    {
        /// <summary>
        ///     Constructor used when indexing
        /// </summary>
        public PmFramePcap(
            PmCaptureBase pmCapture,
            Int64 fraIndex,
            Int64 fraOffset,
            PmLinkType pmLinkType,
            DateTime timeStamp,
            Int64 oriLength,
            Int64 incLength) : base(pmCapture, pmLinkType, timeStamp, incLength, PmFrameType.Pcap, fraIndex, oriLength)
        {
            this.FrameOffset = fraOffset;
            this.L2Offset = this.FrameOffset + 16;
        }

        public PmFramePcap() : base()
        {
        }
    }
}