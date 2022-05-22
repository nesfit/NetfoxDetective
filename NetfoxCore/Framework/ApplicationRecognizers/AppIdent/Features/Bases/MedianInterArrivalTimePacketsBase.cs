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
    public class MedianInterArrivalTimePacketsBase : FeatureBase
    {
        public MedianInterArrivalTimePacketsBase() { }
        public MedianInterArrivalTimePacketsBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public MedianInterArrivalTimePacketsBase(double featureValue) : base(featureValue) { }
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

            var pmFrameBases = frames as PmFrameBase[] ?? frames.ToArray();
            if(pmFrameBases == null || !pmFrameBases.Any()) { return -1; }

            var length = pmFrameBases.Count();
            if(length <= 1) { return 0; }

            var halfIndex = length / 2;
            var median = 0.0;
            var times = new double[length];

            for(var i = 0; i < length - 1; i++) { times[i] = (pmFrameBases[i + 1].FirstSeen - pmFrameBases[i].FirstSeen).TotalSeconds; }

            var sortedTimes = times.OrderBy(n => n);
            if((length % 2) == 0) { median = (sortedTimes.ElementAt(halfIndex) + sortedTimes.ElementAt(halfIndex - 1)) / 2; }
            else { median = sortedTimes.ElementAt(halfIndex); }
            return median;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}