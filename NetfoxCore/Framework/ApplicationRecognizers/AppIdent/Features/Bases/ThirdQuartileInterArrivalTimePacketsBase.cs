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
using Netfox.AppIdent.Metrics;
using Netfox.AppIdent.Misc;
using Netfox.Core.Enums;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.AppIdent.Features.Bases
{
    public class ThirdQuartileInterArrivalTimePacketsBase : FeatureBase
    {
        public ThirdQuartileInterArrivalTimePacketsBase() { }
        public ThirdQuartileInterArrivalTimePacketsBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public ThirdQuartileInterArrivalTimePacketsBase(double featureValue) : base(featureValue) { }
        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;
        public override double WeightBias { get; } = 0.3;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            IEnumerable<PmFrameBase> frames;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    frames = l7Conversation.UpFlowFrames.OrderBy(i => i.FirstSeen);
                    break;
                case DaRFlowDirection.down:
                    frames = l7Conversation.DownFlowFrames.OrderBy(i => i.FirstSeen);
                    break;
                case DaRFlowDirection.non:
                    frames = l7Conversation.Frames.OrderBy(i => i.FirstSeen);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            var frameArray = frames as PmFrameBase[] ?? frames.ToArray();
            if(!frameArray.Any()) { return -1; }

            var length = frameArray.Count();
            if(length <= 1) { return 0; }

            var times = new double[length - 1];

            for(var i = 0; i < length - 1; i++) { times[i] = Math.Abs((frameArray[i + 1].FirstSeen - frameArray[i].FirstSeen).TotalSeconds); }

            int medianIndex;
            if(times.Length == 1) { medianIndex = 1; }
            else { medianIndex = times.Length / 2; }
            Array.Sort(times);
            return GetMedian(times.Reverse().Take(medianIndex).ToArray());
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }

        public static double GetMedian(double[] source)
        {
            var temp = source;
            Array.Sort(temp);

            var count = temp.Length;
            if(count == 0) { throw new InvalidOperationException("Empty collection"); }
            if(count % 2 == 0)
            {
                var a = temp[count / 2 - 1];
                var b = temp[count / 2];
                return (a + b) / 2.0;
            }
            return temp[count / 2];
        }
    }
}