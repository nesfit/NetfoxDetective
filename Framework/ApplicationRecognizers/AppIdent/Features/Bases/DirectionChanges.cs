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
    public class DirectionChanges : FeatureBase
    {
        public DirectionChanges() { }

        public DirectionChanges(L7Conversation l7Conversation) : base(l7Conversation, DaRFlowDirection.non) { }

        // This constructor is also needed (called from FeatureBaseTests)
        public DirectionChanges(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, DaRFlowDirection.non) { }

        public DirectionChanges(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return Math.Abs(this.Normalize(this.FeatureValue) - this.Normalize(sampleFeature.FeatureValue));
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            var communicationDirectionChanges = 0;
            var l7Pdus = l7Conversation.L7PDUs as L7PDU[] ?? l7Conversation.L7PDUs?.ToArray();
            if(l7Pdus == null) { return -1; }

            var srcIp = l7Pdus.FirstOrDefault()?.SourceEndPoint;

            foreach(var pdu in l7Pdus)
            {
                if(srcIp.Equals(pdu.SourceEndPoint)) { continue; }

                communicationDirectionChanges++;
                srcIp = pdu.SourceEndPoint;
            }

            return communicationDirectionChanges;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
        }
    }
}