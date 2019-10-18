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
using System.Globalization;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperPOP.Models;
using NUnit.Framework;

namespace Netfox.SnooperPOP.Tests
{
    internal class SnooperPOPTests : SnooperBaseTests
    {
        [Test]
        public void POPTest_email_pop_smtp_1_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new []{"09.12.2009 17:06:09","USER","xplicotest@yahoo.es"},
                new []{"09.12.2009 17:06:10","PASS","kebablover" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_pop_smtp_1_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportPOP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get POPSnooper exported objects
            {
                if ((exportedObjects is SnooperExportPOP) && exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("09.12.2009 17:06:09"))
                {
                    exportedObjectsReference = (SnooperExportPOP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(4, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedDataObjectPOP).Cast<SnooperExportedDataObjectPOP>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(2, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Type, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Value, messagePatterns[i][2]);
            }
        }

        [Test]
        public void POPTest_email_pop_smtp_1_filtered_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new []{"09.12.2009 17:06:09","USER","xplicotest@yahoo.es"},
                new []{"09.12.2009 17:06:10","PASS","kebablover" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_pop_smtp_1_filtered_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportPOP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get POPSnooper exported objects
            {
                if ((exportedObjects is SnooperExportPOP) && exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("09.12.2009 17:06:09"))
                {
                    exportedObjectsReference = (SnooperExportPOP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(4, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedDataObjectPOP).Cast<SnooperExportedDataObjectPOP>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(2, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Type, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Value, messagePatterns[i][2]);
            }
        }

        [Test, Ignore("ondemand")]
        public void M57Case()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(@"F:\pcaps\m57\m57.pcap"));

            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("POP3")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            var emails = this.SnooperExports.Sum(exportBase => exportBase.ExportObjects.Count);
            Console.WriteLine($"POP3 emails: {emails}");
        }
    }
}
