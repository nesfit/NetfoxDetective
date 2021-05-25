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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperSIP.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperSIP.Tests
{
    internal class SnooperSIPTests : SnooperBaseTests
    {
        public SnooperSIPTests()
        {
            this.SnoopersToUse = new List<ISnooper> { new SnooperSIP() };
        }

        [Test]
        public void SIPTest_sip_caps_cancelled()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_cancelled_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);
            var eo = this.SnooperExports.FirstOrDefault();

            //Assert.AreEqual(2, this.SnooperExports.Count);

            SnooperExportSIP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get SIPSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportSIP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var calls = exportedObjectBases.Where(i => i is SIPCall).Cast<SIPCall>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, calls.Length);

            Assert.AreEqual(calls[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "21.02.2011 17:27:35");
            Assert.AreEqual(calls[0].From, "\"910081229\"<sip:910081229@81.91.216.18>");
            Assert.AreEqual(calls[0].To, "\"774693213\"<sip:774693213@81.91.216.18>");
            Assert.AreEqual(calls[0].RejectReason, "Unauthorized");
        }

        [Test]
        public void SIPTest_sip_caps_direct_call__outgoing___sip()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_direct_call__outgoing___sip_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            //Assert.AreEqual(35, this.SnooperExports.Count);

            SnooperExportSIP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get SIPSnooper exported objects
            {
                if ((exportedObjects is SnooperExportSIP) && (exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("22.06.2011 9:01:48")))
                {
                    exportedObjectsReference = (SnooperExportSIP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var calls = exportedObjectBases.Where(i => i is SIPCall).Cast<SIPCall>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, calls.Length);

            Assert.AreEqual(calls[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "22.06.2011 9:01:48");
            Assert.AreEqual(calls[0].From, "\"unknown\" <sip:10.10.10.215>");
            Assert.AreEqual(calls[0].To, "\"unknown\"<sip:10.10.10.109>");
            Assert.AreEqual(calls[0].RejectReason, "");
            //Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_h323sip()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_h323sip_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);


            SnooperExportSIP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get SIPSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportSIP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(2, exportedObjectBases.Length);
        }

        [Test]
        public void SIPTest_sip_caps_rejected()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_rejected_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(3, this.SnooperExports.Count);
            Assert.AreEqual(3, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_sip_h323_matej()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_h323_matej_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_sip_incoming()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_incoming_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_sip_konference_na_asterisku()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_konference_na_asterisku_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(2, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_sip_no_inv()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_no_inv_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(8, this.SnooperExports.Count);
            Assert.AreEqual(7, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_sip_no_ok()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_no_ok_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(8, this.SnooperExports.Count);
            Assert.AreEqual(7, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_sip_caps_sip_rtcp()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_rtcp_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(2, this.SnooperExports.Count);
            Assert.AreEqual(2, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_cisco_g711alaw()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_cisco_g711alaw_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(3, this.SnooperExports.Count);
            Assert.AreEqual(3, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_cisco_g711ulaw()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_cisco_g711ulaw_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(3, this.SnooperExports.Count);
            Assert.AreEqual(3, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_cisco_g729br8()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_cisco_g729br8_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(3, this.SnooperExports.Count);
            Assert.AreEqual(3, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_cisco_g729r8()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_cisco_g729r8_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(3, this.SnooperExports.Count);
            Assert.AreEqual(3, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_amr_w()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_amr_wb_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_g7221()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_g7221_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_g722()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_g722_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_g726_16()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_g726_16_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_g726_24()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_g726_24_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_g726_32()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_g726_32_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_g726_40()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_g726_40_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_gsm()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_gsm_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_netbox_to_cesnet_test_call_number()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_netbox_to_cesnet_test_call_number_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(44, this.SnooperExports.Count);
            Assert.AreEqual(6, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_pcma()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_pcma_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_pcmu()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_pcmu_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_silk8()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_silk8_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_silk16()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_silk16_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_speex8()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_speex8_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count); 
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ekiga_speex16()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ekiga_speex16_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_alaw()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_alaw_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_amr_12()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_amr_12_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g726_16()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g726_16_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g726_24()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g726_24_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g726_32()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g726_32_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g726_40()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g726_40_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g729()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g729_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g729a()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g729a_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g729b()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g729b_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g7231_5k()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g7231_5k_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_g7231_6k()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_g7231_6k_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_baapi_a_ulaw()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_baapi_a_ulaw_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }
        
        [Test]
        public void SIPTest_voip_ixia_babgk_a_alaw()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_alaw_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_amr_12()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_amr_12_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g726_16()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g726_16_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g726_24()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g726_24_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g726_32()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g726_32_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g726_40()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g726_40_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g729()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g729_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g729a()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g729a_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g729b()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g729b_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g7231_5k()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g7231_5k_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_g7231_6k()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_g7231_6k_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void SIPTest_voip_ixia_babgk_a_ulaw()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.voip_ixia_babgk_a_ulaw_pcap)));

            var conversations = this.L7Conversations.ToArray();
            
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, false);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }
    }
}
