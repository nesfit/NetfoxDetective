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
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperICQ.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperICQ.Tests
{
    class SnooperICQTests : SnooperBaseTests
    {
        [Test]
        public void SnooperICQTest()
        {
            var messagePatterns = new[] //Messages timestampes, senders, receivers, messages
            {
                new[]{"23.02.2012 11:34:03", "", "310451170", "<HTML><BODY dir=\"ltr\"><FONT color=\"#000000\" size=\"2\" face=\"Arial\">test</FONT></BODY></HTML>"},
                new[]{"23.02.2012 11:38:42", "", "310451170", "<HTML><BODY dir=\"ltr\"><FONT color=\"#000000\" size=\"2\" face=\"Arial\">zprava pro kontakt</FONT></BODY></HTML>"},
                new[]{"23.02.2012 11:38:48", "310451170", "", "<HTML><BODY>zprava pro klienta</BODY></HTML>" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.pcap_mix_icq1_cap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, false);

            List<SnooperExportICQ> exportedObjectsReferences = new List<SnooperExportICQ>();
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get ICQSnooper exported objects
            {
                if (exportedObjects is SnooperExportICQ)
                    exportedObjectsReferences.Add((SnooperExportICQ)exportedObjects);
            }
            Assert.IsNotNull(exportedObjectsReferences);

            var exportedObjectBases = exportedObjectsReferences[1].ExportObjects.ToArray();//0 3 0 1 - counts of single array elements
            Assert.AreEqual(3, exportedObjectBases.Length); //Total number of exported objects should be 3

            var messages = exportedObjectBases.Where(i => i is SnooperExportedObjectICQ).Cast<SnooperExportedObjectICQ>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(3, messages.Length); //Every exported object should be message
            Assert.AreEqual(messagePatterns.Length, messages.Length); //Both arrays should have same length

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Sender == null ? "" : messages[i].Sender, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Receiver == null ? "" : messages[i].Receiver, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Message, messagePatterns[i][3]);
            }
        }
    }
}
