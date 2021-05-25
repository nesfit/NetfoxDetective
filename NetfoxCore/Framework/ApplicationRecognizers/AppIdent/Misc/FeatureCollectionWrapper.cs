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
using System.Linq;
using System.Reflection;
using Netfox.AppIdent.Features.Bases;
using Netfox.AppIdent.Models;

namespace Netfox.AppIdent.Misc
{
    public class FeatureCollectionWrapper<T> : IFeatureCollectionWrapper<T> where T : FeatureBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public FeatureCollectionWrapper(FeatureVector[] featureVectors)
        {
            this.FeatureVectors = featureVectors;
            var props = typeof(FeatureVector).GetProperties().Where(prop => prop.PropertyType == typeof(T));
            if(props.Count() != 1)
            {
                throw new InvalidOperationException("IdentificationFeatures cannot contain two properties of the same type - {props.FirstOrDefault()?.PropertyType}");
            }
            this.FeaturePropertyInfo = props.First();
        }

        public PropertyInfo FeaturePropertyInfo { get; }
        public FeatureVector[] FeatureVectors { get; }

        public T this[int i] => (T) this.FeaturePropertyInfo.GetValue(this.FeatureVectors[i], null);

        #region Implementation of IEnumerable
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.FeatureVectors.Select(feature => (T) this.FeaturePropertyInfo.GetValue(feature, null)).GetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}