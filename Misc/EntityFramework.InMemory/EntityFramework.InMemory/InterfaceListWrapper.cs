using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Netfox.Core.Collections;

namespace EntityFramework.InMemory
{
    public class InterfaceListWrapper<TEntity> : IList<TEntity>,INotifyCollectionChanged, IList where TEntity:class 
    {
        public InterfaceListWrapper(ConcurrentObservableCollection<TEntity> concurrentObservableCollection)
        {
            this.Data = concurrentObservableCollection;
            this.Local = concurrentObservableCollection;
        }

        public InterfaceListWrapper(IList testDbSetGeneric)
        {
            this.Data = testDbSetGeneric;
        }

        public IList Data { get; }
        public ConcurrentObservableCollection<TEntity> Local { get; }

        #region Implementation of IEnumerable
        public IEnumerator<TEntity> GetEnumerator() {
            return this.Data.OfType<TEntity>().Select(item => item as TEntity).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable) this.Data).GetEnumerator(); }
        #endregion

        #region Implementation of ICollection<TEntity>
        public void Add(TEntity item) { this.Data.Add(item); }
        public int Add(object value) { return ((IList) this.Data).Add(value); }
        public bool Contains(object value) { return ((IList) this.Data).Contains(value); }
        void IList.Clear() { this.Data.Clear(); }
        public int IndexOf(object value) { return ((IList) this.Data).IndexOf(value); }
        public void Insert(int index, object value) { ((IList) this.Data).Insert(index, value); }
        public void Remove(object value) { ((IList) this.Data).Remove(value); }
        void IList.RemoveAt(int index) { this.Data.RemoveAt(index); }
        object IList.this[int index]
        {
            get { return ((IList) this.Data)[index]; }
            set { ((IList) this.Data)[index] = value; }
        }

        bool IList.IsReadOnly => ((IList) this.Data).IsReadOnly;

        public bool IsFixedSize => ((IList) this.Data).IsFixedSize;

        void ICollection<TEntity>.Clear() { this.Data.Clear(); }
        public bool Contains(TEntity item) { return this.Data.Contains(item); }
        public void CopyTo(TEntity[] array, int arrayIndex) { this.Data.CopyTo(array, arrayIndex); }
        public bool Remove(TEntity item) { this.Data.Remove(item); return true;}
        public void CopyTo(Array array, int index) { ((ICollection) this.Data).CopyTo(array, index); }
        int ICollection.Count => this.Data.Count;

        public object SyncRoot => ((ICollection) this.Data).SyncRoot;

        public bool IsSynchronized => ((ICollection) this.Data).IsSynchronized;

        int ICollection<TEntity>.Count => this.Data.Count;

        bool ICollection<TEntity>.IsReadOnly => this.Data.IsReadOnly;
        #endregion

        #region Implementation of IList<TEntity>
        public int IndexOf(TEntity item) { return this.Data.IndexOf(item); }
        public void Insert(int index, TEntity item) { this.Data.Insert(index, item); }
        void IList<TEntity>.RemoveAt(int index) { this.Data.RemoveAt(index); }
        public TEntity this[int index]
        {
            get { return this.Data[index] as TEntity; }
            set { this.Data[index] = value; }
        }

        public void Clear()
        {
            this.Data.Clear();
        }

        public void RemoveAt(int index)
        {
            this.Data.RemoveAt(index);
        }
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