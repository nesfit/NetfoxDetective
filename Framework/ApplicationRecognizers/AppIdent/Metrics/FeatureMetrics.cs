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
using System.Linq;
using Netfox.AppIdent.Features.Bases;
using Netfox.AppIdent.Misc;

namespace Netfox.AppIdent.Metrics
{
    public static class FeatureMetrics
    {
        public static double DistanceMetric(FeatureBase value, IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(value.FeatureValue.Equals(-1.0) || !featureValues.Any()) { return 0; }
            var result = Math.Abs(value.FeatureValue - featureValues.First().FeatureValue);
            foreach(var stat in featureValues.Skip(1))
            {
                var d = Math.Abs(value.FeatureValue - stat.FeatureValue);
                if(d < result) { result = d; }
            }
            //Console.WriteLine("Distance " + result + " value: " + value.FeatureValue);
            return result;
        }

        public static double FeatureMetricAverage(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(feature => !feature.FeatureValue.Equals(-1.0)).ToArray();
            return !features.Any()? 0 : features.Average(feature => feature.FeatureValue);
        }

        public static double First3BEqualMetric(FeatureBase value, IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(value.FeatureValue.Equals(-1.0) || !featureValues.Any()) { return 0; }
            var tmpTrue = featureValues.Count(x => x.FeatureValue.Equals(1.0));
            var tmpFalse = featureValues.Count(x => x.FeatureValue.Equals(0.0));
            Console.WriteLine("3Bequal " + tmpTrue + " " + tmpFalse + " FeatureValue: " + value.FeatureValue);
            if((tmpTrue > tmpFalse && value.FeatureValue.Equals(1.0)) || (tmpTrue < tmpFalse && value.FeatureValue.Equals(0.0))) { return 0.0; }

            return 1.0;
        }

        public static double IsValueInStatsMetric(FeatureBase value, IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(value.FeatureValue.Equals(-1.0) || !featureValues.Any()) { return 0; }

            return featureValues.Any(feature => value.FeatureValue.Equals(feature.FeatureValue))? 0.0 : 1.0;
        }
    }
}