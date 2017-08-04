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

using System.Globalization;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperRTP.Models;
using NUnit.Framework;

namespace Netfox.SnooperRTP.Tests
{
    internal class SnooperRTPTests : SnooperBaseTests
    {

        [Test]
        public void RTPTest_sip_rtcp()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.sip_caps_sip_rtcp_pcap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportRTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get RTPSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportRTP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(0, exportedObjectBases.Length);

            Assert.AreEqual(76, this.SnooperExports.Count);
        }

        [Test]
        public void RTPTest_cisco_g711alaw_pcap()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.voip_cisco_g711alaw_pcap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, false);

            SnooperExportRTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get RTPSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportRTP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(0, exportedObjectBases.Length);

            /*
            var objs = this.SnooperExports.OrderBy(it => it.TimeStampFirst).ToArray();
            Assert.AreEqual(objs[0].TimeStampFirst.ToString(), "16.4.2014 15:23:43");
            Assert.AreEqual(objs[1].TimeStampFirst.ToString(), "16.4.2014 15:23:43");
            Assert.AreEqual(objs[2].TimeStampFirst.ToString(), "16.4.2014 15:23:43");
            Assert.AreEqual(objs[3].TimeStampFirst.ToString(), "16.4.2014 15:23:43");
            Assert.AreEqual(objs[4].TimeStampFirst.ToString(), "16.4.2014 15:23:45");
            Assert.AreEqual(objs[5].TimeStampFirst.ToString(), "16.4.2014 15:23:45");
            Assert.AreEqual(objs[6].TimeStampFirst.ToString(), "16.4.2014 15:23:47");
            Assert.AreEqual(objs[7].TimeStampFirst.ToString(), "16.4.2014 15:23:47");*/
        }

        [Test][Explicit][Category("Explicit")]
        public void RTPTest_voip_rtp_cap()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.voip_rtp_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, false);

            var objs = this.SnooperExports.OrderBy(it => it.TimeStampFirst).ToArray();

            Assert.AreEqual(objs[0].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[1].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[2].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[3].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[4].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[5].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[6].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[7].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[8].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[9].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[10].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:06");
            Assert.AreEqual(objs[11].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:07");
            Assert.AreEqual(objs[12].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:07");
            Assert.AreEqual(objs[13].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:14");
            Assert.AreEqual(objs[14].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:19");
            Assert.AreEqual(objs[15].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:00:35");
            Assert.AreEqual(objs[16].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:01:02");
            Assert.AreEqual(objs[17].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:01:05");
            Assert.AreEqual(objs[18].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:01:11");
            Assert.AreEqual(objs[19].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:01:17");
            Assert.AreEqual(objs[20].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:01:51");
            Assert.AreEqual(objs[21].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:02:10");
            Assert.AreEqual(objs[22].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:02:13");
            Assert.AreEqual(objs[23].TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)), "02.04.2012 10:03:19");
        }
    }
}
