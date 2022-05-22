using Netfox.Core.Infrastructure;
using Netfox.Logger;

namespace Netfox.Detective.Infrastructure
{
    public interface IAppSettings : INetfoxSettings, ILoggerSettings
    {
    }
}