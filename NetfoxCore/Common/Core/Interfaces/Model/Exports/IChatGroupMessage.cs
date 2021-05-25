using System.Collections.Generic;

namespace Netfox.Core.Interfaces.Model.Exports
{
    public interface IChatGroupMessage : IChatMessage
    {
        IEnumerable<string> Receivers { get; }
    }
}