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
using Netfox.AppIdent.Misc;
using Netfox.Core.Enums;
using Netfox.Framework.Models;

namespace Netfox.AppIdent.Features.Bases
{
    public class First3BytesEqualBase : FeatureBase
    {
        public First3BytesEqualBase() { }
        public First3BytesEqualBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public First3BytesEqualBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Discrete;
        public override FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(0, 1);
        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature) { return this.FeatureValue.Equals(sampleFeature.FeatureValue)? 0 : 1; }

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
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }


            var l7Pdus = pdus as L7PDU[] ?? pdus.ToArray();
            if(l7Pdus.Count() <= 1) { return 0; }

            var numBytes = 3;
            var firstPDUBytesCount = l7Pdus.First().PDUByteArr.Length;

            if(firstPDUBytesCount < numBytes) { numBytes = firstPDUBytesCount; }

            var pattern = l7Pdus.First().PDUByteArr.Take(numBytes).ToArray();

            foreach(var pdu in l7Pdus)
            {
                if(pdu.PDUByteArr.Length < numBytes) { return 0; }

                if(pattern.SequenceEqual(pdu.PDUByteArr.Take(3))) { continue; }
                return 0;
            }
            return 1;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var trueValues = featureValues.Count(feature => feature.FeatureValue.Equals(1.0));
            var falseValues = featureValues.Count(feature => feature.FeatureValue.Equals(0.0));

            this.FeatureValue = (trueValues > falseValues)? 1 : 0;

            if(trueValues == 0 || falseValues == 0) { this.Weight = 1; } // /(double)Math.Sqrt(trueValues+falseValues); }
            else if(trueValues < falseValues) { this.Weight = 1 - (trueValues / (double) falseValues); }
            else { this.Weight = 1 - (falseValues / (double) trueValues); }
        }
    }
}