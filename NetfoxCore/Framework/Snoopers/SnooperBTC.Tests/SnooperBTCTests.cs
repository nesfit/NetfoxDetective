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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Core.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperBTC.Interfaces;
using Netfox.Snoopers.SnooperBTC.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperBTC.Tests
{
    internal class SnooperBTCTests : SnooperBaseTests
    {
        public SnooperBTCTests()
        {
            this.SnoopersToUse = new List<ISnooper> { new SnooperBTC() };
        }

        [Test]
        public void BCTest_bitcoin_btc1_part1_pcapng()
        {
            var path = PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc1_part1_pcapng);
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(path));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportBTC exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get BTCSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportBTC) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedDataObjectBTC).Cast<SnooperExportedDataObjectBTC>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, messages.Length);

            Assert.AreEqual(messages[0].StateString, "Half handshake (only response)");
            Assert.AreEqual(messages[0].TypeString, "Registration");
            Assert.AreEqual(messages[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "26.02.2015 8:55:08");
        }

        [Test]
        public void BCTest_bitcoin_btc1_pcapng()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc1_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportBTC exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get BTCSnooper exported objects
            {
                if ((exportedObjects is SnooperExportBTC) && (exportedObjects.TimeStampFirst.ToString().Equals("26.2.2015 8:55:07")) && ((SnooperExportBTC)exportedObjects).ExportObjects.Count > 0)
                {
                    exportedObjectsReference = (SnooperExportBTC)exportedObjects;
                    break;
                }

            }
            Assert.IsNotNull(exportedObjectsReference);

            var objs = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, objs.Length);

            Assert.AreEqual(((SnooperExportedDataObjectBTC)objs[0]).StateString, "Full handshake");
            Assert.AreEqual(((SnooperExportedDataObjectBTC)objs[0]).TypeString, "Registration");
            Assert.AreEqual(((SnooperExportedDataObjectBTC)objs[0]).UserAgentsString, "/Satoshi:0.10.0/");
            Assert.AreEqual(((SnooperExportedDataObjectBTC)objs[0]).TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "26.02.2015 8:55:07");
        }

        [Test]
        public void BCTest_bitcoin_btc2_part1_pcapng()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc2_part1_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(136, this.GetExportedObjectCount());
        }

        [Test]
        public void BCTest_bitcoin_btc2_part2_pcapng()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc2_part2_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
            Assert.IsTrue((bool)(this.SnooperExports.ToArray()[0].ExportObjects[0].ExportValidity == ExportValidity.ValidWhole));
            var obj = this.SnooperExports.ToArray()[0].ExportObjects[0] as SnooperExportedDataObjectBTC;
            Assert.IsTrue(obj.Type == SnooperExportedDataObjectBTCType.Version_Verack);
            Assert.IsTrue(obj.State == SnooperExportedDataObjectBTCState.VersionAndVerackReceived);
        }

        [Test]
        [Category("LongRunning")]
        [Explicit][Category("Explicit")]
        public void BCTest_bitcoin_btc2_pcapng()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc2_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            // number of exports varies between 200 and 210 for some reason (one export per conversation)
            //Assert.LessOrEqual(this.SnooperExports.Count, 210);
            //Assert.GreaterOrEqual(this.SnooperExports.Count, 200);

            // not anymore
            Assert.AreEqual(162, this.SnooperExports.Count);
            Assert.AreEqual(1315, this.GetExportedObjectCount());
        }

        [Test]
        public void BCTest_bitcoin_btc2_snapshot_pcapng()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc2_snapshot_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(2, this.SnooperExports.Count);
            Assert.AreEqual(2, this.GetExportedObjectCount());
        }

        [Test]
        public void BCTest_bitcoin_btc2_tx_01_pcapng()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.bitcoin_btc2_tx_01_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(3, this.GetExportedObjectCount());
        }
    }
}
