// Copyright (c) 2017 Jan Pluskal, Matus Dobrotka
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
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperDNS.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperDNS.Tests
{
    internal class SnooperDNSTests : SnooperBaseTests
    {
        public SnooperDNSTests()
        {
            this.SnoopersToUse = new List<ISnooper> {new SnooperDNS()};
        }

        private void CheckQueryAndAnswer(ushort messageID, IReadOnlyList<ushort> flags, IReadOnlyList<string> messages,
            DNSParseMsg.DNSResponseType responseType, string filePath)
        {
            this.LoadPcapAndExportData(filePath);

            SnooperExportDNS exportedObjectsReference = null;
            exportedObjectsReference = SnooperExports.OfType<SnooperExportDNS>().FirstOrDefault(i =>
                ((SnooperExportedDataObjectDNS) i.ExportObjects[0]).MessageId == messageID);

            Assert.IsNotNull(exportedObjectsReference);
            Assert.AreEqual(2, exportedObjectsReference.ExportObjects.Count);

            int i = 1, j = 0;
            foreach (var snooperExportedObjectBase in exportedObjectsReference.ExportObjects.ToArray())
            {
                var message = (SnooperExportedDataObjectDNS) snooperExportedObjectBase;
                Assert.AreEqual(message.MessageId, messageID);
                Assert.AreEqual(message.Flags, flags[j++]);
                Assert.AreEqual(message.Queries.SerializedValue, messages[0]);
                switch (responseType)
                {
                    case DNSParseMsg.DNSResponseType.ANSWER:
                        Assert.AreEqual(messages[i++], message.Answer.SerializedValue);
                        break;
                    case DNSParseMsg.DNSResponseType.ADDITIONAL:
                        Assert.AreEqual(messages[i++], message.Additional.SerializedValue);
                        break;
                }
            }
        }

        private void LoadPcapAndExportData(String filePath)
        {
            Console.WriteLine($"{DateTime.Now} -- Pcap parsing start");
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(filePath));
            Console.WriteLine($"{DateTime.Now} -- Pcap parsing finished");

            var conversations = this.L7Conversations.Where(conversation =>
                conversation.SourceEndPoint.Port == 53 || conversation.DestinationEndPoint.Port == 53).ToArray();

            Console.WriteLine($"{DateTime.Now} -- Export start");
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);
            Console.WriteLine($"{DateTime.Now} -- Export finished");

            var dnsQueries = this.SnooperExports.Where(i => i != null).Sum(exportBase => exportBase.ExportObjects.Sum(exportObjectBase =>
            {
                if (!(exportObjectBase is SnooperExportedDataObjectDNS dnsExport) || dnsExport.Answer.Any()) return 0;

                return dnsExport.Queries.Count;
            }));


            var dnsAnswer = this.SnooperExports.Sum(exportBase => exportBase.ExportObjects.Sum(exportObjectBase =>
            {
                if (!(exportObjectBase is SnooperExportedDataObjectDNS dnsExport)) return 0;

                return dnsExport.Answer.Count;
            }));


            var dnsAdditional = this.SnooperExports.Sum(exportBase => exportBase.ExportObjects.Sum(exportObjectBase =>
            {
                if (!(exportObjectBase is SnooperExportedDataObjectDNS dnsExport)) return 0;

                return dnsExport.Additional.Count;
            }));


            var dnsAuthority = this.SnooperExports.Sum(exportBase => exportBase.ExportObjects.Sum(exportObjectBase =>
            {
                if (!(exportObjectBase is SnooperExportedDataObjectDNS dnsExport)) return 0;

                return dnsExport.Authority.Count;
            }));

            Console.WriteLine($"DNS queries: {dnsQueries}");
            Console.WriteLine($"DNS answers: {dnsAnswer}");
            Console.WriteLine($"DNS additional: {dnsAdditional}");
            Console.WriteLine($"DNS authority: {dnsAuthority}");
        }

        [Test, Ignore("OnDemand")]
        public void M57Case()
        {
            this.LoadPcapAndExportData(@"F:\pcaps\m57\m57.pcap ");
        }

        [Test]
        public void DNSTest_responseA_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xcbec;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"stun.softjoys.com\",\"QType\":1,\"QClass\":1}]",
                "[]",
                "[{\"IPAddress\":\"69.4.236.239\",\"TTL\":3600,\"Name\":\"stun.softjoys.com\",\"QType\":1,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_01_pcap));
        }

        [Test]
        public void DNSTest_responseNAPTR_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xe5ce;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"cesnet.cz\",\"QType\":35,\"QClass\":1}]",
                "[]",
                "[{\"Order\":100,\"Preference\":50,\"Flags\":[\"s\"],\"Services\":\"SIP+D2U\",\"Regexp\":\"\",\"Replacement\":\"_sip._udp.cesnet.cz\",\"TTL\":86400,\"Name\":\"cesnet.cz\",\"QType\":35,\"QClass\":1},{\"Order\":200,\"Preference\":50,\"Flags\":[\"s\"],\"Services\":\"SIP+D2T\",\"Regexp\":\"\",\"Replacement\":\"_sip._tcp.cesnet.cz\",\"TTL\":86400,\"Name\":\"cesnet.cz\",\"QType\":35,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_01_pcap)
            );
        }

        [Test]
        public void DNSTest_responseSVR_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0x4d38;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"_sip._udp.cesnet.cz\",\"QType\":33,\"QClass\":1}]",
                "[]",
                "[{\"Service\":\"_sip\",\"Protocol\":\"_udp\",\"Priority\":100,\"Weight\":10,\"Port\":5060,\"Target\":\"cyrus.cesnet.cz\",\"TTL\":86400,\"Name\":\"cesnet.cz\",\"QType\":33,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_01_pcap)
            );
        }

        [Test]
        public void DNSTest_responseAAAA_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0x7184;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"www.fit.vutbr.cz\",\"QType\":28,\"QClass\":1}]",
                "[]",
                "[{\"IPAddress\":\"2001:67c:1220:809::93e5:917\",\"TTL\":9914,\"Name\":\"www.fit.vutbr.cz\",\"QType\":28,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responseMX_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0x8b5b;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"fit.vutbr.cz\",\"QType\":15,\"QClass\":1}]",
                "[]",
                "[{\"Preference\":10,\"Exchange\":\"kazi.fit.vutbr.cz\",\"TTL\":11029,\"Name\":\"fit.vutbr.cz\",\"QType\":15,\"QClass\":1},{\"Preference\":20,\"Exchange\":\"eva.fit.vutbr.cz\",\"TTL\":11029,\"Name\":\"fit.vutbr.cz\",\"QType\":15,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responseNS_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0x8e4d;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"fit.vutbr.cz\",\"QType\":2,\"QClass\":1}]",
                "[]",
                "[{\"RDataName\":\"kazi.fit.vutbr.cz\",\"TTL\":10316,\"Name\":\"fit.vutbr.cz\",\"QType\":2,\"QClass\":1},{\"RDataName\":\"rhino.cis.vutbr.cz\",\"TTL\":10316,\"Name\":\"fit.vutbr.cz\",\"QType\":2,\"QClass\":1},{\"RDataName\":\"gate.feec.vutbr.cz\",\"TTL\":10316,\"Name\":\"fit.vutbr.cz\",\"QType\":2,\"QClass\":1},{\"RDataName\":\"guta.fit.vutbr.cz\",\"TTL\":10316,\"Name\":\"fit.vutbr.cz\",\"QType\":2,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responsePTR_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xc2ac;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"2.188.160.194.in-addr.arpa\",\"QType\":12,\"QClass\":1}]",
                "[]",
                "[{\"RDataName\":\"golem.gymzv.sk\",\"TTL\":11204,\"Name\":\"2.188.160.194.in-addr.arpa\",\"QType\":12,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responseCNAME_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xd6eb;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"wis.fit.vutbr.cz\",\"QType\":5,\"QClass\":1}]",
                "[]",
                "[{\"RDataName\":\"agata.fit.vutbr.cz\",\"TTL\":4282,\"Name\":\"wis.fit.vutbr.cz\",\"QType\":5,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responseSOA_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xb872;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"fit.vutbr.cz\",\"QType\":6,\"QClass\":1}]",
                "[]",
                "[{\"MName\":\"guta.fit.vutbr.cz\",\"RName\":\"michal.fit.vutbr.cz\",\"Serial\":201704100,\"Refresh\":10800,\"Retry\":3600,\"Expire\":691200,\"Minimum\":86400,\"TTL\":11026,\"Name\":\"fit.vutbr.cz\",\"QType\":6,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responseTXT_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xdd9d;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"vutbr.cz\",\"QType\":16,\"QClass\":1}]",
                "[]",
                "[{\"RDataName\":\"google-site-verification=kSdrjCE0ee5GKpv_Xtr-18k9Pm1OzRIVXrkm9kIwEAk\",\"TTL\":2618,\"Name\":\"vutbr.cz\",\"QType\":16,\"QClass\":1},{\"RDataName\":\"MS=ms21627876\",\"TTL\":2618,\"Name\":\"vutbr.cz\",\"QType\":16,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_02_pcap)
            );
        }

        [Test]
        public void DNSTest_responseOther_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0x5012;

            var flags = new ushort[2]
            {
                0x0100,
                0x81a0
            };

            var messages = new string[]
            {
                "[{\"Name\":\"www.ietf.org\",\"QType\":1,\"QClass\":1}]",
                "[]",
                "[{\"IPAddress\":\"64.170.98.30\",\"TTL\":1240,\"Name\":\"www.ietf.org\",\"QType\":1,\"QClass\":1},{\"DataBytes\":\"AAEFAwAABwhS0HHHUO8wr54EBGlldGYDb3JnAIgwPGyZ6wBM944GJ8hUZueu2u+qpb9VdAqsTknf6PZ75zdBpWS2laA5rl6WiGqMqWxscM8ICvy2OgldpOklisInAZqRDS9uocMo5Bczw1vJF94mxYbX7Dm/KWHu5wY99umnLaf+4hvg0bg5j55GAY2vsA2JOtXaE7ZKmYOe3594kve7YzTwgvQvZJ0QLg2FBvnVcWpWs0sGK6b2Pe3VAtR9rKQ9IOFHQ8gmxgOOpqLmeBj8zTHsIH/ePKsa51PKVj5N50FuU4C1ZBqFoHKNqMHyxJQan+qyeCzrMLoWsPCKU7ZsNxbIscUy0LyjhOZ37XdZzIUEHfQmh46CjQFBM+U=\",\"TTL\":1240,\"Name\":\"www.ietf.org\",\"QType\":46,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ANSWER,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_sec_pcap)
            );
        }

        [Test]
        public void DNSTest_tcp_xvican01_via_FrameworkController2()
        {
            const ushort messageID = 0xd0d0;

            var flags = new ushort[2]
            {
                0x0100,
                0x8180
            };

            var messages = new string[]
            {
                "[{\"Name\":\"cesnet.cz\",\"QType\":255,\"QClass\":1}]",
                "[]",
                "[{\"IPAddress\":\"195.113.144.205\",\"TTL\":41073,\"Name\":\"nsa.ces.net\",\"QType\":1,\"QClass\":1},{\"IPAddress\":\"2001:718:1:1::144:205\",\"TTL\":41073,\"Name\":\"nsa.ces.net\",\"QType\":28,\"QClass\":1},{\"IPAddress\":\"195.113.144.228\",\"TTL\":2438,\"Name\":\"nsa.cesnet.cz\",\"QType\":1,\"QClass\":1},{\"IPAddress\":\"2001:718:1:101::144:228\",\"TTL\":2438,\"Name\":\"nsa.cesnet.cz\",\"QType\":28,\"QClass\":1},{\"IPAddress\":\"158.196.149.9\",\"TTL\":2200,\"Name\":\"decsys.vsb.cz\",\"QType\":1,\"QClass\":1}]"
            };

            this.CheckQueryAndAnswer(messageID, flags, messages, DNSParseMsg.DNSResponseType.ADDITIONAL,
                PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_03_pcap)
            );
        }

        [Test]
        public void DNSTest_mix1_xvican01_via_FrameworkController2()
        {
            var messageID = new ushort[] {0xcbec, 0xcbec, 0xe5ce, 0xe5ce, 0x4d38, 0x4d38, 0x5724, 0x5724};
            var flags = new ushort[] {0x0100, 0x8180};
            var answers = new int[] {0, 1, 0, 2, 0, 1, 0, 1};
            var authorityNameServers = new int[] {0, 3, 0, 3, 0, 3, 0, 3};
            var additional = new int[] {0, 0, 0, 3, 0, 3, 0, 3};

            this.FrameworkController.ProcessCapture(
                this.PrepareCaptureForProcessing(
                    PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_01_pcap)
                ));
            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            var i = 0;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get SnooperDNS exported objects
            {
                var j = 0;
                var exportedObjectsReference = exportedObjects as SnooperExportDNS;
                Assert.IsNotNull(exportedObjectsReference);
                Assert.AreEqual(2, exportedObjectsReference.ExportObjects.Count);
                foreach (var snooperExportedObjectBase in exportedObjectsReference.ExportObjects.ToArray())
                {
                    var message = (SnooperExportedDataObjectDNS) snooperExportedObjectBase;
                    Assert.AreEqual(message.MessageId, messageID[i]);
                    Assert.AreEqual(message.Flags, flags[j]);
                    Assert.AreEqual(1, message.Queries.Count);
                    Assert.AreEqual(answers[i], message.Answer.Count);
                    Assert.AreEqual(authorityNameServers[i], message.Authority.Count);
                    Assert.AreEqual(additional[i], message.Additional.Count);
                    ++j;
                    ++i;
                }
            }
        }

        [Test]
        public void DNSTest_mix2_xvican01_via_FrameworkController2()
        {
            var messageID = new ushort[] {0x244e, 0x244e, 0xd0d0, 0xd0d0, 0xa400, 0xa400};
            var flags = new ushort[] {0x0100, 0x8380, 0x0100, 0x8180, 0x0100, 0x8183};
            var answers = new int[] {0, 1, 0, 10, 0, 0};
            var authorityNameServers = new int[] {0, 0, 0, 3, 0, 1};
            var additional = new int[] {0, 0, 0, 5, 0, 0};

            this.FrameworkController.ProcessCapture(
                this.PrepareCaptureForProcessing(
                    PcapPath.GetPcap(PcapPath.Pcaps.dns_dns_xvican01_03_pcap)
                ));
            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            var i = 0;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get SnooperDNS exported objects
            {
                var exportedObjectsReference = exportedObjects as SnooperExportDNS;
                Assert.IsNotNull(exportedObjectsReference);
                Assert.AreEqual(2, exportedObjectsReference.ExportObjects.Count);
                foreach (var snooperExportedObjectBase in exportedObjectsReference.ExportObjects.ToArray())
                {
                    var message = (SnooperExportedDataObjectDNS) snooperExportedObjectBase;
                    Assert.AreEqual(message.MessageId, messageID[i]);
                    Assert.AreEqual(message.Flags, flags[i]);
                    Assert.AreEqual(1, message.Queries.Count);
                    Assert.AreEqual(answers[i], message.Answer.Count);
                    Assert.AreEqual(authorityNameServers[i], message.Authority.Count);
                    Assert.AreEqual(additional[i], message.Additional.Count);
                    ++i;
                }
            }
        }
    }
}