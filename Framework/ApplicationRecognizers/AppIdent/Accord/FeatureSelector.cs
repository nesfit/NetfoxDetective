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
using System.Collections.Generic;
using System.Linq;
using Netfox.AppIdent.Features.Bases;
using Netfox.AppIdent.Models;

namespace Netfox.AppIdent.Accord {
    [Serializable]
    public class FeatureSelector
    {
        public IReadOnlyList<Type> SelectedFeatures => this._selectedFeatures.AsReadOnly();
        private readonly List<Type> _selectedFeatures;
        public FeatureSelector(IEnumerable<Type> selectedFeatureTypes)
        {
            this._selectedFeatures = new List<Type>(selectedFeatureTypes);
        }
        public FeatureSelector()
        {
            var selectedFeatureTypes = typeof(FeatureVector).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute)))
                .Select(p => p.PropertyType);
           this._selectedFeatures = new List<Type>(selectedFeatureTypes);
        }

        public FeatureSelector(IEnumerable<Type> selectedFeatureTypes, bool skipDiscrete)
        {
            if(!skipDiscrete)
            {
                this._selectedFeatures = new List<Type>(selectedFeatureTypes);
                return;
            }

            this._selectedFeatures = new List<Type>();
            foreach (var selectedFeatureType in selectedFeatureTypes)
            {
                var feature = Activator.CreateInstance(selectedFeatureType) as FeatureBase;
                 if(feature.FeatureKind == FeatureKind.Discrete) continue;
                this._selectedFeatures.Add(selectedFeatureType);
            }
        }

        public void RemoveFeature(Type feature) => this._selectedFeatures.Remove(feature);
    }
}