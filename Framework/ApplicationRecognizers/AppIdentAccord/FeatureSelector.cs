using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AppIdent.Features.Bases;

namespace AppIdentAccord {
    public class FeatureSelector
    {
        public IReadOnlyList<Type> SelectedFeatures => this._selectedFeatures.AsReadOnly();
        private readonly List<Type> _selectedFeatures;
        public FeatureSelector(IEnumerable<Type> selectedFeatureTypes)
        {
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