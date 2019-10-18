// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

using AlphaChiTech.VirtualizingCollection.Interfaces;

namespace Netfox.Core.Collections
{
    /// <summary>
    ///     A collections which only holds weak references to the items.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public class WeakConccurentCollection<T> : ICollection<T>, IList<T>, IObservableCollection, IObservableCollection<T> where T : class
    {
        private readonly WeakReference _gcSentinel = new WeakReference(new object());
        private readonly ConcurrentObservableCollection<WeakReference> _inner;

        public void CopyTo(Array array, int index) { ((ICollection) this._inner).CopyTo(array, index); }

        public bool IsSynchronized => ((ICollection) this._inner).IsSynchronized;

        public object SyncRoot => ((ICollection) this._inner).SyncRoot;

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(T item)
        {
            // this.CleanIfNeeded();
            this._inner.Add(new WeakReference(item));
        }

        /// <summary>
        ///     Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            this._inner.Clear();
        }

        /// <summary>
        ///     Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            this.CleanIfNeeded();
            return this._inner.Any(w => ((T) w.Target) == item);
        }

        /// <summary>
        ///     Copies the elements of the collection to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from the collection.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CleanIfNeeded();

            if(array == null) throw new ArgumentNullException(nameof(array));
            if(arrayIndex < 0 || arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if((arrayIndex + this._inner.Count) > array.Length)
                throw new ArgumentException(
                    "The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

            var items = this._inner.Select(item => (T) item.Target).Where(value => value != null).ToArray();

            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count
        {
            get
            {
                this.CleanIfNeeded();
                return this._inner.Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly => false;

        /// <summary>
        ///     Removes the first occurrence of a specific object from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        ///     true if <paramref name="item" /> was successfully removed from the
        ///     <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
        ///     <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(T item)
        {
            this.CleanIfNeeded();

            for(var i = 0; i < this._inner.Count; i++)
            {
                var target = (T) this._inner[i].Target;
                if(target == item)
                {
                    this._inner.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the
        ///     collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            this.CleanIfNeeded();

            var enumerable = this._inner.Select(item => (T) item.Target).Where(value => value != null);
            return enumerable.GetEnumerator();
        }

        /// <summary>
        ///     Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            // this.CleanIfNeeded();
            this._inner.AddItems(items.Select(i => new WeakReference(i)));
        }

        /// <summary>
        ///     Removes all dead entries.
        /// </summary>
        /// <returns>true if entries where removed; otherwise false.</returns>
        public bool Purge()
        {
            var ret = false;
            foreach(var item in this._inner)
            {
                if(!item.IsAlive)
                {
                    ret = true;
                    this._inner.Remove(item);
                }
            }
            //return _inner.RemoveAll(l => !l.IsAlive) > 0;
            return ret;
        }

        private void CleanIfNeeded()
        {
            if(this._gcSentinel.IsAlive) return;

            this._gcSentinel.Target = new object();
            this.Purge();
        }

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="WeakCollection&lt;T&gt;" /> class that is empty and has the default
        ///     initial capacity.
        /// </summary>
        public WeakConccurentCollection() : this(null) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WeakCollection&lt;T&gt;" /> class that contains elements copied from
        ///     the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new collection.</param>
        public WeakConccurentCollection(IEnumerable<T> collection)
        {
            this._inner = collection == null
                ? new ConcurrentObservableCollection<WeakReference>()
                : new ConcurrentObservableCollection<WeakReference>(collection.Select(item => new WeakReference(item)));
            this._inner.CollectionChanged += this.InnerOnCollectionChanged;
        }

        private void InnerOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            NotifyCollectionChangedEventArgs notificationArgs = null;
            IList newArgList = null;
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                     newArgList = this.ConvertWeakReferenceList(notifyCollectionChangedEventArgs.NewItems);
                    if(newArgList.Count==0)return;
                    notificationArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newArgList);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    newArgList = this.ConvertWeakReferenceList(notifyCollectionChangedEventArgs.OldItems);
                    if (newArgList.Count == 0) return;
                    notificationArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, newArgList);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    notificationArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                default: throw new NotImplementedException();
            }

            this.CollectionChanged?.Invoke(this, notificationArgs);
        }

        private IList ConvertWeakReferenceList(IList weakList)
        {
            var newItems = new ArrayList();
            foreach(WeakReference item in weakList)
            {
                if(item.IsAlive)
                    newItems.Add(item.Target);
            }
            return newItems;
        }
        #endregion

        #region Implementation of IList<T>
        public int IndexOf(T item) { return this._inner.IndexOf(new WeakReference(item)); }
        public void Insert(int index, T item) { this._inner.Insert(index, new WeakReference(item)); }
        public void RemoveAt(int index) { this._inner.RemoveAt(index); }

        public T this[int index]
        {
            get => this._inner[index].Target as T;
            set => this._inner[index] = new WeakReference(value);
        }
        #endregion

        #region Implementation of INotifyCollectionChanged
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region Implementation of IObservableCollection
        public void Add(object item) { throw new NotImplementedException(); }
        public bool Remove(object item) { throw new NotImplementedException(); }

        #region Implementation of IObservable<out T>
        public IDisposable Subscribe(IObserver<T> observer) { throw new NotImplementedException(); }
        #endregion
        #endregion
        #endregion
    }
}