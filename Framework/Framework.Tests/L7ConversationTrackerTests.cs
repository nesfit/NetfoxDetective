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

using Netfox.NetfoxFrameworkAPI.Tests;
using NUnit.Framework;

namespace Netfox.Framework.Tests
{
    /// <summary> A defragment and reassemble tests.</summary>
    [TestFixture][Ignore("Old tests")]
    public class L4ProcessorTests : UnitTestBaseSetupFixture
    {
        /// <summary> Tests packet generator.</summary>
        [Test]
        public static void PacketGeneratorTest()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.pcap_mix_50_cap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs();
            //var strangePdu = pdus.Where(pdu => pdu.FrameListNumbers.Contains((uint)2135));
        }

        /// <summary> Tests pv 4evidence packet analysis.</summary>
        [Test]
        public static void Pv4EvidencePacketAnalysisTest()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.pcap_mix_evidence_packet_analysis_pcap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs();
            //var strangePdu = pdus.Where(pdu => pdu.FrameListNumbers.Contains((uint)2135));
        }

        /// <summary> Pv 4 TCP fast retranssmit.</summary>
        [Test]
        public static void Pv4TCPFastRetranssmit()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.small_pcaps_tcp_fastretranssmit_pcap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs(conversations[0]);
            //var payloads = pdus.Select(daRl7Dpu => daRl7Dpu.GetPDUByteArr()).ToList();
            //var paylen = 0;
            //payloads.ForEach(i => paylen += i.Length);
            //var virtFrames = manager.Captures.First().CaptureProcessor.GetVirtualFramesCount();
            //Assert.IsTrue(paylen == 261548 && virtFrames == 0);
        }

        /// <summary> Pv 4 TCP multiple retranssmits.</summary>
        [Test]
        public static void Pv4TCPMultipleRetranssmits()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.small_pcaps_tcp_fragment_overlaps_old_data_pcap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs(conversations[0]);
            //var payloads = pdus.Select(daRl7Dpu => daRl7Dpu.GetPDUByteArr()).ToList();
            //var paylen = 0;
            //payloads.ForEach(i => paylen += i.Length);
            //var virtFrames = manager.Captures.First().CaptureProcessor.GetVirtualFramesCount();
            //Assert.IsTrue(paylen == 200108 && virtFrames == 0);
        }

        /// <summary> Pv 4 TCP reassembling.</summary>
        [Test]
        public static void Pv4TCPReassembling()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.small_pcaps_tcp_reassembling_cap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs(conversations[0]);
            //var payloads = pdus.Select(daRl7Dpu => daRl7Dpu.GetPDUByteArr()).ToList();
            //var paylen = 0;
            //payloads.ForEach(i => paylen += i.Length);
            //var virtFrames = manager.Captures.First().CaptureProcessor.GetVirtualFramesCount();
            //Assert.IsTrue(paylen == 13402 && virtFrames == 0);
        }

        /// <summary> Pv 4 TCP reassembling.</summary>
        [Test]
        public static void Pv4TCPReassemblingAdvanced()
        {
            //var pc = new CaptureProcessor(Pcaps.Default.pcap_mix_icq1_cap);
            //pc.TrackConversations();
            //var l4flow = pc.GetL4Flows();
            //var convs = pc.GetConversations();

            //var emptyFlows = l4flow.Where(f => !f.Frames.Any());

            //var groupedL7 = from c in convs
            //                group c by new
            //                {
            //                    c.TargetEndPoint,
            //                    c.SourceEndPoint
            //                }
            //    into g
            //                select g;

            //var moreInConv = groupedL7.Where(g => g.Count() > 1);

            //var convsCount = convs.Count();
            //var l4flowCount = l4flow.Count();


            //Assert.IsTrue(convsCount == 187); // 1 missing is for non tcp/udp traffic
            //Assert.IsTrue(l4flowCount == 188);
        }

        /// <summary> Pv 4 TCP reassembling more missing.</summary>
        [Test]
        public static void Pv4TCPReassemblingMoreMissing()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.small_pcaps_tcp_reassembling_more_missing_cap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs(conversations[0]);
            //var payloads = pdus.Select(daRl7Dpu => daRl7Dpu.GetPDUByteArr()).ToList();
            //var paylen = 0;
            //payloads.ForEach(i => paylen += i.Length);
            //var virtFrames = manager.Captures.First().CaptureProcessor.GetVirtualFramesCount();
            //Assert.IsTrue(paylen == 13402 && virtFrames == 3);
        }

        /// <summary> Pv 4 TCP reassembling one missing.</summary>
        [Test]
        public static void Pv4TCPReassemblingOneMissing()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.small_pcaps_tcp_reassembling_one_missing_cap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs(conversations[0]);
            //var payloads = pdus.Select(daRl7Dpu => daRl7Dpu.GetPDUByteArr()).ToList();
            //var paylen = 0;
            //payloads.ForEach(i => paylen += i.Length);
            //var virtFrames = manager.Captures.First().CaptureProcessor.GetVirtualFramesCount();
            //Assert.IsTrue(paylen == 13402 && virtFrames == 1);
        }

        /// <summary> Pv 4 UDP fragmentation.</summary>
        [Test]
        public static void Pv4UdpFragmentation()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.small_pcaps_ip_frag_3_cap);
            //var conversations = manager.GetConversations().ToArray();
            //var pdus = manager.GetL7PDUs(conversations.First(c => c.L7PDUs.Count() == 3));
            //var payloads = new List<byte[]>();
            //foreach (var daRl7Dpu in pdus)
            //{
            //    Assert.IsTrue(daRl7Dpu.ExtractedBytes == 8312);
            //    payloads.Add(daRl7Dpu.GetPDUByteArr());
            //}
            //Assert.IsTrue(payloads.Count == 3 && payloads.First().Length == 8312);
        }

        /// <summary> Pv 4 UDP reassembling.</summary>
        [Test]
        public static void Pv4UDPReassembling()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.pcap_mix_root_cz_cap);
            //var pdus = manager.GetL7PDUs();
            //var udpPdus = pdus.Where(p => p.FrameList.First().IpProtocol == IPProtocolType.UDP).ToList();
            //Assert.IsTrue(udpPdus.Count == 232);
        }

        /// <summary> Pv 4 UDP reassembling convert.</summary>
        [Test]
        public static void Pv4UDPReassemblingConv()
        {
            //var manager = new CaptureManager();
            //manager.AddCapture(Pcaps.Default.pcap_mix_root_cz_cap);
            //var conversations = manager.GetConversations().ToArray();
            //var convcount = conversations.Take(20).First(c => c.L7PDUs.Count() == 39).L7PDUs.Count();
            //Assert.IsTrue(convcount == 39);
        }
    }
}