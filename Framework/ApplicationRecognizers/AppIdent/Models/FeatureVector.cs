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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using numl.Model;
using Netfox.AppIdent.EPI;
using Netfox.AppIdent.Features;
using Netfox.AppIdent.Features.Bases;
#pragma warning disable 169

namespace Netfox.AppIdent.Models
{
    [DataContract]
    public class FeatureVector
    {
        private string _applicationProtocolName;
        private ByteFrequencyDownFlow _byteFrequencyDownFlow;
        private ByteFrequencyUpFlow _byteFrequencyUpFlow;
        private BytePairsReoccuringDownFlow _bytePairsReoccuringDownFlow;
        private BytePairsReoccuringUpFlow _bytePairsReoccuringUpFlow;
        private DirectionChanges _directionChanges;
        private Duration _duration;
        private DurationFlow _durationFlow;
        private EntropyDownFlow _entropyDownFlow;
        private EntropyUpFlow _entropyUpFlow;
        private FINPacketsDown _finPacketsDown;
        private FINPacketsUp _finPacketsUp;
        private First3BytesEqualDownFlow _first3BytesEqualDownFlow;
        private First3BytesEqualUpFlow _first3BytesEqualUpFlow;
        private First4BytesHashDownFlow _first4BytesHashDownFlow;
        private First4BytesHashUpFlow _first4BytesHashUpFLow;
        private FirstBitPositionDownFlow _firstBitPositionDownFlow;
        private FirstBitPositionUpFlow _firstBitPositionUpFlow;
        private FirstPayloadSize _firstPayloadSize;
        private FirstQuartileControlBytesDown _firstQuartileControlBytesDown;
        private FirstQuartileControlBytesUp _firstQuartileControlBytesUp;
        private FirstQuartileControlBytesUpAndDown _firstQuartileControlBytesUpAndDown;
        private FirstQuartileInterArrivalTimeDown _firstQuartileInterArrivalTimeDown;
        private FirstQuartileInterArrivalTimePacketsDown _firstQuartileInterArrivalTimePacketsDown;
        private FirstQuartileInterArrivalTimePacketsUp _firstQuartileInterArrivalTimePacketsUp;
        private FirstQuartileInterArrivalTimePacketsUpAndDown _firstQuartileInterArrivalTimePacketsUpAndDown;
        private FirstQuartileInterArrivalTimeUp _firstQuartileInterArrivalTimeUp;
        private FirstQuartileInterArrivalTimeUpAndDown _firstQuartileInterArrivalTimeUpAndDown;
        private MaxControlBytesDown _maxControlBytesDown;
        private MaxControlBytesUp _maxControlBytesUp;
        private MaxControlBytesUpAndDown _maxControlBytesUpAndDown;
        private MaxInterArrivalTimeDownFlow _maxInterArrivalTimeDownFlow;
        private MaxInterArrivalTimePacketsDownFlow _maxInterArrivalTimePacketsDownFlow;
        private MaxInterArrivalTimePacketsUpAndDownFlow _maxInterArrivalTimePacketsUpAndDownFlow;
        private MaxInterArrivalTimePacketsUpFlow _maxInterArrivalTimePacketsUpFlow;
        private MaxInterArrivalTimeUpFlow _maxInterArrivalTimeUpFlow;
        private MaxPacketLengthDownFlow _maxPacketLengthDownFlow;
        private MaxPacketLengthUpFlow _maxPacketLengthUpFlow;
        private MaxSegmentSizeDown _maxSegmentSizeDown;
        private MaxSegmentSizeUp _maxSegmentSizeUp;
        private MedianInterArrivalTimeDownFlow _meadiaInterArrivalTimeDownFlow;
        private MeanControlBytesDown _meanControlBytesDown;
        private MeanControlBytesUp _meanControlBytesUp;
        private MeanControlBytesUpAndDown _meanControlBytesUpAndDown;
        private MeanInterArrivalTimeDownFlow _meanInterArrivalTimeDownFlow;
        private MeanInterArrivalTimePacketsDownFlow _meanInterArrivalTimePacketsDownFlow;
        private MeanInterArrivalTimePacketsUpAndDownFlow _meanInterArrivalTimePacketsUpAndDownFlow;
        private MeanInterArrivalTimePacketsUpFlow _meanInterArrivalTimePacketsUpFlow;
        private MeanInterArrivalTimeUpFlow _meanInterArrivalTimeUpFlow;
        private MeanPacketLengthDownFlow _meanPacketLengthDownFlow;
        private MeanPacketLengthUpFlow _meanPacketLengthUpFlow;
        private MedianControlBytesUp _medianControlBytesUp;
        private MedianControlBytesUpAndDown _medianControlBytesUpAndDown;
        private MedianControlBytesDown _medianControlDown;
        private MedianInterArrivalTimePacketsDownFlow _medianInterArrivalTimePacketsDownFlow;
        private MedianInterArrivalTimePacketsUpAndDownFlow _medianInterArrivalTimePacketsUpAndDownFlow;
        private MedianInterArrivalTimePacketsUpFlow _medianInterArrivalTimePacketsUpFlow;
        private MedianInterArrivalTimeUpAndDownFlow _medianInterArrivalTimeUpAndDownFlow;
        private MedianInterArrivalTimeUpFlow _medianInterArrivalTimeUpFlow;
        private MinControlBytesUp _minControlBytesUp;
        private MinControlBytesUpAndDown _minControlBytesUpAndDown;
        private MinControlBytesDown _minControlDown;
        private MinInterArrivalTimeDownFlow _minInterArrivalTimeDownFlow;
        private MinInterArrivalTimePacketsDownFlow _minInterArrivalTimePacketsDownFlow;
        private MinInterArrivalTimePacketsUpAndDownFlow _minInterArrivalTimePacketsUpAndDownFlow;
        private MinInterArrivalTimePacketsUpFlow _minInterArrivalTimePacketsUpFlow;
        private MinInterArrivalTimeUpFlow _minInterArrivalTimeUpFlow;
        private MinPacketLengthDownFlow _minPacketLengthDownFlow;
        private MinPacketLengthUpFlow _minPacketLengthUpFlow;
        private MinSegmentSizeDown _minSegmentSizeDown;
        private MinSegmentSizeUp _minSegmentSizeUp;
        private NumberOfBytesDownFlow _numberOfBytesDownFlow;
        private NumberOfBytesUpFlow _numberOfBytesUpFlow;
        private NumberOfPacketsDownFlow _numberOfPacketsDownFlow;
        private NumberOfPacketsPerTimeDown _numberOfPacketsPerTimeDown;
        private NumberOfPacketsPerTimeUp _numberOfPacketsPerTimeUp;
        private NumberOfPacketsUpFlow _numberOfPacketsUpFlow;
        private PacketLengthDistributionDownFlow _packetLengthDistributionDownFlow;
        private PacketLengthDistributionUpFlow _packetLengthDistributionUpFlow;
        private PortDown _portDown;
        private PortUp _portUp;
        private EPIProtocolModel _predictedModel;
        private Dictionary<EPIProtocolModel, double> _protocolModelDifferences;
        private PUSHPacketsDown _pushPacketsDown;
        private PUSHPacketsUp _pushPacketsUp;
        private SYNPacketsDown _synPacketsDown;
        private SYNPacketsUp _synPacketsUp;
        private ThirdQuartileControlBytesDown _thirdQuartileControlBytesDown;
        private ThirdQuartileControlBytesUp _thirdQuartileControlBytesUp;
        private ThirdQuartileControlBytesUpAndDown _thirdQuartileControlBytesUpAndDown;
        private ThirdQuartileInterArrivalTimeDown _thirdQuartileInterArrivalTimeDown;
        private ThirdQuartileInterArrivalTimePacketsDown _thirdQuartileInterArrivalTimePacketsDown;
        private ThirdQuartileInterArrivalTimePacketsUp _thirdQuartileInterArrivalTimePacketsUp;
        private ThirdQuartileInterArrivalTimePacketsUpAndDown _thirdQuartileInterArrivalTimePacketsUpAndDown;
        private ThirdQuartileInterArrivalTimeUp _thirdQuartileInterArrivalTimeUp;
        private ThirdQuartileInterArrivalTimeUpAndDown _thirdQuartileInterArrivalTimeUpAndDown;
        private TransportProtocolType _transportProtocolType;
        private string _applicationProtocolNameFull;
        private string _label;

        [DataMember]
        [FeatureStatistical]
        public MinInterArrivalTimePacketsUpAndDownFlow MinInterArrivalTimePacketsUpAndDownFlow
        {
            get => this._minInterArrivalTimePacketsUpAndDownFlow ?? (this._minInterArrivalTimePacketsUpAndDownFlow = new MinInterArrivalTimePacketsUpAndDownFlow());
            set => this._minInterArrivalTimePacketsUpAndDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinInterArrivalTimePacketsUpAndDownFlowML => this.MinInterArrivalTimePacketsUpAndDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MinInterArrivalTimePacketsUpFlow MinInterArrivalTimePacketsUpFlow
        {
            get => this._minInterArrivalTimePacketsUpFlow ?? (this._minInterArrivalTimePacketsUpFlow = new MinInterArrivalTimePacketsUpFlow());
            set => this._minInterArrivalTimePacketsUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinInterArrivalTimePacketsUpFlowML => this.MinInterArrivalTimePacketsUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MinInterArrivalTimePacketsDownFlow MinInterArrivalTimePacketsDownFlow
        {
            get => this._minInterArrivalTimePacketsDownFlow ?? (this._minInterArrivalTimePacketsDownFlow = new MinInterArrivalTimePacketsDownFlow());
            set => this._minInterArrivalTimePacketsDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinInterArrivalTimePacketsDownFlowML => this.MinInterArrivalTimePacketsDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MedianInterArrivalTimePacketsUpAndDownFlow MedianInterArrivalTimePacketsUpAndDownFlow
        {
            get => this._medianInterArrivalTimePacketsUpAndDownFlow ?? (this._medianInterArrivalTimePacketsUpAndDownFlow = new MedianInterArrivalTimePacketsUpAndDownFlow());
            set => this._medianInterArrivalTimePacketsUpAndDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianInterArrivalTimePacketsUpAndDownFlowML => this.MedianInterArrivalTimePacketsUpAndDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MedianInterArrivalTimePacketsUpFlow MedianInterArrivalTimePacketsUpFlow
        {
            get => this._medianInterArrivalTimePacketsUpFlow ?? (this._medianInterArrivalTimePacketsUpFlow = new MedianInterArrivalTimePacketsUpFlow());
            set => this._medianInterArrivalTimePacketsUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianInterArrivalTimePacketsUpFlowML => this.MedianInterArrivalTimePacketsUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MedianInterArrivalTimePacketsDownFlow MedianInterArrivalTimePacketsDownFlow
        {
            get => this._medianInterArrivalTimePacketsDownFlow ?? (this._medianInterArrivalTimePacketsDownFlow = new MedianInterArrivalTimePacketsDownFlow());
            set => this._medianInterArrivalTimePacketsDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianInterArrivalTimePacketsDownFlowML => this.MedianInterArrivalTimePacketsDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MedianInterArrivalTimeUpAndDownFlow MedianInterArrivalTimeUpAndDownFlow
        {
            get => this._medianInterArrivalTimeUpAndDownFlow ?? (this._medianInterArrivalTimeUpAndDownFlow = new MedianInterArrivalTimeUpAndDownFlow());
            set => this._medianInterArrivalTimeUpAndDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianInterArrivalTimeUpAndDownFlowML => this.MedianInterArrivalTimeUpAndDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MedianInterArrivalTimeDownFlow MedianInterArrivalTimeDownFlow
        {
            get => this._meadiaInterArrivalTimeDownFlow ?? (this._meadiaInterArrivalTimeDownFlow = new MedianInterArrivalTimeDownFlow());
            set => this._meadiaInterArrivalTimeDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianInterArrivalTimeDownFlowML => this.MedianInterArrivalTimeDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MedianInterArrivalTimeUpFlow MedianInterArrivalTimeUpFlow
        {
            get => this._medianInterArrivalTimeUpFlow ?? (this._medianInterArrivalTimeUpFlow = new MedianInterArrivalTimeUpFlow());
            set => this._medianInterArrivalTimeUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianInterArrivalTimeUpFlowML => this.MedianInterArrivalTimeUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanInterArrivalTimePacketsUpAndDownFlow MeanInterArrivalTimePacketsUpAndDownFlow
        {
            get => this._meanInterArrivalTimePacketsUpAndDownFlow ?? (this._meanInterArrivalTimePacketsUpAndDownFlow = new MeanInterArrivalTimePacketsUpAndDownFlow());
            set => this._meanInterArrivalTimePacketsUpAndDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanInterArrivalTimePacketsUpAndDownFlowML => this.MeanInterArrivalTimePacketsUpAndDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanInterArrivalTimePacketsDownFlow MeanInterArrivalTimePacketsDownFlow
        {
            get => this._meanInterArrivalTimePacketsDownFlow ?? (this._meanInterArrivalTimePacketsDownFlow = new MeanInterArrivalTimePacketsDownFlow());
            set => this._meanInterArrivalTimePacketsDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanInterArrivalTimePacketsDownFlowML => this.MeanInterArrivalTimePacketsDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanInterArrivalTimePacketsUpFlow MeanInterArrivalTimePacketsUpFlow
        {
            get => this._meanInterArrivalTimePacketsUpFlow ?? (this._meanInterArrivalTimePacketsUpFlow = new MeanInterArrivalTimePacketsUpFlow());
            set => this._meanInterArrivalTimePacketsUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanInterArrivalTimePacketsUpFlowML => this.MeanInterArrivalTimePacketsUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxInterArrivalTimePacketsUpAndDownFlow MaxInterArrivalTimePacketsUpAndDownFlow
        {
            get => this._maxInterArrivalTimePacketsUpAndDownFlow ?? (this._maxInterArrivalTimePacketsUpAndDownFlow = new MaxInterArrivalTimePacketsUpAndDownFlow());
            set => this._maxInterArrivalTimePacketsUpAndDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxInterArrivalTimePacketsUpAndDownFlowML => this.MaxInterArrivalTimePacketsUpAndDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxInterArrivalTimePacketsDownFlow MaxInterArrivalTimePacketsDownFlow
        {
            get => this._maxInterArrivalTimePacketsDownFlow ?? (this._maxInterArrivalTimePacketsDownFlow = new MaxInterArrivalTimePacketsDownFlow());
            set => this._maxInterArrivalTimePacketsDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxInterArrivalTimePacketsDownFlowML => this.MaxInterArrivalTimePacketsDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxInterArrivalTimePacketsUpFlow MaxInterArrivalTimePacketsUpFlow
        {
            get => this._maxInterArrivalTimePacketsUpFlow ?? (this._maxInterArrivalTimePacketsUpFlow = new MaxInterArrivalTimePacketsUpFlow());
            set => this._maxInterArrivalTimePacketsUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxInterArrivalTimePacketsUpFlowML => this.MaxInterArrivalTimePacketsUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MinSegmentSizeUp MinSegmentSizeUp
        {
            get => this._minSegmentSizeUp ?? (this._minSegmentSizeUp = new MinSegmentSizeUp());
            set => this._minSegmentSizeUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinSegmentSizeUpML => this.MinSegmentSizeUp;

        [DataMember]
        [FeatureStatistical]
        public MinSegmentSizeDown MinSegmentSizeDown
        {
            get => this._minSegmentSizeDown ?? (this._minSegmentSizeDown = new MinSegmentSizeDown());
            set => this._minSegmentSizeDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinSegmentSizeDownML => this.MinSegmentSizeDown;

        [DataMember]
        [FeatureStatistical]
        public MaxSegmentSizeUp MaxSegmentSizeUp
        {
            get => this._maxSegmentSizeUp ?? (this._maxSegmentSizeUp = new MaxSegmentSizeUp());
            set => this._maxSegmentSizeUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxSegmentSizeUpML => this.MaxSegmentSizeUp;

        [DataMember]
        [FeatureStatistical]
        public MaxSegmentSizeDown MaxSegmentSizeDown
        {
            get => this._maxSegmentSizeDown ?? (this._maxSegmentSizeDown = new MaxSegmentSizeDown());
            set => this._maxSegmentSizeDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxSegmentSizeDownML => this.MaxSegmentSizeDown;

        [DataMember]
        [FeatureStatistical]
        public MinControlBytesUpAndDown MinControlBytesUpAndDown
        {
            get => this._minControlBytesUpAndDown ?? (this._minControlBytesUpAndDown = new MinControlBytesUpAndDown());
            set => this._minControlBytesUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinControlBytesUpAndDownML => this.MinControlBytesUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public MinControlBytesUp MinControlBytesUp
        {
            get => this._minControlBytesUp ?? (this._minControlBytesUp = new MinControlBytesUp());
            set => this._minControlBytesUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinControlBytesUpML => this.MinControlBytesUp;

        [DataMember]
        [FeatureStatistical]
        public MinControlBytesDown MinControlBytesDown
        {
            get => this._minControlDown ?? (this._minControlDown = new MinControlBytesDown());
            set => this._minControlDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinControlBytesDownML => this.MinControlBytesDown;

        [DataMember]
        [FeatureStatistical]
        public MedianControlBytesUpAndDown MedianControlBytesUpAndDown
        {
            get => this._medianControlBytesUpAndDown ?? (this._medianControlBytesUpAndDown = new MedianControlBytesUpAndDown());
            set => this._medianControlBytesUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianControlBytesUpAndDownML => this.MedianControlBytesUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public MedianControlBytesUp MedianControlBytesUp
        {
            get => this._medianControlBytesUp ?? (this._medianControlBytesUp = new MedianControlBytesUp());
            set => this._medianControlBytesUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianControlBytesUpML => this.MedianControlBytesUp;

        [DataMember]
        [FeatureStatistical]
        public MedianControlBytesDown MedianControlBytesDown
        {
            get => this._medianControlDown ?? (this._medianControlDown = new MedianControlBytesDown());
            set => this._medianControlDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MedianControlBytesDownML => this.MedianControlBytesDown;

        [DataMember]
        [FeatureStatistical]
        public MeanControlBytesUpAndDown MeanControlBytesUpAndDown
        {
            get => this._meanControlBytesUpAndDown ?? (this._meanControlBytesUpAndDown = new MeanControlBytesUpAndDown());
            set => this._meanControlBytesUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanControlBytesUpAndDownML => this.MeanControlBytesUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public MeanControlBytesDown MeanControlBytesDown
        {
            get => this._meanControlBytesDown ?? (this._meanControlBytesDown = new MeanControlBytesDown());
            set => this._meanControlBytesDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanControlBytesDownML => this.MeanControlBytesDown;

        [DataMember]
        [FeatureStatistical]
        public MeanControlBytesUp MeanControlBytesUp
        {
            get => this._meanControlBytesUp ?? (this._meanControlBytesUp = new MeanControlBytesUp());
            set => this._meanControlBytesUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanControlBytesUpML => this.MeanControlBytesUp;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileControlBytesDown ThirdQuartileControlBytesDown
        {
            get => this._thirdQuartileControlBytesDown ?? (this._thirdQuartileControlBytesDown = new ThirdQuartileControlBytesDown());
            set => this._thirdQuartileControlBytesDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileControlBytesDownML => this.ThirdQuartileControlBytesDown;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileControlBytesUpAndDown ThirdQuartileControlBytesUpAndDown
        {
            get => this._thirdQuartileControlBytesUpAndDown ?? (this._thirdQuartileControlBytesUpAndDown = new ThirdQuartileControlBytesUpAndDown());
            set => this._thirdQuartileControlBytesUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileControlBytesUpAndDownML => this.ThirdQuartileControlBytesUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileControlBytesUp ThirdQuartileControlBytesUp
        {
            get => this._thirdQuartileControlBytesUp ?? (this._thirdQuartileControlBytesUp = new ThirdQuartileControlBytesUp());
            set => this._thirdQuartileControlBytesUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileControlBytesUpML => this.ThirdQuartileControlBytesUp;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileInterArrivalTimePacketsDown ThirdQuartileInterArrivalTimePacketsDown
        {
            get => this._thirdQuartileInterArrivalTimePacketsDown ?? (this._thirdQuartileInterArrivalTimePacketsDown = new ThirdQuartileInterArrivalTimePacketsDown());
            set => this._thirdQuartileInterArrivalTimePacketsDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileInterArrivalTimePacketsDownML => this.ThirdQuartileInterArrivalTimePacketsDown;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileInterArrivalTimePacketsUpAndDown ThirdQuartileInterArrivalTimePacketsUpAndDown
        {
            get => this._thirdQuartileInterArrivalTimePacketsUpAndDown
                   ?? (this._thirdQuartileInterArrivalTimePacketsUpAndDown = new ThirdQuartileInterArrivalTimePacketsUpAndDown());
            set => this._thirdQuartileInterArrivalTimePacketsUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileInterArrivalTimePacketsUpAndDownML => this.ThirdQuartileInterArrivalTimePacketsUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileInterArrivalTimePacketsUp ThirdQuartileInterArrivalTimePacketsUp
        {
            get => this._thirdQuartileInterArrivalTimePacketsUp ?? (this._thirdQuartileInterArrivalTimePacketsUp = new ThirdQuartileInterArrivalTimePacketsUp());
            set => this._thirdQuartileInterArrivalTimePacketsUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileInterArrivalTimePacketsUpML => this.ThirdQuartileInterArrivalTimePacketsUp;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileInterArrivalTimeDown ThirdQuartileInterArrivalTimeDown
        {
            get => this._thirdQuartileInterArrivalTimeDown ?? (this._thirdQuartileInterArrivalTimeDown = new ThirdQuartileInterArrivalTimeDown());
            set => this._thirdQuartileInterArrivalTimeDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileInterArrivalTimeDownML => this.ThirdQuartileInterArrivalTimeDown;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileInterArrivalTimeUpAndDown ThirdQuartileInterArrivalTimeUpAndDown
        {
            get => this._thirdQuartileInterArrivalTimeUpAndDown ?? (this._thirdQuartileInterArrivalTimeUpAndDown = new ThirdQuartileInterArrivalTimeUpAndDown());
            set => this._thirdQuartileInterArrivalTimeUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileInterArrivalTimeUpAndDownML => this.ThirdQuartileInterArrivalTimeUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public ThirdQuartileInterArrivalTimeUp ThirdQuartileInterArrivalTimeUp
        {
            get => this._thirdQuartileInterArrivalTimeUp ?? (this._thirdQuartileInterArrivalTimeUp = new ThirdQuartileInterArrivalTimeUp());
            set => this._thirdQuartileInterArrivalTimeUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ThirdQuartileInterArrivalTimeUpML => this.ThirdQuartileInterArrivalTimeUp;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileInterArrivalTimePacketsDown FirstQuartileInterArrivalTimePacketsDown
        {
            get => this._firstQuartileInterArrivalTimePacketsDown ?? (this._firstQuartileInterArrivalTimePacketsDown = new FirstQuartileInterArrivalTimePacketsDown());
            set => this._firstQuartileInterArrivalTimePacketsDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileInterArrivalTimePacketsDownML => this.FirstQuartileInterArrivalTimePacketsDown;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileInterArrivalTimePacketsUpAndDown FirstQuartileInterArrivalTimePacketsUpAndDown
        {
            get => this._firstQuartileInterArrivalTimePacketsUpAndDown
                   ?? (this._firstQuartileInterArrivalTimePacketsUpAndDown = new FirstQuartileInterArrivalTimePacketsUpAndDown());
            set => this._firstQuartileInterArrivalTimePacketsUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileInterArrivalTimePacketsUpAndDownML => this.FirstQuartileInterArrivalTimePacketsUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileInterArrivalTimePacketsUp FirstQuartileInterArrivalTimePacketsUp
        {
            get => this._firstQuartileInterArrivalTimePacketsUp ?? (this._firstQuartileInterArrivalTimePacketsUp = new FirstQuartileInterArrivalTimePacketsUp());
            set => this._firstQuartileInterArrivalTimePacketsUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileInterArrivalTimePacketsUpML => this.FirstQuartileInterArrivalTimePacketsUp;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileInterArrivalTimeDown FirstQuartileInterArrivalTimeDown
        {
            get => this._firstQuartileInterArrivalTimeDown ?? (this._firstQuartileInterArrivalTimeDown = new FirstQuartileInterArrivalTimeDown());
            set => this._firstQuartileInterArrivalTimeDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileInterArrivalTimeDownML => this.FirstQuartileInterArrivalTimeDown;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileInterArrivalTimeUpAndDown FirstQuartileInterArrivalTimeUpAndDown
        {
            get => this._firstQuartileInterArrivalTimeUpAndDown ?? (this._firstQuartileInterArrivalTimeUpAndDown = new FirstQuartileInterArrivalTimeUpAndDown());
            set => this._firstQuartileInterArrivalTimeUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileInterArrivalTimeUpAndDownML => this.FirstQuartileInterArrivalTimeUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileInterArrivalTimeUp FirstQuartileInterArrivalTimeUp
        {
            get => this._firstQuartileInterArrivalTimeUp ?? (this._firstQuartileInterArrivalTimeUp = new FirstQuartileInterArrivalTimeUp());
            set => this._firstQuartileInterArrivalTimeUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileInterArrivalTimeUpML => this.FirstQuartileInterArrivalTimeUp;

        [DataMember]
        [FeatureStatistical]
        public MaxControlBytesDown MaxControlBytesDown
        {
            get => this._maxControlBytesDown ?? (this._maxControlBytesDown = new MaxControlBytesDown());
            set => this._maxControlBytesDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxControlBytesDownML => this.MaxControlBytesDown;

        [DataMember]
        [FeatureStatistical]
        public MaxControlBytesUpAndDown MaxControlBytesUpAndDown
        {
            get => this._maxControlBytesUpAndDown ?? (this._maxControlBytesUpAndDown = new MaxControlBytesUpAndDown());
            set => this._maxControlBytesUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxControlBytesUpAndDownML => this.MaxControlBytesUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public MaxControlBytesUp MaxControlBytesUp
        {
            get => this._maxControlBytesUp ?? (this._maxControlBytesUp = new MaxControlBytesUp());
            set => this._maxControlBytesUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxControlBytesUpML => this.MaxControlBytesUp;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileControlBytesDown FirstQuartileControlBytesDown
        {
            get => this._firstQuartileControlBytesDown ?? (this._firstQuartileControlBytesDown = new FirstQuartileControlBytesDown());
            set => this._firstQuartileControlBytesDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileControlBytesDownML => this.FirstQuartileControlBytesDown;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileControlBytesUpAndDown FirstQuartileControlBytesUpAndDown
        {
            get => this._firstQuartileControlBytesUpAndDown ?? (this._firstQuartileControlBytesUpAndDown = new FirstQuartileControlBytesUpAndDown());
            set => this._firstQuartileControlBytesUpAndDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileControlBytesUpAndDownML => this.FirstQuartileControlBytesUpAndDown;

        [DataMember]
        [FeatureStatistical]
        public FirstQuartileControlBytesUp FirstQuartileControlBytesUp
        {
            get => this._firstQuartileControlBytesUp ?? (this._firstQuartileControlBytesUp = new FirstQuartileControlBytesUp());
            set => this._firstQuartileControlBytesUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstQuartileControlBytesUpML => this.FirstQuartileControlBytesUp;

        [DataMember]
        [FeatureStatistical]
        public NumberOfPacketsPerTimeDown NumberOfPacketsPerTimeDown
        {
            get => this._numberOfPacketsPerTimeDown ?? (this._numberOfPacketsPerTimeDown = new NumberOfPacketsPerTimeDown());
            set => this._numberOfPacketsPerTimeDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double NumberOfPacketsPerTimeDownML => this.NumberOfPacketsPerTimeDown;

        [DataMember]
        [FeatureStatistical]
        public NumberOfPacketsPerTimeUp NumberOfPacketsPerTimeUp
        {
            get => this._numberOfPacketsPerTimeUp ?? (this._numberOfPacketsPerTimeUp = new NumberOfPacketsPerTimeUp());
            set => this._numberOfPacketsPerTimeUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double NumberOfPacketsPerTimeUpML => this.NumberOfPacketsPerTimeUp;

        [DataMember]
        [FeatureStatistical]
        public Duration Duration
        {
            get => this._duration ?? (this._duration = new Duration());
            set => this._duration = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double DurationML => this.Duration;

        [DataMember]
        [FeatureStatistical]
        public PUSHPacketsDown PUSHPacketsDown
        {
            get => this._pushPacketsDown ?? (this._pushPacketsDown = new PUSHPacketsDown());
            set => this._pushPacketsDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double PUSHPacketsDownML => this.PUSHPacketsDown;

        [DataMember]
        [FeatureStatistical]
        public FINPacketsDown FINPacketsDown
        {
            get => this._finPacketsDown ?? (this._finPacketsDown = new FINPacketsDown());
            set => this._finPacketsDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FINPacketsDownML => this.FINPacketsDown;

        [DataMember]
        [FeatureStatistical]
        public FINPacketsUp FINPacketsUp
        {
            get => this._finPacketsUp ?? (this._finPacketsUp = new FINPacketsUp());
            set => this._finPacketsUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FINPacketsUpML => this.FINPacketsUp;

        [DataMember]
        [FeatureStatistical]
        public PUSHPacketsUp PUSHPacketsUp
        {
            get => this._pushPacketsUp ?? (this._pushPacketsUp = new PUSHPacketsUp());
            set => this._pushPacketsUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double PUSHPacketsUpML => this.PUSHPacketsUp;

        [DataMember]
        [FeatureStatistical]
        public SYNPacketsDown SYNPacketsDown
        {
            get => this._synPacketsDown ?? (this._synPacketsDown = new SYNPacketsDown());
            set => this._synPacketsDown = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double SYNPacketsDownML => this.SYNPacketsDown;

        [DataMember]
        [FeatureStatistical]
        public SYNPacketsUp SYNPacketsUp
        {
            get => this._synPacketsUp ?? (this._synPacketsUp = new SYNPacketsUp());
            set => this._synPacketsUp = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double SYNPacketsUpML => this.SYNPacketsUp;

        [DataMember]
        [FeatureStatistical]
        public DurationFlow DurationFlow
        {
            get => this._durationFlow ?? (this._durationFlow = new DurationFlow());
            set => this._durationFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double DurationFlowML => this.DurationFlow;

        //[DataMember]
        //[FeatureStatistical]
        //public PortUp PortUp
        //{
        //    get => this._portUp ?? (this._portUp = new PortUp());
        //    set => this._portUp = value;
        //}

        //[FeatureStatisticalMl]
        //[FeatureStatisticalAccord]
        //public double PortUpML => this.PortUp;

        //[DataMember]
        //[FeatureStatistical]
        //public PortDown PortDown
        //{
        //    get => this._portDown ?? (this._portDown = new PortDown());
        //    set => this._portDown = value;
        //}

        //[FeatureStatisticalMl]
        //[FeatureStatisticalAccord]
        //public double PorDownML => this.PortDown;

        [DataMember]
        [FeatureStatistical] //todo accroding to above
        public FirstPayloadSize FirstPayloadSize
        {
            get => this._firstPayloadSize ?? (this._firstPayloadSize = new FirstPayloadSize());
            set => this._firstPayloadSize = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstPayloadSizeML => this.FirstPayloadSize;

        [DataMember]
        [FeatureStatistical]
        public DirectionChanges DirectionChanges
        {
            get => this._directionChanges ?? (this._directionChanges = new DirectionChanges());
            set => this._directionChanges = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double DirectionChangesML => this.DirectionChanges;

        [DataMember]
        [FeatureStatistical]
        public NumberOfPacketsUpFlow NumberOfPacketsUpFlow
        {
            get => this._numberOfPacketsUpFlow ?? (this._numberOfPacketsUpFlow = new NumberOfPacketsUpFlow());
            set => this._numberOfPacketsUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double NumberOfPacketsUpFlowML => this.NumberOfPacketsUpFlow;

        [DataMember]
        [FeatureStatistical]
        public NumberOfPacketsDownFlow NumberOfPacketsDownFlow
        {
            get => this._numberOfPacketsDownFlow ?? (this._numberOfPacketsDownFlow = new NumberOfPacketsDownFlow());
            set => this._numberOfPacketsDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double NumberOfPacketsDownFlowML => this.NumberOfBytesDownFlow;

        [DataMember]
        [FeatureStatistical]
        public NumberOfBytesUpFlow NumberOfBytesUpFlow
        {
            get => this._numberOfBytesUpFlow ?? (this._numberOfBytesUpFlow = new NumberOfBytesUpFlow());
            set => this._numberOfBytesUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double NumberOfBytesUpFlowML => this.NumberOfBytesUpFlow;

        [DataMember]
        [FeatureStatistical]
        public NumberOfBytesDownFlow NumberOfBytesDownFlow
        {
            get => this._numberOfBytesDownFlow ?? (this._numberOfBytesDownFlow = new NumberOfBytesDownFlow());
            set => this._numberOfBytesDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double NumberOfBytesDownFlowML => this.NumberOfBytesDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MinPacketLengthUpFlow MinPacketLengthUpFlow
        {
            get => this._minPacketLengthUpFlow ?? (this._minPacketLengthUpFlow = new MinPacketLengthUpFlow());
            set => this._minPacketLengthUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinPacketLengthUpFlowML => this.MinPacketLengthUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MinPacketLengthDownFlow MinPacketLengthDownFlow
        {
            get => this._minPacketLengthDownFlow ?? (this._minPacketLengthDownFlow = new MinPacketLengthDownFlow());
            set => this._minPacketLengthDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinPacketLengthDownFlowML => this.MinPacketLengthDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxPacketLengthUpFlow MaxPacketLengthUpFlow
        {
            get => this._maxPacketLengthUpFlow ?? (this._maxPacketLengthUpFlow = new MaxPacketLengthUpFlow());
            set => this._maxPacketLengthUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxPacketLengthUpFlowML => this.MaxPacketLengthUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxPacketLengthDownFlow MaxPacketLengthDownFlow
        {
            get => this._maxPacketLengthDownFlow ?? (this._maxPacketLengthDownFlow = new MaxPacketLengthDownFlow());
            set => this._maxPacketLengthDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxPacketLengthDownFlowML => this.MaxPacketLengthDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanPacketLengthUpFlow MeanPacketLengthUpFlow
        {
            get => this._meanPacketLengthUpFlow ?? (this._meanPacketLengthUpFlow = new MeanPacketLengthUpFlow());
            set => this._meanPacketLengthUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanPacketLengthUpFlowML => this.MeanPacketLengthUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanPacketLengthDownFlow MeanPacketLengthDownFlow
        {
            get => this._meanPacketLengthDownFlow ?? (this._meanPacketLengthDownFlow = new MeanPacketLengthDownFlow());
            set => this._meanPacketLengthDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanPacketLengthDownFlowML => this.MeanPacketLengthDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MinInterArrivalTimeUpFlow MinInterArrivalTimeUpFlow
        {
            get => this._minInterArrivalTimeUpFlow ?? (this._minInterArrivalTimeUpFlow = new MinInterArrivalTimeUpFlow());
            set => this._minInterArrivalTimeUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinInterArrivalTimeUpFlowML => this.MinInterArrivalTimeUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MinInterArrivalTimeDownFlow MinInterArrivalTimeDownFlow
        {
            get => this._minInterArrivalTimeDownFlow ?? (this._minInterArrivalTimeDownFlow = new MinInterArrivalTimeDownFlow());
            set => this._minInterArrivalTimeDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MinInterArrivalTimeDownFlowML => this.MinInterArrivalTimeDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxInterArrivalTimeUpFlow MaxInterArrivalTimeUpFlow
        {
            get => this._maxInterArrivalTimeUpFlow ?? (this.MaxInterArrivalTimeUpFlow = new MaxInterArrivalTimeUpFlow());
            set => this._maxInterArrivalTimeUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxInterArrivalTimeUpFlowML => this.MaxInterArrivalTimeUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MaxInterArrivalTimeDownFlow MaxInterArrivalTimeDownFlow
        {
            get => this._maxInterArrivalTimeDownFlow ?? (this.MaxInterArrivalTimeDownFlow = new MaxInterArrivalTimeDownFlow());
            set => this._maxInterArrivalTimeDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MaxInterArrivalTimeDownFlowML => this.MaxInterArrivalTimeDownFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanInterArrivalTimeUpFlow MeanInterArrivalTimeUpFlow
        {
            get => this._meanInterArrivalTimeUpFlow ?? (this._meanInterArrivalTimeUpFlow = new MeanInterArrivalTimeUpFlow());
            set => this._meanInterArrivalTimeUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanInterArrivalTimeUpFlowML => this.MeanInterArrivalTimeUpFlow;

        [DataMember]
        [FeatureStatistical]
        public MeanInterArrivalTimeDownFlow MeanInterArrivalTimeDownFlow
        {
            get => this._meanInterArrivalTimeDownFlow ?? (this._meanInterArrivalTimeDownFlow = new MeanInterArrivalTimeDownFlow());
            set => this._meanInterArrivalTimeDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double MeanInterArrivalTimeDownFlowML => this.MeanInterArrivalTimeDownFlow;

        [DataMember]
        [FeatureStatistical]
        public BytePairsReoccuringUpFlow BytePairsReoccuringUpFlow
        {
            get => this._bytePairsReoccuringUpFlow ?? (this._bytePairsReoccuringUpFlow = new BytePairsReoccuringUpFlow());
            set => this._bytePairsReoccuringUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double BytePairsReoccuringUpFlowML => this.BytePairsReoccuringUpFlow;

        [DataMember]
        [FeatureStatistical]
        public BytePairsReoccuringDownFlow BytePairsReoccuringDownFlow
        {
            get => this._bytePairsReoccuringDownFlow ?? (this._bytePairsReoccuringDownFlow = new BytePairsReoccuringDownFlow());
            set => this._bytePairsReoccuringDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double BytePairsReoccuringDownFlowML => this.BytePairsReoccuringDownFlow;

        [DataMember]
        [FeatureStatistical]
        public First4BytesHashUpFlow First4BytesHashUpFLow
        {
            get => this._first4BytesHashUpFLow ?? (this._first4BytesHashUpFLow = new First4BytesHashUpFlow());
            set => this._first4BytesHashUpFLow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double First4BytesHashUpFLowML => this.First4BytesHashUpFLow;

        [DataMember]
        [FeatureStatistical]
        public First4BytesHashDownFlow First4BytesHashDownFlow
        {
            get => this._first4BytesHashDownFlow ?? (this._first4BytesHashDownFlow = new First4BytesHashDownFlow());
            set => this._first4BytesHashDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double First4BytesHashDownFlowML => this.First4BytesHashDownFlow;

        [DataMember]
        [FeatureStatistical]
        public First3BytesEqualUpFlow First3BytesEqualUpFlow
        {
            get => this._first3BytesEqualUpFlow ?? (this._first3BytesEqualUpFlow = new First3BytesEqualUpFlow());
            set => this._first3BytesEqualUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double First3BytesEqualUpFlowML => this.First3BytesEqualUpFlow;

        [DataMember]
        [FeatureStatistical]
        public First3BytesEqualDownFlow First3BytesEqualDownFlow
        {
            get => this._first3BytesEqualDownFlow ?? (this._first3BytesEqualDownFlow = new First3BytesEqualDownFlow());
            set => this._first3BytesEqualDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double First3BytesEqualDownFlowML => this.First3BytesEqualDownFlow;

        [DataMember]
        [FeatureStatistical]
        public FirstBitPositionUpFlow FirstBitPositionUpFlow
        {
            get => this._firstBitPositionUpFlow ?? (this._firstBitPositionUpFlow = new FirstBitPositionUpFlow());
            set => this._firstBitPositionUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstBitPositionUpFlowML => this.FirstBitPositionUpFlow;

        [DataMember]
        [FeatureStatistical]
        public FirstBitPositionDownFlow FirstBitPositionDownFlow
        {
            get => this._firstBitPositionDownFlow ?? (this._firstBitPositionDownFlow = new FirstBitPositionDownFlow());
            set => this._firstBitPositionDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double FirstBitPositionDownFlowML => this.FirstBitPositionDownFlow;

        [DataMember]
        [FeatureStatistical]
        public ByteFrequencyUpFlow ByteFrequencyUpFlow
        {
            get => this._byteFrequencyUpFlow ?? (this._byteFrequencyUpFlow = new ByteFrequencyUpFlow());
            set => this._byteFrequencyUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ByteFrequencyUpFlowML => this.ByteFrequencyUpFlow;

        [DataMember]
        [FeatureStatistical]
        public ByteFrequencyDownFlow ByteFrequencyDownFlow
        {
            get => this._byteFrequencyDownFlow ?? (this._byteFrequencyDownFlow = new ByteFrequencyDownFlow());
            set => this._byteFrequencyDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double ByteFrequencyDownFlowML => this.ByteFrequencyDownFlow;

        [DataMember]
        [FeatureStatistical]
        public PacketLengthDistributionUpFlow PacketLengthDistributionUpFlow
        {
            get => this._packetLengthDistributionUpFlow ?? (this._packetLengthDistributionUpFlow = new PacketLengthDistributionUpFlow());
            set => this._packetLengthDistributionUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double PacketLengthDistributionUpFlowML => this.PacketLengthDistributionUpFlow;

        [DataMember]
        [FeatureStatistical]
        public PacketLengthDistributionDownFlow PacketLengthDistributionDownFlow
        {
            get => this._packetLengthDistributionDownFlow ?? (this._packetLengthDistributionDownFlow = new PacketLengthDistributionDownFlow());
            set => this._packetLengthDistributionDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double PacketLengthDistributionDownFlowML => this.PacketLengthDistributionDownFlow;

        [DataMember]
        [FeatureStatistical]
        public TransportProtocolType TransportProtocolType
        {
            get => this._transportProtocolType ?? (this._transportProtocolType = new TransportProtocolType());
            set => this._transportProtocolType = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double TransportProtocolTypeML => this.TransportProtocolType;

        //PDU stats

        [Feature]
        [NotMapped]
        public double Precision { get; set; }

        [StringLabel]
        public string Label
        {
            //get { return this._label ?? (this._label = this.ApplicationProtocolNameFull); }//TODO if you want classify by applications
            //get => this._label ?? (this._label = this.ApplicationProtocolName); //TODO  Classify by transport protocol and port
            get => this._label ?? (this._label = LabelSelector.LabelType==LabelSelector.ELabelType.ApplicationProtocolName?this.ApplicationProtocolName:this.ApplicationProtocolNameFull);
            set => this._label = value;
        }
        [NotMapped]
        [DataMember]
        public String ApplicationProtocolName
        {
            get => this._applicationProtocolName ?? (this._applicationProtocolName = this.PredictedModel.ApplicationProtocolName);
            set => this._applicationProtocolName = value;
        }
        // [StringLabel] 
        [NotMapped]
        [DataMember]
        public String ApplicationProtocolNameFull
        {
            get => this._applicationProtocolNameFull ?? (this._applicationProtocolNameFull = this.PredictedModel.ApplicationProtocolName);
            set => this._applicationProtocolNameFull = value;
        }

        public Dictionary<EPIProtocolModel, double> ProtocolModelDifferences => this._protocolModelDifferences
                                                                                        ?? (this._protocolModelDifferences = new Dictionary<EPIProtocolModel, double>());

        public EPIProtocolModel PredictedModel => this._predictedModel ?? (this._predictedModel = this.ProtocolModelDifferencesOrdered.FirstOrDefault().Key);

        [DataMember]
        [FeatureStatistical]
        public EntropyUpFlow EntropyUpFlow
        {
            get => this._entropyUpFlow ?? (this._entropyUpFlow = new EntropyUpFlow());
            set => this._entropyUpFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double EntropyUpFlowML => this.EntropyUpFlow;

        [DataMember]
        [FeatureStatistical]
        public EntropyDownFlow EntropyDownFlow
        {
            get => this._entropyDownFlow ?? (this._entropyDownFlow = new EntropyDownFlow());
            set => this._entropyDownFlow = value;
        }

        [FeatureStatisticalMl]
        [FeatureStatisticalAccord]
        public double EntropyDownFlowML => this.EntropyDownFlow;

        #region Overrides of Object
        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var diff in this.ProtocolModelDifferencesOrdered) { sb.Append($"{diff.Key.ApplicationProtocolName} {diff.Value}, "); }
            return sb.ToString();
        }

        public IOrderedEnumerable<KeyValuePair<EPIProtocolModel, double>> ProtocolModelDifferencesOrdered => this.ProtocolModelDifferences.OrderBy(pair => pair.Value);
        #endregion
    }
}