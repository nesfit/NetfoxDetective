using Netfox.Persistence;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface INetfoxDBContextFactory
    {
        NetfoxDbContext Create();
    }
}