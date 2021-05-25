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
using Netfox.AppIdent.Metrics;
using Netfox.AppIdent.Misc;
using Netfox.Core.Enums;
using Netfox.Framework.Models;

namespace Netfox.AppIdent.Features.Bases
{
    public class NumberOfPacketsBase : FeatureBase
    {
        public NumberOfPacketsBase() { }
        public NumberOfPacketsBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public NumberOfPacketsBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            switch(flowDirection)
            {
                case DaRFlowDirection.up: return l7Conversation.UpFlowFrames.Count();
                case DaRFlowDirection.down: return l7Conversation.DownFlowFrames.Count();
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingStandarDeviation(featureValues);
        }
    }
}