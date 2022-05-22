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
using Numl.Utils;
using Netfox.AppIdent.Features.Bases;
using Netfox.AppIdent.Misc;

namespace Netfox.AppIdent.Metrics
{
    public class WeightMetrics
    {
        public static double WeightProbabilityMetric(IFeatureCollectionWrapper<FeatureBase> trainingFeatureVector)
        {
            var mostVal = (from i in trainingFeatureVector
                group i by i.FeatureValue
                into grp
                orderby grp.Count() descending
                select grp).First();

            var result = (double) mostVal.Count() / trainingFeatureVector.Count();

            //Console.WriteLine("Weight: " + result + "\n" + "Count " + trainingFeatureVector.Count());
            return result;
        }

        // https://en.wikipedia.org/wiki/Coefficient_of_variation
        public static double WeightUsingCoefficientOfVariation(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(feature => !feature.FeatureValue.Equals(-1.0));
            if(!features.Any() || features.Count() < 2) { return 0; }

            var stdDev = features.StandardDeviation(feature => feature.FeatureValue);

            if(stdDev.Equals(0)) { return 1; } // /Math.Sqrt(features.Count()); }

            var average = features.Average(feature => feature.FeatureValue);

            if(stdDev.CompareTo(average) > 0) { return 0; } //average / stdDev; }

            return stdDev / average;
        }

        public static double WeightUsingNormEntropy(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(feature => !feature.FeatureValue.Equals(-1.0));
            if(!features.Any() || features.Count() == 1) { return 0; }

            var values = (from i in featureValues
                group i by i.FeatureValue
                into grp
                orderby grp.Count() descending
                select grp);
            //if (featureValues.Count() == 1) { return 1; }

            int classes;
            var entropyValue = Entropy.Calculate(features.Select(feature => feature.FeatureValue), out classes);

            if(entropyValue.Equals(0)) return 1; //(double)1 / Math.Sqrt(classes);
            var result = 1 - entropyValue / Math.Log(classes);
            if(result.Equals(0)) return result;

            //return WeightUsingCoefficientOfVariation(featureValues);
            if(values.Count() < Math.Sqrt(classes))
            {
                var coeffOfVar = WeightUsingCoefficientOfVariation(featureValues);

                if(!coeffOfVar.Equals(0)) return coeffOfVar;
                //return coeffOfVar;
            }
            return result;
        }

        public static double WeightUsingStandarDeviation(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(feature => !feature.FeatureValue.Equals(-1.0));
            if(!features.Any() || features.Count() < 2) { return 0; }
            //if (featureValues.Count() < 2) { return 1; }

            var stdDev = features.StandardDeviation(feature => feature.FeatureValue);
            // if(stdDev.Equals(0)) { return 1;}
            return 1 / Math.Sqrt(1 + stdDev);
        }

        public static double WeightUsingVariance(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            if(featureValues.Count() < 2) { return 1; }

            var variance = featureValues.Variance(feature => feature.FeatureValue);
            if(variance.Equals(0)) { return (double) 1 / featureValues.Count(); }
            return 1 / variance;
        }

        public static double WeightUsingVarianceAndStandarDeviation(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var features = featureValues.Where(feature => !feature.FeatureValue.Equals(-1.0));
            if(!features.Any() || features.Count() < 2) { return 0; }
            //if (featureValues.Count() < 2) { return 1; }

            var stdDev = features.StandardDeviation(feature => feature.FeatureValue);

            if(stdDev.Equals(0)) { return 1 / Math.Sqrt(features.Count()); }

            var variance = features.Variance(feature => feature.FeatureValue);

            if(stdDev.CompareTo(variance) > 0) { return 1 - (stdDev); }
            return 1 - (stdDev / variance);
        }
    }
}