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
using L7Conversation = Netfox.Framework.Models.L7Conversation;
using L7PDU = Netfox.Framework.Models.L7PDU;

namespace Netfox.AppIdent.Features.Bases
{
    public class MinInterArrivalTimeBase : FeatureBase
    {
        public MinInterArrivalTimeBase() { }
        public MinInterArrivalTimeBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public MinInterArrivalTimeBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;
        public override double WeightBias { get; } = 0.3;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            IEnumerable<L7PDU> pdus;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    pdus = l7Conversation.UpFlowPDUs;
                    break;
                case DaRFlowDirection.down:
                    pdus = l7Conversation.DownFlowPDUs;
                    break;
                case DaRFlowDirection.non:
                    pdus = l7Conversation.L7PDUs;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            var l7Pdus = pdus as L7PDU[] ?? pdus.ToArray();
            if(!l7Pdus.Any()) { return -1; }

            var length = l7Pdus.Count();
            var pdusArray = l7Pdus.ToArray();

            if(length <= 1) { return 0; }

            var minTime = Math.Abs((pdusArray[1].FirstSeen - pdusArray[0].FirstSeen).TotalSeconds);

            for(var i = 1; i < length - 1; i++)
            {
                var interArrivalTime = Math.Abs((pdusArray[i + 1].FirstSeen - pdusArray[i].FirstSeen).TotalSeconds);

                if(minTime.CompareTo(interArrivalTime) > 0) { minTime = interArrivalTime; }
            }

            return minTime;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}