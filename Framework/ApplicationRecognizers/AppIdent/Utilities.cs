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

namespace Netfox.AppIdent
{
    public static class Utilities
    {
        public static double F_Measure(int tp, int fp, int p)
        {
            var prec = Precission(tp, fp);
            var rec = Recall(tp, p);
            if(prec == 0 || rec == 0) { return 0; }
            var res = 2 * prec * rec / (prec + rec);
            return res;
        }

        public static double MeanValue(List<double> valueList)
        {
            var tmpValue = 0.0;
            var count = 0;
            foreach(var value in valueList)
            {
                if(value.Equals(-1.0)) { continue; }
                tmpValue += value;
                count++;
            }
            return tmpValue / count;
        }

        public static double Precission(int tp, int fp)
        {
            if(tp + fp == 0) { return 0; }
            var res = (double) tp / (tp + fp);
            return res;
        }

        public static double Recall(int tp, int p)
        {
            if(p == 0) { return 0; }
            var res = (double) tp / p;
            return res;
        }

        //http://stackoverflow.com/questions/895929/how-do-i-determine-the-standard-deviation-stddev-of-a-set-of-values
        public static double StandardDeviation(List<int> valueList)
        {
            var M = 0.0;
            var S = 0.0;
            var k = 1;
            foreach(double value in valueList)
            {
                var tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double StandardDeviation(List<long> valueList)
        {
            var M = 0.0;
            var S = 0.0;
            var k = 1;
            foreach(double value in valueList)
            {
                if(value.Equals(-1.0)) { continue; }
                var tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double StandardDeviation(List<double> valueList)
        {
            var M = 0.0;
            var S = 0.0;
            var k = 1;
            foreach(var value in valueList)
            {
                if(value.Equals(-1.0)) { continue; }
                var tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }

        public static double Z_score(double oldValue, double mean, double stdDev)
        {
            if(stdDev != 0) { return (oldValue - mean) / stdDev; }

            return 0;
        }

        public static double Z_score(double oldValue, List<double> stats)
        {
            var stdDev = StandardDeviation(stats);

            if(stdDev != 0) { return (oldValue - stats.Average()) / stdDev; }

            return 0;
        }
    }
}