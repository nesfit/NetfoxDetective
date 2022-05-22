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
    public class MaxControlBytesBase : FeatureBase
    {
        public MaxControlBytesBase() { }
        public MaxControlBytesBase(L7Conversation l7Conversation, DaRFlowDirection flowDirection) : base(l7Conversation, flowDirection) { }
        public MaxControlBytesBase(double featureValue) : base(featureValue) { }

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
            if(!frames.Any()) { return -1; }
            double controlBytes;
            if(frames.First().L7Offset == -1) { controlBytes = frames.First().OriginalLengthWithoutPadding - (frames.First().L4Offset - frames.First().L2Offset); }
            else
            {
                var dataSize = (int) (frames.First().OriginalLengthWithoutPadding - (frames.First().L7Offset - frames.First().L2Offset));
                controlBytes = frames.First().OriginalLengthWithoutPadding - (frames.First().L4Offset - frames.First().L2Offset) - dataSize;
            }
            foreach(var frame in frames)
            {
                double tmpControlBytes;
                if(frame.L7Offset == -1) { tmpControlBytes = frame.OriginalLengthWithoutPadding - (frame.L4Offset - frame.L2Offset); }
                else
                {
                    var dataSize = (int) (frame.OriginalLengthWithoutPadding - (frame.L7Offset - frame.L2Offset));
                    tmpControlBytes = frame.OriginalLengthWithoutPadding - (frame.L4Offset - frame.L2Offset) - dataSize;
                }
                if(controlBytes.CompareTo(tmpControlBytes) < 0) { controlBytes = tmpControlBytes; }
            }
            return controlBytes;
        }

        public override void ComputeFeatureForProtocolModel(IFeatureCollectionWrapper<FeatureBase> featureValues)
        {
            this.FeatureValue = FeatureMetrics.FeatureMetricAverage(featureValues);
            this.Weight = WeightMetrics.WeightUsingNormEntropy(featureValues);
            if(this.FeatureValue > 24) { this.Weight = 1; }
            if(this.FeatureValue == 20) { this.Weight = 0; }
            //if (this.FeatureValue == 32) { this.Weight = 0.5; }
        }
    }
}