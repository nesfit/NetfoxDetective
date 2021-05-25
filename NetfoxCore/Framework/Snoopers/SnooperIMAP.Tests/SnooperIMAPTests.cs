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
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperIMAP.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperIMAP.Tests
{
    internal class SnooperIMAPTests : SnooperBaseTests
    {
        [Test]
        public void IMAPTest_email_imap_smtp_collector_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new[] { "19.08.2013 15:42:26", "TestPC02 <testpc02@seznam.cz>", "TestPC01 <testpc01@seznam.cz>", "1 - 3 rtf", "This is a multi-part message in MIME format.\r\n"},
                new[] { "19.08.2013 15:43:42", "TestPC02 <testpc02@seznam.cz>", "TestPC01 <testpc01@seznam.cz>", "4 - 8 rtf", "This is a multi-part message in MIME format.\r\n" },
                new[] { "19.08.2013 15:44:43", "TestPC02 <testpc02@seznam.cz>", "TestPC01 <testpc01@seznam.cz>", "priloha .txt", "This is a multi-part message in MIME format.\r\n" },
                new[] { "19.08.2013 15:50:23", "TestPC04 <testpc04@seznam.cz>", "testpc01@seznam.cz", "29 - 30", "29 vznikl ji? pracovn? pom?r na z?klad? ?stn? sjednan? pracovn? smlouvy.\r\n30 Dodate?n? p?semn? vyhotoven? pracovn? smlouvy m? u? jen povahu \r\npotvrzen? toho,\r\n" },
                new[] { "19.08.2013 15:54:11", "\"TestPC03@seznam.cz\" <testpc03@seznam.cz>", "testpc01@seznam.cz", "46 - 50 rtf", "This is a multi-part message in MIME format.\r\n" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.email_imap_smtp_collector_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportIMAP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get IMAPSnooper exported objects
            {
                if ((exportedObjects is SnooperExportIMAP) && exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("19.08.2013 15:42:26"))
                {
                    exportedObjectsReference = (SnooperExportIMAP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(5, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is MIMEemail).Cast<MIMEemail>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(5, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length); //Both arrays should have same length

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].From, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].To, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Subject, messagePatterns[i][3]);
                Assert.AreEqual(messages[i].RawContent, messagePatterns[i][4]);
            }
        }
        [Test]
        public void IMAPTest_email_imap_smtp_collector_filtered_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new[] {"19.08.2013 15:49:27","\"TestPC03@seznam.cz\" <testpc03@seznam.cz>","testpc04@seznam.cz","27 - 28","27 nikoliv za novou pracovn? smlouvu.\r\n28 Pokud je pracovn? smlouva vyhotovena p?semn? pozd?ji ne? v den \r\nn?stupu zam?stnance do pr?ce,\r\n" },
                new[] {"19.08.2013 15:51:33","TestPC01 <testpc01@seznam.cz>","testpc04@seznam.cz","priloha pdf","This is a multi-part message in MIME format.\r\n" },
                new[] {"19.08.2013 15:52:59","\"TestPC03@seznam.cz\" <testpc03@seznam.cz>","testpc04@seznam.cz","36 - 40 rtf","This is a multi-part message in MIME format.\r\n" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.email_imap_smtp_collector_filtered_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            //Assert.AreEqual(1, this.SnooperExports.Count);

            SnooperExportIMAP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get IMAPSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportIMAP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(3, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is MIMEemail).Cast<MIMEemail>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(3, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length); //Both arrays should have same length

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].From, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].To, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Subject, messagePatterns[i][3]);
                Assert.AreEqual(messages[i].RawContent, messagePatterns[i][4]);
            }

            //Assert.AreEqual(3, this.GetExportedObjectCount());
        }

        [Test]
        public void IMAPTest_email_imap_smtp_pc2_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.email_imap_smtp_pc2_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(27, this.SnooperExports.Count);
            Assert.AreEqual(7, this.GetExportedObjectCount());
        }

        [Test]
        public void IMAPTest_email_test_imap_1_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.email_test_imap_1_cap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test]
        public void IMAPTest_email_test_imap_1_nm_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.email_test_imap_1_nm_cap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(1, this.GetExportedObjectCount());
        }

        [Test, Ignore("ondemand")]
        public void M57Case()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(@"F:\pcaps\m57\m57.pcap"));

            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("IMAP")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            var emails = this.SnooperExports.Sum(exportBase => exportBase.ExportObjects.Count);
            Console.WriteLine($"DNS queries: {emails}");
        }
    }
}
