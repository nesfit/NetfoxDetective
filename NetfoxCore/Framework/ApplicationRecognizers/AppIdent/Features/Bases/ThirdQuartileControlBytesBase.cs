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
    public class ThirdQuartileControlBytesBase : FeatureBase
    {
        public ThirdQuartileControlBytesBase() { }
        public ThirdQuartileControlBytesBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public ThirdQuartileControlBytesBase(double featureValue) : base(featureValue) { }

        public override FeatureKind FeatureKind { get; } = FeatureKind.Continous;

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
                    frames = l7Conversation.UpFlowFrames;
                    break;
                case DaRFlowDirection.down:
                    frames = l7Conversation.DownFlowFrames;
                    break;
                case DaRFlowDirection.non:
                    frames = l7Conversation.Frames;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(flowDirection), flowDirection, null);
            }

            var Frames = frames as PmFrameBase[] ?? frames.ToArray();
            if(!Frames.Any()) { return -1; }

            var length = Frames.Count();
            if(length <= 1) { return 0; }
            var bytes = new double[length];

            for(var i = 0; i < length; i++)
            {
                if(Frames[i].L7Offset == -1) { bytes[i] = Frames[i].OriginalLengthWithoutPadding - (Frames[i].L4Offset - Frames[i].L2Offset); }
                else
                {
                    var dataSize = (int) (Frames[i].OriginalLengthWithoutPadding - (Frames[i].L7Offset - Frames[i].L2Offset));
                    bytes[i] = Frames[i].OriginalLengthWithoutPadding - (Frames[i].L4Offset - Frames[i].L2Offset) - dataSize;
                }
            }

            int medianIndex;
            if(bytes.Length == 1) { medianIndex = 1; }
            else { medianIndex = bytes.Length / 2; }
            Array.Sort(bytes);
            return GetMedian(bytes.Reverse().Take(medianIndex).ToArray());
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingStandarDeviation(featureValues);
            if(this.FeatureValue > 24) { this.Weight = 1; }
            if(this.FeatureValue == 20) { this.Weight = 0; }
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