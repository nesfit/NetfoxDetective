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
using System.Reactive.Subjects;
using System.Threading;

namespace Netfox.Core.Collections
{
  
    public class ConcurrentHashSet<T> : IEnumerable<T>, ICollection<T>
    {
        private readonly HashSet<T> _hashSet = new HashSet<T>();
        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public ConcurrentHashSet() { }

        public ConcurrentHashSet(IEnumerable<T> initCollection)
        {
            this.AddRange(initCollection);
        }

        public void AddRange(IEnumerable<T> initCollection)
        {
            foreach (var item in initCollection) { this.Add(item); }
        }

        public class ConcurrentHashSetEnumerator<TT> : IEnumerator<TT>
        {
            private  TT[] _array;
            private  IEnumerator _enumerator;

            public ConcurrentHashSetEnumerator(TT[] array)
            {
                this._array = array;
                this._enumerator = array.GetEnumerator();
            }

            #region Implementation of IDisposable
            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() {
                this._array = null;
                this._enumerator = null;
            }
            #endregion

            #region Implementation of IEnumerator
            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
            ///     the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext() => this._enumerator.MoveNext();

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public void Reset() => this._enumerator.Reset();

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>
            ///     The element in the collection at the current position of the enumerator.
            /// </returns>
            public TT Current => (TT) this._enumerator.Current;

            /// <summary>
            ///     Gets the current element in the collection.
            /// </summary>
            /// <returns>
            ///     The current element in the collection.
            /// </returns>
            object IEnumerator.Current => this._enumerator.Current;
            #endregion
        }

        #region Implementation of ICollection<T> ...ish
        public bool Add(T item)
        {
            lock(this._lock)
            {
                var inserted = this._hashSet.Add(item);
                return inserted;
            }
        }

        public void Clear()
        {
            lock(this._lock) { this._hashSet.Clear(); }
        }

        public bool Contains(T item) { lock(this._lock) { return this._hashSet.Contains(item); } }

        public bool Remove(T item) {
            lock(this._lock)
            {
                var ret = this._hashSet.Remove(item);
                return ret;
            } }

        public int Count
        {
            get { lock(this._lock) { return this._hashSet.Count; } }
        }
        #endregion

        #region Implementation of IEnumerable
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            T[] arr;
            lock(this._lock) { arr = this._hashSet.ToArray(); }
            return new ConcurrentHashSetEnumerator<T>(arr);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        #endregion


        #region Implementation of ICollection<T>
        void ICollection<T>.Add(T item) { this.Add(item); }
        public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }
        public bool IsReadOnly { get; } = false;
        #endregion
    }
}