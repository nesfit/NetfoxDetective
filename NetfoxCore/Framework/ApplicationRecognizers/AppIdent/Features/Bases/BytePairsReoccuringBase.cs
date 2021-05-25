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
    public class BytePairsReoccuringBase : FeatureBase
    {
        public BytePairsReoccuringBase() { }
        public BytePairsReoccuringBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public BytePairsReoccuringBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

        [DataMember]
        private byte[] FeatureBytePairsValue { get; set; }

        [DataMember]
        private byte[][] ModelBytePairsValues { get; set; }

        public override double ComputeDistanceToProtocolModel(FeatureBase sampleFeature)
        {
            var featureValue = sampleFeature as BytePairsReoccuringBase;
            if(this.ModelBytePairsValues != null)
            {
                foreach(var modelBytePairsValue in this.ModelBytePairsValues) { if(modelBytePairsValue.SequenceEqual(featureValue.FeatureBytePairsValue)) { return 0; } }
            }
            else { if(this.FeatureBytePairsValue.SequenceEqual(featureValue.FeatureBytePairsValue)) { return 0; } }
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
                this.FeatureBytePairsValue = new byte[1]
                {
                    0
                };
                return -1;
            }

            var arrLength = firstMsg.GetLength(0);
            if(arrLength < 2)
            {
                this.FeatureBytePairsValue = new byte[1]
                {
                    0
                };
                return -1;
            }
            var byteReoccuring = 0;

            if(arrLength > 16) { arrLength = 16; }

            var returnArr = new byte[arrLength];
            if(firstMsg[0] == firstMsg[1]) { returnArr[0] = 1; }
            else { returnArr[0] = 0; }
            for(var i = 1; i < arrLength; i++)
            {
                if(firstMsg[i] == firstMsg[i - 1])
                {
                    byteReoccuring++;
                    returnArr[i] = 1;
                }
                else { returnArr[i] = 0; }
            }

            this.FeatureBytePairsValue = returnArr;
            return byteReoccuring;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            //this.ModelBytePairsValues = (from feature in featureValues as IFeatureCollectionWrapper<BytePairsReoccuringBase>
            //    group feature by feature.FeatureBytePairsValue
            //    into grp
            //    select grp.Key).Where(grp => grp != null).ToArray();

            var featuresTyped = featureValues as IFeatureCollectionWrapper<BytePairsReoccuringBase>;
            var hashAndCounts = featuresTyped.GroupBy(feature => feature.FeatureBytePairsValue, new ArrayEqualityComparer<byte>()).Select(g => new Entropy.EntropyItem<byte[]>
            {
                Key = g.Key,
                Count = g.Count()
            }).ToArray();

            this.ModelBytePairsValues = hashAndCounts.Select(hashAndCount => hashAndCount.Key).ToArray();

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