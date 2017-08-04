using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Netfox.Core.Collections;
using Netfox.Core.Database;
using Netfox.Core.Extensions;

namespace EntityFramework.InMemory
{
 
    public class TestDbSet : DbSet, IQueryable, IDbAsyncEnumerable
    {
        private readonly Type _type;
        readonly IList _data;

        public IList TestDbSetGeneric { get; }

        public TestDbSet(IList testDbSetGeneric,Type type)
        {
            this._type = type;
            this._data = testDbSetGeneric;
            this.TestDbSetGeneric = testDbSetGeneric;
        }

        #region Overrides of DbSet
        public override object Find(params object[] keyValues) => throw new NotImplementedException();
        public override Task<object> FindAsync(params object[] keyValues) => throw new NotImplementedException();
        public override Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues) => throw new NotImplementedException();

        public override object Attach(object entity) { this.Add(entity);
            return entity;
        }

        public override object Add(object entity)
        {
            this._data.Add(entity);
            return entity;
        }

        private static IEntity GetIModel(object entity) => entity as IEntity;
        
        public override IEnumerable AddRange(IEnumerable entities)
        {
            dynamic data = this._data; //TODO Use interface
            var addRange = entities as object[] ?? entities.Cast<object>().ToArray();
            data.AddRange(addRange);
            return addRange;
        }
        public override object Remove(object entity) { this._data.Remove(GetIModel(entity).Id);
            return entity;
        }
        public override IEnumerable RemoveRange(IEnumerable entities) {
            var removeRange = entities as object[] ?? entities.Cast<object>().ToArray();
            foreach (var entity in removeRange) { this.Remove(entity); }
            return removeRange;
        }
        public override object Create() => Activator.CreateInstance(this._type);
        public override object Create(Type derivedEntityType) => Activator.CreateInstance(derivedEntityType);
        public override DbSqlQuery SqlQuery(string sql, params object[] parameters) => throw new NotImplementedException();
        public override bool Equals(object obj) => throw new NotImplementedException();
        public override int GetHashCode() => throw new NotImplementedException();
        public override IList Local => this._data;
        #endregion

        #region Overrides of DbQuery
        public override DbQuery Include(string path) => throw new NotImplementedException();
        public override DbQuery AsNoTracking() => throw new NotImplementedException();
#pragma warning disable 672
        public override DbQuery AsStreaming() => throw new NotImplementedException();
#pragma warning restore 672
        public override string ToString() => throw new NotImplementedException();
        public override Type ElementType => this._type;
        #endregion

        #region Implementation of IDbAsyncEnumerable
        public IDbAsyncEnumerator GetAsyncEnumerator() => throw new NotImplementedException();
        #endregion

        #region Implementation of IQueryable
        public Expression Expression => throw new NotImplementedException();
        Type IQueryable.ElementType => this.ElementType;

        public IQueryProvider Provider => throw new NotImplementedException();
        #endregion

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this._data.GetEnumerator();
    }

    public class TestDbSet<TEntity> : DbSet<TEntity>, IQueryable, IEnumerable<TEntity>, IDbAsyncEnumerable<TEntity>, IList, INotifyCollectionChanged  where TEntity : class
    {

        public InterfaceListWrapper<TEntity> Data {get;}
        private ConcurrentHashSet<TEntity> DataHashSet { get; } = new ConcurrentHashSet<TEntity>();
        readonly IQueryable _query;

        public Action<TEntity> OnAddAction { get; }
        public Action<IEnumerable<TEntity>> OnAddBulkAction { get; }
        public Action<TEntity> OnRemoveAction { get; }

        public TestDbSet(Action<TEntity> addCallback, Action<IEnumerable<TEntity>> addBulkCallback, Action<TEntity> removeCallback) : this(addCallback, addBulkCallback, removeCallback, null)
        { }

        public TestDbSet(Action<TEntity> addCallback, Action<IEnumerable<TEntity>> addBulkCallback, Action<TEntity> removeCallback, TestDbSet dbSet = null)
        {
            if(dbSet == null)
            {
                this.Data = new InterfaceListWrapper<TEntity>(new ConcurrentObservableCollection<TEntity>());
                this._query = this.Data.AsQueryable().Where(p => (p.GetType().IsSubclassOf(typeof(TEntity))) || (p.GetType() == typeof(TEntity)));
            }
            else
            {
                this.Data = new InterfaceListWrapper<TEntity>(dbSet.TestDbSetGeneric);
                this._query = this.Data.AsQueryable().Where(p => (p.GetType().IsSubclassOf(typeof(TEntity))) || (p.GetType() == typeof(TEntity)));
            }

            this.OnAddAction = addCallback;
            this.OnAddBulkAction = addBulkCallback;
            this.OnRemoveAction = removeCallback;
        }
        

        public override TEntity Add(TEntity item)
        {
            if ((item == null) || (this.DataHashSet.Contains(item)))
            {
                return item;
            }

            this.Data.Add(item);
            this.DataHashSet.Add(item);
            this.OnAddAction?.Invoke(item);
            return item;
        }

        public  IEnumerable<TEntity> AddRange(IEnumerable items) { return this.AddRange(items,true); }

        public IEnumerable<TEntity> AddRange(IEnumerable items, bool insertReferencedObjects)
        {
            var typedItems = items.Cast<TEntity>();

            if(typedItems.Any(i => i == null)) Debugger.Break();

            var itemsByType = typedItems.GroupBy(item => item.GetType(), item => item, (type, homoItems) => new
            {
                type,
                homoItems
            });
            foreach(var homoItems in itemsByType.Select(i => i.homoItems.Where(ii => !this.DataHashSet.Contains(ii))))
            {
                var enumerable = homoItems as TEntity[] ?? homoItems.ToArray();
                if(!enumerable.Any()) continue;
                this.Data.AddRange(enumerable);
                this.DataHashSet.AddRange(enumerable);
                if(insertReferencedObjects) this.OnAddBulkAction?.Invoke(enumerable);
            }
            return typedItems;
        }

        public override IEnumerable<TEntity> AddRange(IEnumerable<TEntity> items) { return this.AddRange(items,true); }

        public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> items, bool insertReferencedObjects)
        {
            var addRange = items as TEntity[] ?? items.ToArray();
            var itemsByType = addRange.GroupBy(item => item.GetType(), item => item, (type, homoItems) => new
            {
                type,
                homoItems
            });
            foreach(var homoItems in itemsByType.Select(i => i.homoItems))
            {
                var enumerable = homoItems as TEntity[] ?? homoItems.ToArray();
                this.Data.AddRange(enumerable);
                if (insertReferencedObjects) this.OnAddBulkAction?.Invoke(enumerable);
            }
            return addRange;
        }

        public override TEntity Remove(TEntity item)
        {
            if ((item == null) || (this.DataHashSet.Contains(item)))
            {
                return item;
            }

            this.Data.Remove(item);
            this.OnRemoveAction?.Invoke(item);
            return item;
        }

        public override TEntity Attach(TEntity item)
        {
            this.Data.Add(item);
            return item;
        }

        public override TEntity Create()
        {
            return Activator.CreateInstance<TEntity>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public override ObservableCollection<TEntity> Local => this.Data.Local;

        Type IQueryable.ElementType => this._query.ElementType;

        Expression IQueryable.Expression => this._query.Expression;

        IQueryProvider IQueryable.Provider => new TestDbAsyncQueryProvider<TEntity>(this._query.Provider);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        IDbAsyncEnumerator<TEntity> IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
        {
            return new TestDbAsyncEnumerator<TEntity>(this.Data.GetEnumerator());
        }

        public static implicit operator DbSet(TestDbSet<TEntity> entry)
        {
            return entry as DbSet<TEntity>;
        }

        #region Implementation of ICollection
        public void CopyTo(Array array, int index) { ((ICollection) this.Data).CopyTo(array, index); }
        public int Count => this.Data.Count();

        public object SyncRoot => ((ICollection) this.Data).SyncRoot;

        public bool IsSynchronized => ((ICollection) this.Data).IsSynchronized;
        #endregion

        #region Implementation of IList
        int IList.Add(object value)
        {
            if(value == null) { return 0; }

            if(value.GetType().IsGenericType)
            {
                var list = value as IList;
                if(list == null) { return 0;}

                foreach(var item in list)
                {
                    this.Add(item as TEntity);
                }
            }
            else
            {
                this.Add(value as TEntity);
            }
            
            return 0;
        }
        bool IList.Contains(object value) { return ((IList) this.Data).Contains(value); }
        void IList.Clear() { this.Data.Clear(); }
        int IList.IndexOf(object value) { return ((IList) this.Data).IndexOf(value); }
        void IList.Insert(int index, object value) { ((IList) this.Data).Insert(index, value); }

        void IList.Remove(object value)
        {
            if(value == null) { return; }

            if (value.GetType().IsGenericType)
            {
                var list = value as IList;
                if (list == null) { return; }

                foreach (var item in list)
                {
                    this.Remove(item as TEntity);
                }
            }
            else
            {
                this.Remove(value as TEntity);
            }
        }

        void IList.RemoveAt(int index) { this.Data.RemoveAt(index); }
        object IList.this[int index]
        {
            get => ((IList) this.Data)[index];
            set => ((IList) this.Data)[index] = value;
        }

        bool IList.IsReadOnly => ((IList) this.Data).IsReadOnly;

        bool IList.IsFixedSize => ((IList) this.Data).IsFixedSize;

        #endregion

        #region Implementation of INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => this.Local.CollectionChanged += value;
            remove => this.Local.CollectionChanged -= value;
        }
        #endregion
    }
}