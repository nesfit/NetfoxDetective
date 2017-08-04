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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Netfox.Core.Database.PersistableJsonSeializable
{
    /// <summary>
    /// Baseclass that allows persisting of scalar values as a collection (which is not supported by EF 4.3)
    /// </summary>
    /// <typeparam name="T">Type of the single collection entry that should be persisted.</typeparam>
    [ComplexType]
    public class PersistableJsonSerializable<T> : IList<T>,ICollection<T>
    {
        /// <summary>
        /// The internal data container for the list data.
        /// </summary>
        private List<T> Data { get; set; } = new List<T>();

        public PersistableJsonSerializable() {}

        public PersistableJsonSerializable(IEnumerable<T> collection) { this.Data = collection.ToList(); }


        /// <summary>
        /// DO NOT Modify manually! This is only used to store/load the data.
        /// </summary>
        public string SerializedValue
        {
            get { return JsonConvert.SerializeObject(this.Data); }
            set
            {
                this.Data.Clear();

                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                this.Data = JsonConvert.DeserializeObject<List<T>>(value);
            }
        }

        #region ICollection<T> Members

        public void Add(T item) => this.Data.Add(item);
        public void AddRange(IEnumerable<T> collection) => this.Data.AddRange(collection);

        public void Clear() => this.Data.Clear();

        public bool Contains(T item) => this.Data.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => this.Data.CopyTo(array, arrayIndex);

        public int Count => this.Data.Count;

        public bool IsReadOnly => false;

        public bool Remove(T item) => this.Data.Remove(item);
        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() => this.Data.GetEnumerator();
        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => this.Data.GetEnumerator();
        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item) => this.Data.IndexOf(item);


        public void Insert(int index, T item) => this.Data.Insert(index, item);


        public void RemoveAt(int index) => this.Data.RemoveAt(index);

        public T this[int index]
        {
            get { return this.Data[index]; }
            set { this.Data[index] = value; }
        }
        #endregion
    }
}
