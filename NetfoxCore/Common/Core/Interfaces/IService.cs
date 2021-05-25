using Castle.Core.Logging;

namespace Netfox.Core.Interfaces
{
    public interface IService
    {
        ILogger Logger { get; set; }
    }
}