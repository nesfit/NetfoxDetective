using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Netfox.Core.Database
{
    public delegate void DbSetChangedHandler(DbContext sender, DbSetChangedArgs args);

    public interface IObservableNetfoxDBContext
    {
        System.Data.Entity.Database Database { get; }
        int SaveChanges();
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        void RegisterDbSetTypes();
        void RegisterVirtualizingObservableDBSetPagedCollections();
        void ActivateDbSetChangeNotifier(Type dbSetType);
        event DbSetChangedHandler DbSetChanged;
    }
}