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
using System.Collections.Concurrent;
using System.Linq;

namespace Netfox.Core.Collections
{
    /// <summary>
    ///     A collections which only holds weak references to the items.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    public class WeakConccurentCollection<T> : ICollection<T> where T : class
    {
        private readonly WeakReference _gcSentinel = new WeakReference(new object());
        private readonly ConcurrentBag<WeakReference> _inner;

        public void CopyTo(Array array, int index) { ((ICollection)this._inner).CopyTo(array, index); }

        public bool IsSynchronized => ((ICollection)this._inner).IsSynchronized;

        public object SyncRoot => ((ICollection)this._inner).SyncRoot;


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
                ? new ConcurrentBag<WeakReference>()
                : new ConcurrentBag<WeakReference>(collection.Select(item => new WeakReference(item)));
        }

        private IList ConvertWeakReferenceList(IList weakList)
        {
            var newItems = new ArrayList();
            foreach (WeakReference item in weakList)
            {
                if (item.IsAlive)
                    newItems.Add(item.Target);
            }
            return newItems;
        }
        #endregion


        #region Implementation of ICollection<T>
        public int Count => this._inner.Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            this._inner.Add(new WeakReference(item));
        }

        public void Clear()
        {
            this._inner.Clear();
        }

        public bool Contains(T item)
        {
            return this._inner.Any(w => ((T)w.Target) == item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if ((arrayIndex + this._inner.Count) > array.Length)
                throw new ArgumentException(
                    "The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

            var items = this._inner.Select(item => (T)item.Target).Where(value => value != null).ToArray();

            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            var enumerable = this._inner.Select(item => (T)item.Target).Where(value => value != null);
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }
        #endregion

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items) {
                this._inner.Add(new WeakReference(item));
            }
        }
    }
}