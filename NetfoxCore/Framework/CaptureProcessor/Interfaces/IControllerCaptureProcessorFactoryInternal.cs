using Netfox.Framework.CaptureProcessor.CoreController;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IControllerCaptureProcessorFactoryInternal
    {
        ControllerCaptureProcessorLocal Create();
    }
}