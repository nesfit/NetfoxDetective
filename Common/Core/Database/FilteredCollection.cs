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
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Netfox.Core.Database
{
    [Serializable]
    public class FilteredCollection<T> : ICollection<T>

    {
        private readonly DbCollectionEntry collectionEntry;

        private readonly Func<T, Boolean> compiledFilter;

        private ICollection<T> collection;

        public FilteredCollection(ICollection<T> collection, DbCollectionEntry collectionEntry, Expression<Func<T, Boolean>> filter)

        {
            this.Filter = filter;

            this.collection = collection ?? new HashSet<T>();

            this.collectionEntry = collectionEntry;

            this.compiledFilter = filter.Compile();

            if(collection != null)
            {
                foreach(var entity in collection) { this.collection.Add(entity); }
                this.collectionEntry.CurrentValue = this;
            }

            else
            {
                this.LoadIfNecessary();
            }
        }

        public Expression<Func<T, Boolean>> Filter { get; private set; }

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() { return ((this as IEnumerable<T>).GetEnumerator()); }
        #endregion

        #region IEnumerable<T> Members
        IEnumerator<T> IEnumerable<T>.GetEnumerator()

        {
            this.LoadIfNecessary();

            return (this.collection.GetEnumerator());
        }
        #endregion

        protected void LoadIfNecessary()

        {
            if(this.collectionEntry.IsLoaded == false)

            {
                var query = this.collectionEntry.Query().Cast<T>().Where(this.Filter);


                this.collection = query.ToList();


                this.collectionEntry.CurrentValue = this;


                var _internalCollectionEntry =
                    this.collectionEntry.GetType().GetField("_internalCollectionEntry", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(this.collectionEntry);

                var _relatedEnd =
                    _internalCollectionEntry.GetType().BaseType.GetField("_relatedEnd", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(_internalCollectionEntry);

                _relatedEnd.GetType().GetField("_isLoaded", BindingFlags.NonPublic|BindingFlags.Instance).SetValue(_relatedEnd, true);
            }
        }

        protected void ThrowIfInvalid(T entity) { if(this.compiledFilter(entity) == false) { throw (new ArgumentException("entity")); } }

        #region ICollection<T> Members
        void ICollection<T>.Add(T item)

        {
            this.LoadIfNecessary();

            this.ThrowIfInvalid(item);

            this.collection.Add(item);
        }

        void ICollection<T>.Clear()

        {
            this.LoadIfNecessary();

            this.collection.Clear();
        }

        Boolean ICollection<T>.Contains(T item)

        {
            this.LoadIfNecessary();

            return (this.collection.Contains(item));
        }

        void ICollection<T>.CopyTo(T[] array, Int32 arrayIndex)

        {
            this.LoadIfNecessary();

            this.collection.CopyTo(array, arrayIndex);
        }

        Int32 ICollection<T>.Count

        {
            get

            {
                this.LoadIfNecessary();

                return (this.collection.Count);
            }
        }

        Boolean ICollection<T>.IsReadOnly

        {
            get

            {
                this.LoadIfNecessary();

                return (this.collection.IsReadOnly);
            }
        }

        Boolean ICollection<T>.Remove(T item)

        {
            this.LoadIfNecessary();

            return (this.collection.Remove(item));
        }
        #endregion
    }
}