using System.Collections;
using System.Collections.Generic;
using AlphaChiTech.VirtualizingCollection.Interfaces;

namespace AlphaChiTech.Virtualization
{
    public class VirtualizingObservableCollectionSource<T> : SynchronizedCollection<T>, IItemSourceProvider<T>, IEditableProvider<T>, IEditableProviderIndexBased<T>,
        IEditableProviderItemBased<T> where T : class
    {
        #region Implementation of IBaseSourceProvider
        public void OnReset(int count) { lock(this.SyncRoot) { this.Clear(); } }
        #endregion

        public new IEnumerator<T> GetEnumerator() => new VirtualizingObservableCollectionSourceEnumerator<T>(this);
        private IEnumerator GetBaseEnumerator() => base.GetEnumerator();

        public class VirtualizingObservableCollectionSourceEnumerator<TT> : IEnumerator<TT> where TT : class
        {
            private readonly VirtualizingObservableCollectionSource<TT> _sourceCollection;
            private int _index = -1;

            internal VirtualizingObservableCollectionSourceEnumerator(VirtualizingObservableCollectionSource<TT> sourceCollection) { this._sourceCollection = sourceCollection; }

            #region Implementation of IDisposable
            public void Dispose() { }
            #endregion

            #region Implementation of IEnumerator
            public bool MoveNext()
            {
                lock(this._sourceCollection.SyncRoot)
                {
                    if(this._sourceCollection.Count <= ++this._index) { return false; }

                    this.Current = this._sourceCollection[this._index];
                    return true;
                }
            }

            public void Reset() { this._index = -1; }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>
            ///     The element in the collection at the current position of the enumerator.
            /// </returns>
            public TT Current { get; private set; }

            object IEnumerator.Current => this.Current;
            #endregion
        }

        #region Implementation of IItemSourceProvider<T>
        public T GetAt(int index, object voc, bool usePlaceholder) { return base[index]; }
        public int GetCount(bool asyncOk) { return this.Count; }
        //public int IndexOf(T item) { this.IndexOf() }
        #endregion

        #region Implementation of IEditableProvider<T>
        public int OnAppend(T item, object timestamp)
        {
            lock(this.SyncRoot)
            {
                this.Add(item);
                return this.Count - 1;
            }
        }

        public void OnInsert(int index, T item, object timestamp) { lock(this.SyncRoot) { this.Insert(index, item); } }
        public void OnRemove(int index, T item, object timestamp) { lock(this.SyncRoot) { this.RemoveAt(index); } }
        public void OnReplace(int index, T oldItem, T newItem, object timestamp) { this[index] = newItem; }
        #endregion

        #region Implementation of IEditableProviderIndexBased<T>
        public T OnRemove(int index, object timestamp)
        {
            lock(this.SyncRoot)
            {
                var item = this[index];
                this.RemoveAt(index);
                return item;
            }
        }

        public T OnReplace(int index, T newItem, object timestamp)
        {
            var oldItem = this[index];
            this[index] = newItem;
            return oldItem;
        }
        #endregion

        #region Implementation of IEditableProviderItemBased<in T>
        public int OnRemove(T item, object timestamp)
        {
            lock(this.SyncRoot)
            {
                var index = this.IndexOf(item);
                this.RemoveAt(index);
                return index;
            }
        }

        public int OnReplace(T oldItem, T newItem, object timestamp)
        {
            var index = this.IndexOf(oldItem);
            this[index] = newItem;
            return index;
        }

        #region Implementation of ISynchronized
        public bool IsSynchronized { get; } = true;
        #endregion

        #endregion
    }
}