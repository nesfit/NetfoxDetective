using System;
using System.Linq;
using System.Threading.Tasks;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;

namespace Netfox.Framework.Models.PmLib.Frames
{
    public class PmFrameVirtual : PmFrameBase
    {
        public PmFrameVirtual(
            PmCaptureBase pmCapture,
            Int64 fraIndex,
            DateTime timeStamp,
            Int64 incLength,
            Int64 l2Offset) : base(pmCapture, SupportedTypes.PmLinkType.Raw, timeStamp, incLength, SupportedTypes.PmFrameType.Virtual, fraIndex,
            incLength)
        {
            // NOTE: value of incLength passed to base constructor also as originalLength
            this.L2Offset = l2Offset;
            this.FrameOffset = l2Offset;
        }

        private PmFrameVirtual() : base()
        {
        }

        public PmFrameVirtual(PmFrameBase copyFrame) : base()
        {
            var t = typeof(PmFrameBase);
            var properties = t.GetProperties().Where(p => p.CanWrite && p.CanRead && p.Name != nameof(PmFrameBase.Id));
            foreach (var pi in properties)
            {
                pi.SetValue(this, pi.GetValue(copyFrame, null), null);
            }
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task IndexFrame(IPmCaptureProcessorBlockBase captureProcessorBlockBase)
        {
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}