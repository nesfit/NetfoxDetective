// Obtained from numl/Utils/DictionaryHelpers.cs

using System;
using System.Collections.Generic;

namespace Netfox.Core.Extensions
{
    public static class DictionaryExtension
    {
        public static void AddOrUpdate<K, V>(this Dictionary<K, List<V>> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                if (!dictionary[key].Contains(value))
                    dictionary[key].Add(value);
            }
            else
                dictionary[key] = new List<V> { value };
        }
        /// <summary>A Dictionary&lt;K,V&gt; extension method that adds an or update.</summary>
        /// <tparam name="K">Generic type parameter.</tparam>
        /// <tparam name="V">Generic type parameter.</tparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrUpdate<K, V>(this Dictionary<K, V> dictionary, K key, Func<V, V> value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value(dictionary[key]);
            else
                dictionary[key] = value(default(V));
        }
        /// <summary>A Dictionary&lt;K,V&gt; extension method that adds an or update.</summary>
        /// <tparam name="K">Generic type parameter.</tparam>
        /// <tparam name="V">Generic type parameter.</tparam>
        /// <param name="dictionary">The dictionary to act on.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="seed">The seed.</param>
        public static void AddOrUpdate<K, V>(this Dictionary<K, V> dictionary, K key, Func<V, V> value, V seed)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value(dictionary[key]);
            else
                dictionary[key] = value(seed);
        }
    }
}
