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
using Netfox.AppIdent.Misc;
using Netfox.Core.Enums;
using Netfox.Framework.Models;
using PacketDotNet;

namespace Netfox.AppIdent.Features.Bases
{
    public class TransportProtocolType : MandatoryFeatureBase
    {
        public TransportProtocolType() { }

        public TransportProtocolType(L7Conversation l7Conversation) : base(l7Conversation, DaRFlowDirection.non) { }

        public TransportProtocolType(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Discrete;
        public override FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(0, 256);

        public IPProtocolType L4ProtocolType { get; set; }

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            return this.L4ProtocolType == (sampleFeature as TransportProtocolType)?.L4ProtocolType? 0 : 1;
        }

        #region Overrides of FeatureBase
        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            this.L4ProtocolType = l7Conversation.L4ProtocolType;
            return (double) this.L4ProtocolType;
        }
        #endregion

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var firstSample = featureValues.FirstOrDefault() as TransportProtocolType;
            foreach(var featureValue in featureValues)
            {
                if(((TransportProtocolType) featureValue).L4ProtocolType != firstSample.L4ProtocolType)
                    throw new InvalidOperationException($"Mandatory condition have not been met: {this.GetType().Name}");
            }
            this.L4ProtocolType = firstSample.L4ProtocolType;
            this.FeatureValue = (double) this.L4ProtocolType;
            this.Weight = 1;
        }
    }
}