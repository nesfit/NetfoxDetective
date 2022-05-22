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

namespace Netfox.AppIdent.Normalization
{
    public class Normalizator
    {
        public FeatureVector Mean { get; internal set; }

        public FeatureVector StdDev { get; internal set; }
        public void Initialize(FeatureVector[] stats) { this.ComputeMeanAndStdDeviation(stats); }

        public void Normalize(FeatureVector[] stats)
        {
            foreach(var stat in stats)
            {
                foreach(var property in stat.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
                {
                    var name = property.Name;
                    var feature = property.GetValue(stat, null);
                    var propertyInfo = feature.GetType().GetProperty(nameof(FeatureBase.FeatureValue));
                    var propertyValue = (double) propertyInfo.GetValue(feature, null);
                    if(!propertyValue.Equals(-1.0))
                    {
                        propertyInfo.SetValue(feature, Utilities.Z_score(propertyValue, this.GetPropValue(this.Mean, name), this.GetPropValue(this.StdDev, name)), null);
                    }
                }
            }
        }

        public void Normalize(FeatureVector stat)
        {
            var t = stat.GetType();
            foreach(var property in t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
            {
                var name = property.Name;
                var feature = property.GetValue(stat, null);
                var propertyInfo = feature.GetType().GetProperty(nameof(FeatureBase.FeatureValue));
                var propertyValue = (double) propertyInfo.GetValue(feature, null);
                if(!propertyValue.Equals(-1.0))
                {
                    propertyInfo.SetValue(feature,
                        Utilities.Z_score((double) propertyInfo.GetValue(feature, null), this.GetPropValue(this.Mean, name), this.GetPropValue(this.StdDev, name)), null);
                }
            }
        }

        private void ComputeMeanAndStdDeviation(FeatureVector[] stats)
        {
            this.Mean = new FeatureVector();
            this.StdDev = new FeatureVector();

            //roztrideni statistik, <statistika, seznam hodnot>
            var tmpDict = new Dictionary<string, List<double>>();

            var t = stats.First().GetType();
            foreach(var stat in stats)
            {
                foreach(var property in t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
                {
                    var name = property.Name;
                    var tmpProperty = property.GetValue(stat, null);
                    if(!tmpDict.ContainsKey(name)) { tmpDict.Add(name, new List<double>()); }
                    var propertyValue = (double) tmpProperty.GetType().GetProperty(nameof(FeatureBase.FeatureValue)).GetValue(tmpProperty);
                    if(!propertyValue.Equals(-1.0)) { tmpDict[name].Add(propertyValue); }
                }
            }

            //TODO paralel? 
            foreach(var property in t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(FeatureStatisticalAttribute))))
            {
                var name = property.Name;

                property.SetValue(this.Mean, Activator.CreateInstance(property.PropertyType, Utilities.MeanValue(tmpDict[name])), null);
                property.SetValue(this.StdDev, Activator.CreateInstance(property.PropertyType, Utilities.StandardDeviation(tmpDict[name])), null);
            }
        }

        private double GetPropValue(object src, string propName)
        {
            var tmpProperty = src.GetType().GetProperty(propName).GetValue(src, null);
            return (double) tmpProperty.GetType().GetProperty(nameof(FeatureBase.FeatureValue)).GetValue(tmpProperty);
        }
    }
}