using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Netfox.Core.Database.PersistableJsonSerializable
{
    /// <summary>
    /// Baseclass that allows persisting of scalar values as a collection (which is not supported by EF 4.3)
    /// </summary>
    /// <typeparam name="T">Type of the single collection entry that should be persisted.</typeparam>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="Tvalue">Type of value</typeparam>
    [ComplexType]
    public class PersistableJsonSerializableDictionary<TKey,Tvalue> : IDictionary<TKey, Tvalue>
    {
        /// <summary>
        /// The internal data container for the list data.
        /// </summary>
        private Dictionary<TKey, Tvalue> Data { get; set; }
        public PersistableJsonSerializableDictionary(Dictionary<TKey, Tvalue> initDictionary)
        {
            this.Data = initDictionary;
        }

        public PersistableJsonSerializableDictionary()
        {
            this.Data = new Dictionary<TKey, Tvalue>();
        }

        public PersistableJsonSerializableDictionary(StringComparer ordinalIgnoreCase)
        {
            //if (typeof(string).IsAssignableFrom(typeof(TKey)))
            //    this.Data = new Dictionary<string, Tvalue>(ordinalIgnoreCase);
            //else
            this.Data = new Dictionary<TKey, Tvalue>();
            
        }

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

                this.Data = JsonConvert.DeserializeObject<Dictionary<TKey, Tvalue>>(value);
            }
        }

        #region Implementation of IEnumerable
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, Tvalue>> GetEnumerator() => this.Data.GetEnumerator(); 

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        #endregion

        #region Implementation of ICollection<KeyValuePair<Tkey,Tvalue>>
        /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public void Add(KeyValuePair<TKey, Tvalue> item) => this.Data.Add(item.Key,item.Value);

        /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
        public void Clear() => this.Data.Clear();

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(KeyValuePair<TKey, Tvalue> item)=> this.Data.Contains(item);
        
        /// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="array" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(KeyValuePair<TKey, Tvalue>[] array, int arrayIndex) { throw new NotImplementedException(); }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, Tvalue> item) => this.Data.Remove(item.Key);

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count => this.Data.Count;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly => false;
        #endregion

        #region Implementation of IDictionary<Tkey,Tvalue>
        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool ContainsKey(TKey key) => this.Data.ContainsKey(key);

        /// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public void Add(TKey key, Tvalue value) => this.Data.Add(key,value);

        /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public bool Remove(TKey key) => this.Data.Remove(key);

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        public bool TryGetValue(TKey key, out Tvalue value) => this.Data.TryGetValue(key,out value);

        /// <summary>Gets or sets the element with the specified key.</summary>
        /// <returns>The element with the specified key.</returns>
        /// <param name="key">The key of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> is not found.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2" /> is read-only.</exception>
        public Tvalue this[TKey key]
        {
            get { return this.Data[key]; }
            set { this.Data[key] = value; }
        }

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<TKey> Keys => this.Data.Keys;

        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public ICollection<Tvalue> Values => this.Data.Values;
        #endregion
    }
}