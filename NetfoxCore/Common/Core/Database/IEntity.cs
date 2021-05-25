using System;
using UnitOfWork.BaseDataEntity;

namespace Netfox.Core.Database
{
    public interface IEntity : IDataEntity
    {
        DateTime FirstSeen { get; }
    }
}