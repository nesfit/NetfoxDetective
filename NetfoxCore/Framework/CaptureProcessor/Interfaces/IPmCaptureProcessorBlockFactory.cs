using System.IO;
using Netfox.Framework.CaptureProcessor.Captures;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IPmCaptureProcessorBlockFactory
    {
        PmCaptureProcessorBlockMnm CreateMnm(FileInfo fileInfo);
        PmCaptureProcessorBlockPcap CreatePcap(FileInfo fileInfo);
        PmCaptureProcessorBlockPcapNg CreatePcapNg(FileInfo fileInfo);
    }
}