using Castle.Core.Logging;

namespace Netfox.Core.Interfaces
{
    //TODO: Description
    public interface ILoggable
    {
        ILogger Logger { get; set; }
    }
}