using System.Net;
using Netfox.Core.Database;

namespace Netfox.Core.Interfaces.Model.Exports
{
    public interface IExportSource : IEntity
    {
        IPEndPoint SourceEndPoint { get; }
        IPEndPoint DestinationEndPoint { get; }
    }
}