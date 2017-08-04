// Copyright (c) 2017 Jan Pluskal, Martin Kmet
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
using Netfox.Core.Enums;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.Services;
using Netfox.NBARDatabase;

namespace Netfox.RTP
{
    public class ApplicationRecognizerRTP : ApplicationRecognizerBase
    {
        public IL7ConversationFactory L7ConversationFactory { get; }

        public enum Proto
        {
            Unknown,
            RTP,
            RTCP
        }

        private const String NBARrtpAppTag = "RTP"; //always check if corresponds to the NBAR <name>rtp</name> at RTP protocol taxonomy
        private const String NBARrtcpAppTag = "RTCP";
        public readonly IReadOnlyList<NBAR2TaxonomyProtocol> NbarRtcpTaxonomy;
        public readonly IReadOnlyList<NBAR2TaxonomyProtocol> NbarRtpTaxonomy;

        public ApplicationRecognizerRTP(NBARProtocolPortDatabase NBARProtocolPortDatabase, IL7ConversationFactory l7ConversationFactory):base(NBARProtocolPortDatabase)
        {
            this.L7ConversationFactory = l7ConversationFactory;
            this.NbarRtpTaxonomy = new List<NBAR2TaxonomyProtocol>
            {
                NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(NBARrtpAppTag)
            };
            this.NbarRtcpTaxonomy = new List<NBAR2TaxonomyProtocol>
            {
                NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(NBARrtcpAppTag)
            };
        }

        public override String Name => @"RTP";

        public override String Description => @"Using a DPI to find RTP and RTCP.";

        public override UInt32 Priority => 3;

        public override String Type => "DPI";

        private UInt32 _minPackets { get; } = 50;

        public static Proto RecognizeProto(Byte[] payload)
        {
            // read application data
            if(payload.Count() <= 12) { return Proto.Unknown; }

            // version check
            if((payload[0]&Convert.ToByte("11000000", 2)) != 128) { return Proto.Unknown; }

            // payload check
            var payloadType = payload[1]&Convert.ToByte("01111111", 2);
            // scan for reserved payload types
            switch(payloadType)
            {
                case 1:
                case 2:
                case 19:
                    return Proto.Unknown;
            }

            // possible RTCP
            if(payloadType >= 72 && payloadType <= 76)
            {
                if(IsDecomposableRTCP(payload)) { return Proto.RTCP; }
                return Proto.Unknown;
            }

            // save pdu
            return Proto.RTP;
        }

        public override IEnumerable<L7Conversation> RecognizeAndUpdateConversation(L7Conversation conversation)
        {
            var rtpMap = new Dictionary<UInt32, List<L7PDU>>();
            var rtcpMap = new Dictionary<UInt32, List<L7PDU>>();
            var unknownPdus = new List<L7PDU>();
            var newL7Conversations = new List<L7Conversation>(new []{conversation});

            unknownPdus.Clear();
            rtcpMap.Clear();
            rtpMap.Clear(); //to be sure
            this.RecognizeRtpStreams(conversation.UpFlowPDUs, rtpMap, rtcpMap, unknownPdus);
            foreach(var rtpStream in rtpMap)
            {
                // add only long enought streams
                if(rtpStream.Value.Count() >= this._minPackets)
                {
                    var upFlow = new FsUnidirectionalFlow(conversation.L4Conversation, DaRFlowDirection.up);
                    upFlow.SubstituteL7PDU(rtpStream.Value);
                    var l7Conv = this.L7ConversationFactory.Create(upFlow, DaRFlowDirection.up);
                    l7Conv.ApplicationProtocols = this.NbarRtcpTaxonomy;
                    newL7Conversations.Add(l7Conv);
                }
                else
                {
                    unknownPdus.AddRange(rtpStream.Value);
                }
            }
            // RTCP
            foreach(var rtcpStream in rtcpMap)
            {
                var upFlow = new FsUnidirectionalFlow(conversation.L4Conversation, DaRFlowDirection.up);
                upFlow.SubstituteL7PDU(rtcpStream.Value);
                var l7Conv = this.L7ConversationFactory.Create(upFlow, DaRFlowDirection.down);
                l7Conv.ApplicationProtocols = this.NbarRtcpTaxonomy;
                newL7Conversations.Add(l7Conv);
            }
            var unknownUpFlow = new FsUnidirectionalFlow(conversation.L4Conversation, DaRFlowDirection.down);
            unknownUpFlow.SubstituteL7PDU(unknownPdus);
            //conversationStore.CreateAndAddConversation(upFlow: unknownUpFlow, downFlow: null);
            // TODO: ZAKOMENTOVANE HADZE VYNIMKU

            unknownPdus.Clear();
            rtpMap.Clear();
            rtcpMap.Clear();
            this.RecognizeRtpStreams(conversation.DownFlowPDUs, rtpMap, rtcpMap, unknownPdus);
            foreach(var rtpStream in rtpMap)
            {
                // check long enought streams
                if(rtpStream.Value.Count() >= this._minPackets)
                {
                    var downFlow = new FsUnidirectionalFlow(conversation.L4Conversation, DaRFlowDirection.down);
                    downFlow.SubstituteL7PDU(rtpStream.Value);
                    var l7Conv = this.L7ConversationFactory.Create(downFlow, DaRFlowDirection.down);
                    l7Conv.ApplicationProtocols = this.NbarRtcpTaxonomy;
                    newL7Conversations.Add(l7Conv);
                }
                else
                {
                    unknownPdus.AddRange(rtpStream.Value);
                }
            }
            // RTCP
            foreach(var rtcpStream in rtcpMap)
            {
                var downFlow = new FsUnidirectionalFlow(conversation.L4Conversation, DaRFlowDirection.down);
                downFlow.SubstituteL7PDU(rtcpStream.Value);
                var l7Conv = this.L7ConversationFactory.Create(downFlow, DaRFlowDirection.down);
                l7Conv.ApplicationProtocols = this.NbarRtcpTaxonomy;
                newL7Conversations.Add(l7Conv);
            }
            var unknownDownFlow = new FsUnidirectionalFlow(conversation.L4Conversation, DaRFlowDirection.down);
            unknownDownFlow.SubstituteL7PDU(unknownPdus);
            //conversationStore.CreateAndAddConversation(upFlow: null, downFlow: unknownDownFlow);
            // TODO: ZAKOMENTOVANE HADZE VYNIMKU

            rtpMap.Clear();
            rtcpMap.Clear();
            return newL7Conversations;
        }

        public override IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation)
        {
            var rtpMap = new Dictionary<UInt32, List<L7PDU>>();
            var rtcpMap = new Dictionary<UInt32, List<L7PDU>>();
            var unknownPdus = new List<L7PDU>();
            rtpMap.Clear(); //to be sure
            rtcpMap.Clear();
            this.RecognizeRtpStreams(conversation.UpFlowPDUs, rtpMap, rtcpMap, unknownPdus);
            this.RecognizeRtpStreams(conversation.DownFlowPDUs, rtpMap, rtcpMap, unknownPdus);
            if(rtpMap.Any()) { return this.NbarRtpTaxonomy; }
            if(rtcpMap.Any()) { return this.NbarRtcpTaxonomy; }
            return null;
        }

        private static Boolean IsDecomposableRTCP(Byte[] payload)
        {
            var compPacketLen = payload.Count();
            var processed = 0;
            var prevPaddingFlag = 0;

            // first packet must have type 200 or 201 (Sender Report/ Reciever Report)
            if((payload[1] != 200) && (payload[1] != 201)) { return false; }

            // check all decomposed packets
            while((processed + 4) <= compPacketLen)
            {
                // check version
                if((payload[processed]&Convert.ToByte("11000000", 2)) != 128) { return false; }
                // only last pack can have P set
                if(prevPaddingFlag != 0) { return false; }
                prevPaddingFlag = payload[processed++]&Convert.ToByte("00100000", 2);
                // skip type
                processed++;
                // add length
                var actPacLen = (payload[processed++] << 8) + (payload[processed++]);
                processed += actPacLen*4;
            }

            return true;
        }

        /// <summary>
        ///     Recognizes RTP stream
        ///     One UDP packet encapsulating RTP is represented by one L7PDU
        /// </summary>
        /// <param name="pdus"></param>
        private void RecognizeRtpStreams(IEnumerable<L7PDU> pdus, Dictionary<UInt32, List<L7PDU>> rtpMap, Dictionary<UInt32, List<L7PDU>> rtcpMap, List<L7PDU> unknownPdus)
        {
            foreach(var pdu in pdus)
            {
                var payload = pdu.PDUByteArr;
                var detectedProto = RecognizeProto(payload);

                switch(detectedProto)
                {
                    case Proto.RTP:
                        var rtpSsrc = (Convert.ToUInt32(payload[8]) << 24) + (Convert.ToUInt32(payload[9]) << 16) + (Convert.ToUInt32(payload[10]) << 8)
                                      + (Convert.ToUInt32(payload[11]));
                        // add pdu to RTP stream
                        if(!rtpMap.ContainsKey(rtpSsrc)) { rtpMap.Add(rtpSsrc, new List<L7PDU>()); }
                        rtpMap[rtpSsrc].Add(pdu);
                        break;
                    case Proto.RTCP:
                        var rtcpSsrc = (Convert.ToUInt32(payload[4]) << 24) + (Convert.ToUInt32(payload[5]) << 16) + (Convert.ToUInt32(payload[6]) << 8)
                                       + (Convert.ToUInt32(payload[7]));
                        // add pdu to RTCP stream
                        if(!rtcpMap.ContainsKey(rtcpSsrc)) { rtcpMap.Add(rtcpSsrc, new List<L7PDU>()); }
                        rtcpMap[rtcpSsrc].Add(pdu);
                        break;
                    default:
                        unknownPdus.Add(pdu);
                        break;
                }
            }
        }
    }
}