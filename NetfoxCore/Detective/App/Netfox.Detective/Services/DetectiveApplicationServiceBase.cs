using Castle.Core.Logging;
using Netfox.Core.Interfaces;
using Netfox.Detective.ViewModels.Interfaces;

namespace Netfox.Detective.Services
{
    public abstract class DetectiveApplicationServiceBase : IDetectiveService
    {
        public ILogger Logger { get; set; }
        public ISystemServices SystemServices { get; set; }
    }
}