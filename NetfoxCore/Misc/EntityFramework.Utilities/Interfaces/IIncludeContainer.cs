using System.Collections.Generic;

namespace EntityFramework.Utilities.Interfaces
{
    public interface IIncludeContainer<T>
    {
        IEnumerable<IncludeExecuter<T>> Includes { get; }
    }
}