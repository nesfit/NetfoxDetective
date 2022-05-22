using System;
using System.Runtime.Serialization;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    [Serializable]
    [DataContract]
    public class PmFrameMnm : PmFrameBase
    {
        public PmFrameMnm(
            PmCaptureBase pmCapture,
            UInt32 fraIndex,
            UInt32 fraOffset,
            PmLinkType pmLinkType,
            DateTime timeStamp,
            Int32 oriLength,
            Int32 incLength) : base(pmCapture, pmLinkType, timeStamp, incLength, PmFrameType.Mnm, fraIndex, oriLength)
        {
            this.FrameOffset = fraOffset;
            this.L2Offset = this.FrameOffset + 16;
        }

        public PmFrameMnm() : base()
        {
        }
    }
}