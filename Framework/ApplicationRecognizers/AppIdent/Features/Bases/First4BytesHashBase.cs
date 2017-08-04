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
using System.Runtime.Serialization;
using Netfox.AppIdent.Misc;
using Netfox.Core.Enums;
using Netfox.Framework.Models;

namespace Netfox.AppIdent.Features.Bases
{
    [DataContract]
    public class First4BytesHashBase : FeatureBase
    {
        public First4BytesHashBase(double featureValue) : base(featureValue) { }
        public First4BytesHashBase() { }
        public First4BytesHashBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Discrete;
        public override FeatureValueRange FeatureValueRange { get; } = new FeatureValueRange(-1, 256);
        [DataMember]
        private byte[] FeatureHashValue { get; set; }

        [DataMember]
        private byte[][] ModelHashValues { get; set; }

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            var featureValue = sampleFeature as First4BytesHashBase;
            if(this.ModelHashValues != null)
            {
                foreach(var modelHashValue in this.ModelHashValues) { if(modelHashValue.SequenceEqual(featureValue.FeatureHashValue)) { return 0; } }
            }
            else { if(this.FeatureHashValue.SequenceEqual(featureValue.FeatureHashValue)) { return 0; } }

            return 1;
        }

        public override double ComputeFeature(L7Conversation l7Conversation, DaRFlowDirection flowDirection)
        {
            byte[] firstMsg = null;
            switch(flowDirection)
            {
                case DaRFlowDirection.up:
                    firstMsg = l7Conversation.UpFlowPDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                case DaRFlowDirection.down:
                    firstMsg = l7Conversation.DownFlowPDUs.FirstOrDefault()?.PDUByteArr;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            if(firstMsg == null)
            {
                this.FeatureHashValue = new byte[1]
                {
                    0
                };
                return -1;
            }

            this.FeatureHashValue = firstMsg.Take(4).ToArray();

            var hash = 0.0;

            foreach(var b in this.FeatureHashValue) { hash += b; }

            return hash / 4;
        }

        /// <summary>
        ///     http://math.stackexchange.com/questions/395121/how-entropy-scales-with-sample-size
        /// </summary>
        /// <param name="featureValues"></param>
        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            var featuresTyped = featureValues as IFeatureCollectionWrapper<First4BytesHashBase>;
            var hashAndCounts = featuresTyped.GroupBy(feature => feature.FeatureHashValue, new ArrayEqualityComparer<byte>()).Select(g => new Entropy.EntropyItem<byte[]>
            {
                Key = g.Key,
                Count = g.Count()
            }).ToArray();

            this.ModelHashValues = hashAndCounts.Select(hashAndCount => hashAndCount.Key).ToArray();

            var normEnth = 0.0;
            var n = hashAndCounts.Count();

            if(n != 1)
            {
                var enth = Entropy.Calculate(hashAndCounts);
                normEnth = enth / Math.Log(n);
            }
            this.Weight = 1 - normEnth;
        }
    }
}