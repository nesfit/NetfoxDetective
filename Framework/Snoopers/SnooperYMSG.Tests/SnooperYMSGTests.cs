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
using Netfox.SnooperYMSG.Models;
using NUnit.Framework;

namespace Netfox.SnooperYMSG.Tests
{
    public class SnooperYMSGTests : SnooperBaseTests
    {
        [Test]
        public void SnooperYMSGTest()
        {
            var messagePatterns = new[]
            {
                new []{"02.04.2012 9:21:41","bpfitvut","bpfitvut1","ahoj, testovaci zprava"},
                new []{"02.04.2012 9:21:41","bpfitvut1","bpfitvut","odpoved na testovaci zpravu"}
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.pcap_mix_ymsg_without_file_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, false);

            SnooperExportYMSG exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get YMSGSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportYMSG) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(2, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedObjectYMSG).Cast<SnooperExportedObjectYMSG>().OrderBy(it => it.Message).ToArray();
            Assert.AreEqual(2, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Sender, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Receiver, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Message, messagePatterns[i][3]);
            }

        }
    }
}