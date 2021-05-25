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
using Netfox.AppIdent.Misc;
using Netfox.AppIdent.Models;

namespace Netfox.AppIdent.Tests
{
    public class FeatureCollectionWrapperMock<TFeature> : IFeatureCollectionWrapper<TFeature> where TFeature : FeatureBase
    {
        public TFeature[] Features { get; }

        public FeatureCollectionWrapperMock(IEnumerable<TFeature> features) {
            this.Features = features as TFeature[] ?? features.ToArray();
        }
        public TFeature this[int i] => this.Features[i];

        public FeatureVector[] FeatureVectors {get { throw new NotImplementedException(); } }

        public PropertyInfo FeaturePropertyInfo { get { throw new NotImplementedException(); } }

        public IEnumerator<TFeature> GetEnumerator() => ((IEnumerable<TFeature>) this.Features).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Features.GetEnumerator();
    }
}