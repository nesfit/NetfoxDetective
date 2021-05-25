using System;
using System.Threading.Tasks;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IPmCaptureProcessorBlockBase
    {
        Task CreateAndAddToMetaFramesVirtualFrame(PmFrameBase pmFrame, PmPacket parentPacket);

        /// <summary>
        ///     This method must be implemented in children classes
        /// </summary>
        Boolean WriteCaptureFile(String outputFile);
    }
}