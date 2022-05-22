using Netfox.Framework.CaptureProcessor.Captures;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface ICaptureProcessorBlockFactory
    {
        CaptureProcessorBlock Create();
    }
}