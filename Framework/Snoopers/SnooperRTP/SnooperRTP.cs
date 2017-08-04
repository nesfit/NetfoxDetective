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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Castle.Windsor;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.Snoopers;
using Netfox.NBARDatabase;
using Netfox.RTP;
using Netfox.SnooperRTP.Models;

namespace Netfox.SnooperRTP
{
    public class SnooperRTP : SnooperBase
    {
        private static IReadOnlyDictionary<uint, string> _staticPtMap;
        private static IReadOnlyDictionary<uint, string> _codecMap;
        private static IReadOnlyDictionary<uint, int> _codecPtMap;
        private static IReadOnlyDictionary<uint, int> _codecTdMap;
        private static IReadOnlyDictionary<uint, int> _codecPlMap;
        private readonly List<SnooperExportedObjectBaseRtpRtcp> _exportedObjects = new List<SnooperExportedObjectBaseRtpRtcp>();

        private readonly ConcurrentDictionary<Tuple<UInt32, IPAddress, IPAddress>, SnooperExportedObjectBaseRtpRtcp> _ssrcMap =
            new ConcurrentDictionary<Tuple<uint, IPAddress, IPAddress>, SnooperExportedObjectBaseRtpRtcp>();

        private NBAR2TaxonomyProtocol _rtcpTaxonomyProtocol;
        private NBAR2TaxonomyProtocol _rtpTaxonomyProtocol;
        public SnooperRTP() { }

        public SnooperRTP(
            WindsorContainer investigationWindsorContainer,
            SelectedConversations conversations,
            DirectoryInfo exportDirectory,
            
            bool ignoreApplicationTags) : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) {}

        public override string Name => "RTP";

        public override string[] ProtocolNBARName => new[]
        {
            "RTP",
            "RTCP"
        };

        public override string Description => "Snooper for RTP/RTCP";
        public override int[] KnownApplicationPorts => null;
        public override SnooperExportBase PrototypExportObject => new SnooperExportRTP();

        public static IReadOnlyDictionary<uint, string> StaticPtMap => _staticPtMap ?? (_staticPtMap = new Dictionary<uint, string>
        {
            {0, "PCMU"},
            {3, "GSM"},
            {4, "G723"},
            {5, "DVI4"},
            {6, "DVI4"},
            {7, "LPC"},
            {8, "PCMA"},
            {9, "G722"},
            {10, "L16"},
            {11, "L16"},
            {12, "QCELP"},
            {13, "CN"},
            {14, "MPA"},
            {15, "G728"},
            {16, "DVI4"},
            {17, "DVI4"},
            {18, "G729"},
            {25, "CelB"},
            {26, "JPEG"},
            {28, "nv"},
            {31, "H261"},
            {32, "MPV"},
            {33, "MP2T"},
            {34, "H263"}
        });

        public static IReadOnlyDictionary<uint, string> CodecMap => _codecMap ?? (_codecMap = new Dictionary<uint, string>
        {
            {0, "AMR-WB"},
            {1, "AMR-12k"},
            {2, "GSM"},
            {3, "G.722"},
            {4, "G.722.1"},
            {5, "G.723.1-5k"},
            {6, "G.723.1-6k"},
            {7, "G.726-16"},
            {8, "G.726-24"},
            {9, "G.726-32"},
            {10, "G.726-40"},
            {11, "G.729"},
            {12, "G.729a"},
            {13, "G.729b"},
            {14, "G.711 a-law"},
            {15, "G.711 u-law"},
            {16, "Speex16"},
            {17, "Speex8"}
        });

        public static IReadOnlyDictionary<uint, int> CodecPtMap => _codecPtMap ?? (_codecPtMap = new Dictionary<uint, int>
        {
            {0, -1}, // -1 = dynamic
            {1, -1},
            {2, 3},
            {3, 9},
            {4, -1},
            {5, 4},
            {6, 4},
            {7, -1},
            {8, -1},
            {9, -1},
            {10, -1},
            {11, 18},
            {12, 18},
            {13, 18},
            {14, 8},
            {15, 0},
            {16, -1},
            {17, -1}
        });

        public static IReadOnlyDictionary<uint, int> CodecTdMap => _codecTdMap ?? (_codecTdMap = new Dictionary<uint, int>
        {
            {0, 160}, // -1 = var
            {1, 160},
            {2, 160},
            {3, 1},
            {4, 16},
            {5, 12},
            {6, 10},
            {7, 4},
            {8, 8},
            {9, 2},
            {10, 8},
            {11, 8},
            {12, 8},
            {13, 0},
            {14, 1},
            {15, 1},
            {16, 80},
            {17, 8}
        });

        public static IReadOnlyDictionary<uint, int> CodecPlMap => _codecPlMap ?? (_codecPlMap = new Dictionary<uint, int>
        {
            {0, 31}, // 0 = var
            {1, 33},
            {2, 33},
            {3, 1},
            {4, 3},
            {5, 1},
            {6, 1},
            {7, 1},
            {8, 3},
            {9, 1},
            {10, 5},
            {11, 1},
            {12, 1},
            {13, 0},
            {14, 1},
            {15, 1},
            {16, 130},
            {17, 1}
        });

        protected new SnooperExportRTP SnooperExport => base.SnooperExport as SnooperExportRTP;
        private uint _minPackets { get; } = 50;

        private NBAR2TaxonomyProtocol RTPTaxonomyProtocol
        {
            get { return this._rtpTaxonomyProtocol ?? (this._rtpTaxonomyProtocol = this.InvestigationWindsorContainer.Resolve<IApplicationRecognizer>().GetNbar2TaxonomyProtocol("RTP")); }
            set { this._rtpTaxonomyProtocol = value; }
        }

        private NBAR2TaxonomyProtocol RtcpTaxonomyProtocol
        {
            get { return this._rtcpTaxonomyProtocol ?? (this._rtcpTaxonomyProtocol = this.InvestigationWindsorContainer.Resolve<IApplicationRecognizer>().GetNbar2TaxonomyProtocol("RTCP")); }
            set { this._rtcpTaxonomyProtocol = value; }
        }

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportRTP();

        /// <summary>
        ///     Process current conversation
        /// </summary>
        protected override void ProcessConversation()
        {
            if(this.CurrentConversation.L7PDUs.Any()) { this.SnooperExport.TimeStampFirst = this.CurrentConversation.FirstSeen; }
            Debug.WriteLine("SnooperRTP.ProcessConversation() called");
            if(this.CurrentConversation.ApplicationProtocols.Contains(this.RTPTaxonomyProtocol))
            {
                if(this.CurrentConversation.L7PDUs.Count() < this._minPackets) { return; }

                if(this.CurrentConversation.L7PDUs.Any(pdu => ApplicationRecognizerRTP.RecognizeProto(pdu.PDUByteArr) != ApplicationRecognizerRTP.Proto.RTP)) { return; }
                //this is RTP
                this.ProcessRTP();
            }
            else if(this.CurrentConversation.ApplicationProtocols.Contains(this.RtcpTaxonomyProtocol))
            {
                if(this.CurrentConversation.L7PDUs.Any(pdu => ApplicationRecognizerRTP.RecognizeProto(pdu.PDUByteArr) != ApplicationRecognizerRTP.Proto.RTCP)) { return; }
                //this is RTCP
                this.ProcessRTCP();
            }
        }

        /// <summary>
        ///     Main function of SnooperRTP
        /// </summary>
        protected override void RunBody()
        {
            this.SelectedConversations.LockSelectedConversations();

            long conversationIndex;
            ILxConversation currentConversation;

            while(this.SelectedConversations.TryGetNextConversations(this.GetType(), out currentConversation, out conversationIndex))
            {
                var selectedL7Conversations = new List<L7Conversation>();

                if (currentConversation is L7Conversation) //todo refactor to SnooperBase.. or make more readable.. to method or somenting...
                { selectedL7Conversations.Add(currentConversation as L7Conversation); }
                else if (currentConversation is L4Conversation) { selectedL7Conversations.AddRange((currentConversation as L4Conversation).L7Conversations); }
                else if (currentConversation is L3Conversation) { selectedL7Conversations.AddRange((currentConversation as L3Conversation).L7Conversations); }

                foreach (var selectedL7Conversation in selectedL7Conversations)
                {
                    this.CurrentConversation = selectedL7Conversation;

                    if(!this.ForceExportOnAllConversations && !this.CurrentConversation.IsXyProtocolConversation(this.ProtocolNBARName)) { continue; }

                    this.OnConversationProcessingBegin();

                    this.ProcessConversation();

                    this.OnBeforeDataExporting();

                    foreach(var result in this._ssrcMap)
                    {
                        var rtpRtcpMsg = result.Value;
                        var rtpConv = rtpRtcpMsg.RTPConversation;

                        if(rtpConv == null)
                        {
                            this.SnooperExport.DiscardExportObject(rtpRtcpMsg);
                            continue;
                        }

                        if(!this._exportedObjects.Contains(rtpRtcpMsg))
                        {
                            Console.WriteLine(rtpRtcpMsg.PayloadFileName);

                            this.SnooperExport.AddExportObject(rtpRtcpMsg);
                            this._exportedObjects.Add(rtpRtcpMsg);
                        }
                    }

                    this.OnAfterDataExporting();

                    this.OnConversationProcessingEnd();
                }
            }
        }

        /// <summary>
        ///     Computes greatest common divisor for both numbers
        /// </summary>
        /// <param name="a">number</param>
        /// <param name="b">number</param>
        /// <returns>greathest common divisor</returns>
        private uint Gcd(uint a, uint b)
        {
            return b != 0? this.Gcd(b, a%b) : a;
        }

        /// <summary>
        ///     Gets RTP header length from raw data
        /// </summary>
        /// <param name="payload">raw data</param>
        /// <returns></returns>
        private int GetRTPHeaderLen(byte[] payload)
        {
            // header length check
            // fixed header len
            var headerLength = 12;
            // CSRC IDs
            headerLength += (payload[0]&Convert.ToByte("00001111", 2))*4;
            // ext header len
            if((payload[0]&Convert.ToByte("00010000", 2)) != 0)
            {
                var extWords = (payload[headerLength + 2] << 8) + (payload[headerLength + 3]);
                headerLength += extWords*4 + 4; // length of extension in bytes + initial 4 octets
            }
            return headerLength;
        }

        private UInt16 GetRTPSeqNum(byte[] payload) { return Convert.ToUInt16((payload[2] << 8) + payload[3]); }

        private SnooperExportedObjectBaseRtpRtcp ParseHeader(byte[] payload)
        {
            // save values to msg
            var msg = new SnooperExportedObjectBaseRtpRtcp(this.SnooperExport);
            msg.PtNum = (byte) (payload[1]&Convert.ToByte("01111111", 2));
            msg.SSRC = (Convert.ToUInt32(payload[8]) << 24) + (Convert.ToUInt32(payload[9]) << 16) + (Convert.ToUInt32(payload[10]) << 8) + (Convert.ToUInt32(payload[11]));
            return msg;
        }

        private void ProcessRTCP() { } // todo

        private void ProcessRTP()
        {
            Console.WriteLine("SnooperRTP.ProcessRTP() called");
            this.ProcessRTPFlow(this.CurrentConversation.UpFlowPDUs);
            this.ProcessRTPFlow(this.CurrentConversation.DownFlowPDUs);
        }

        private void ProcessRTPFlow(IEnumerable<L7PDU> flowPDUs)
        {
            Console.WriteLine("SnooperRTP.ProcessRTPFlow() called");
            var pdus = flowPDUs as L7PDU[] ?? flowPDUs.ToArray();
            if(!pdus.Any()) { return; }

            Console.WriteLine("1");

            // initialize RTP message
            var rtpRtcpMsg = this.ParseHeader(pdus.First().PDUByteArr);
            rtpRtcpMsg.Start = pdus.First().FirstSeen;
            rtpRtcpMsg.End = pdus.Last().LastSeen;
            rtpRtcpMsg.PacketsCnt = 0;
            rtpRtcpMsg.LostPacketsCnt = 0;
            rtpRtcpMsg.BytesCnt = 0;
            rtpRtcpMsg.RTPConversation = this.CurrentConversation;
            Console.WriteLine("2");
            // determine payload type
            if((rtpRtcpMsg.PtNum >= 96) && (rtpRtcpMsg.PtNum <= 127)) {
                rtpRtcpMsg.PayloadType = "Dynamic (" + rtpRtcpMsg.PtNum + ")";
            }
            else if(StaticPtMap.ContainsKey(rtpRtcpMsg.PtNum)) {
                rtpRtcpMsg.PayloadType = StaticPtMap[rtpRtcpMsg.PtNum] + " (" + rtpRtcpMsg.PtNum + ")";
            }
            else
            {
                rtpRtcpMsg.PayloadType = "Unassigned (" + rtpRtcpMsg.PtNum + ")";
            }
            Console.WriteLine("3");

            // Sorting along the sequence number
            UInt32 actIncrement = 0;
            UInt32 lastSeqNumber = 0;
            var sortedPDUsDict = new SortedDictionary<UInt32, L7PDU>();
            foreach(var pdu in pdus)
            {
                // decode sequence number
                var payload = pdu.PDUByteArr;
                var actSeqNumber = this.GetRTPSeqNum(payload);

                // act seqnumber increment TODO: pokus s vyhladavanim klucov
                if((lastSeqNumber >= (UInt16.MaxValue - 100)) && (actSeqNumber <= 100)) { actIncrement += UInt16.MaxValue + 1; }
                if(!sortedPDUsDict.ContainsKey(actSeqNumber + actIncrement)) { sortedPDUsDict.Add(actSeqNumber + actIncrement, pdu); }

                // add pdu to sorted list


                lastSeqNumber = actSeqNumber;
                rtpRtcpMsg.ExportSources.Add(pdu);
            }
            Console.WriteLine("4");

            // save payload and count
            UInt32 lastTimestamp = 0;
            UInt32 deltaTime = 0;
            UInt32 payloadLen = 0;
            var firstSeqNum = this.GetRTPSeqNum(sortedPDUsDict.First().Value.PDUByteArr);
            var prevSeqNum = firstSeqNum == 0? UInt16.MaxValue : Convert.ToUInt16(firstSeqNum - 1);
            Console.WriteLine("5");

            if(!Directory.Exists(this.ExportBaseDirectory.FullName)) { Directory.CreateDirectory(this.ExportBaseDirectory.FullName); }
            rtpRtcpMsg.PayloadFilePath = Path.Combine(this.ExportBaseDirectory.FullName, rtpRtcpMsg.PayloadFileName);
            using(var fileStream = File.Create(rtpRtcpMsg.PayloadFilePath))
            {
                foreach(var pdu in sortedPDUsDict.Values)
                {
                    // get variables
                    UInt32 lostWindowSize = 0;
                    var payload = pdu.PDUByteArr;
                    var headerLen = this.GetRTPHeaderLen(payload);
                    var seqNum = this.GetRTPSeqNum(payload);
                    var actPayloadLen = Convert.ToUInt32(payload.Count() - headerLen);
                    var actTimestamp = (Convert.ToUInt32(payload[4]) << 24) + (Convert.ToUInt32(payload[5]) << 16) + (Convert.ToUInt32(payload[6]) << 8)
                                       + (Convert.ToUInt32(payload[7]));
                    // act counters
                    rtpRtcpMsg.PacketsCnt++;
                    rtpRtcpMsg.BytesCnt += Convert.ToUInt32(actPayloadLen);
                    if(seqNum != (prevSeqNum + 1)%(UInt16.MaxValue + 1))
                    {
                        // overflowed seqence number handle and actualisation (packet diff - 1, when overflowed occurs => + (UInt16.MaxValue + 1)-1)
                        lostWindowSize = seqNum > prevSeqNum? Convert.ToUInt32(seqNum - prevSeqNum - 1) : Convert.ToUInt32(seqNum - prevSeqNum + UInt16.MaxValue);

                        rtpRtcpMsg.LostPacketsCnt += lostWindowSize;
                    }


                    prevSeqNum = seqNum;
                    // act delta time values
                    if(rtpRtcpMsg.PacketsCnt > 2)
                    {
                        var actDelta = actTimestamp - lastTimestamp;
                        if(lostWindowSize > 0) { actDelta /= lostWindowSize + 1; }
                        if(actDelta != deltaTime) { deltaTime = 0; }
                    }
                    else if(rtpRtcpMsg.PacketsCnt == 2) { deltaTime = actTimestamp - lastTimestamp; }
                    lastTimestamp = actTimestamp;
                    // act payload length
                    if(rtpRtcpMsg.PacketsCnt > 1) {
                        if(payloadLen != actPayloadLen) { payloadLen = 0; }
                    }
                    else
                    {
                        payloadLen = actPayloadLen;
                    }
                    // save payload to file
                    fileStream.Write(payload, headerLen, Convert.ToInt32(actPayloadLen));
                }
            }
            Console.WriteLine("6");
            // try detect codec
            rtpRtcpMsg.PayloadLen = payloadLen > 0? payloadLen.ToString() : "Var.";
            rtpRtcpMsg.TimeDelta = deltaTime > 0? deltaTime.ToString() : "Var.";
            // init codecs
            int ptParam;
            uint tdParam;
            uint plParam;
            if((rtpRtcpMsg.PtNum >= 96) && (rtpRtcpMsg.PtNum <= 127)) {
                ptParam = -1;
            }
            else
            {
                ptParam = rtpRtcpMsg.PtNum;
            }
            var commonDivisor = this.Gcd(payloadLen, deltaTime);
            if(commonDivisor != 0)
            {
                tdParam = deltaTime/commonDivisor;
                plParam = payloadLen/commonDivisor;
            }
            else
            {
                tdParam = deltaTime;
                plParam = payloadLen;
            }
            var possibleCodecs = new List<string>();
            // filter possible codecs
            for (uint i = 0; i < CodecMap.Count(); i++)
            {
                var possibleCodec = true; // reset

                // rule out codecs
                if(CodecPtMap[i] != ptParam) { possibleCodec = false; }
                if(CodecTdMap[i] != tdParam) { possibleCodec = false; }
                if(CodecPlMap[i] != plParam) { possibleCodec = false; }

                if(possibleCodec)
                {
                    possibleCodecs.Add(CodecMap[i]);
                }
            }
            rtpRtcpMsg.PossibleCodecs = possibleCodecs;
            Console.WriteLine("7");
            // act info in map
            var srcAddress = pdus.First().SourceEndPoint.Address;
            var dstAddress = pdus.First().DestinationEndPoint.Address;
            var rtpID = new Tuple<UInt32, IPAddress, IPAddress>(rtpRtcpMsg.SSRC, srcAddress, dstAddress);
            Console.WriteLine("7.1");

            var whatWentWrong = string.Empty;
            var wavFilePath = RTPExportedPayload.TryConverting(rtpRtcpMsg.PtNum, Path.Combine(this.ExportBaseDirectory.FullName, rtpRtcpMsg.PayloadFileName), out whatWentWrong);
            if (wavFilePath == null)
            {
                // almost like exception
            }
            else
            { rtpRtcpMsg.WavFilePath = wavFilePath; }

            var discardObject = this._ssrcMap.ContainsKey(rtpID);

            this._ssrcMap.AddOrUpdate(rtpID, rtpRtcpMsg, (u, msg) =>
            {
                Console.WriteLine("7.2");
                msg.UpdateValues(rtpRtcpMsg);
                Console.WriteLine("7.3");
                return msg;
            }); //TODO this message is most probably RTCP or RTCP was evaluated before this

            if(discardObject) { this.SnooperExport.DiscardExportObject(rtpRtcpMsg); }
        }
    }
}