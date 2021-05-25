using System;

namespace Netfox.Core.Database
{
    public interface IOrdereableEntity : IEntity
    {
        Int64 OrderingKey { get; }
    }
}