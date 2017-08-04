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
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperSMTP.Models;
using NUnit.Framework;

namespace Netfox.SnooperSMTP.Tests
{
    internal class SnooperSMTPTests : SnooperBaseTests
    {
        [Test]
        public void SMTPTest_email_pop_smtp_1_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_pop_smtp_1_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(6, this.SnooperExports.Count);

            SnooperExportSMTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get smtpSnooper exported objects
            {
                if ((exportedObjects is SnooperExportSMTP) && exportedObjects.ExportObjects.Count > 0)
                {
                    exportedObjectsReference = (SnooperExportSMTP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var emails = exportedObjectBases.Where(i => i is MIMEemail).Cast<MIMEemail>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, emails.Length);

            Assert.AreEqual(emails[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "05.10.2009 6:06:10");
            Assert.AreEqual(emails[0].From, "\"Gurpartap Singh\" <gurpartap@patriots.in>");
            Assert.AreEqual(emails[0].To, "<raj_deol2002in@yahoo.co.in>");
            Assert.AreEqual(emails[0].RawContent, "This is a multipart message in MIME format.\r\n\r\n");
            Assert.AreEqual(emails[0].Subject, "SMTP");
        }

        [Test]
        public void SMTPTest_email_imap_smtp_collector_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_imap_smtp_collector_pcap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(64, this.SnooperExports.Count);

            SnooperExportSMTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get smtpSnooper exported objects
            {
                if ((exportedObjects is SnooperExportSMTP) && (exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("19.08.2013 15:40:43")))
                {
                    exportedObjectsReference = (SnooperExportSMTP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var emails = exportedObjectBases.Where(i => i is MIMEemail).Cast<MIMEemail>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, emails.Length);

            Assert.AreEqual(emails[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "19.08.2013 15:40:43");
            Assert.AreEqual(emails[0].From, "TestPC01 <testpc01@seznam.cz>");
            Assert.AreEqual(emails[0].To, "testpc02@seznam.cz");
            Assert.AreEqual(emails[0].RawContent, "1 Ve zku?ebn? dob? m??e zru?it pracovn? pom?r zam?stnavatel i \r\nzam?stnanec a ??dn? z nich nemus? uv?d?t d?vody sv?ho rozhodnut?.\r\n2 Firma u? v p??pad? d??ve privilegovan?ch zam?stnanc? nemus? ??dat o \r\np?edchoz? souhlas p??slu?n? odborov? org?n.\r\n3 Zku?ebn? dobu lze sjednat pouze p?ed vznikem pracovn?ho pom?ru.");
            Assert.AreEqual(emails[0].Subject, "1 - 3");

            Assert.AreEqual(19, this.GetExportedObjectCount());
        }

        [Test]
        public void SMTPTest_email_imap_smtp_pc2_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_imap_smtp_pc2_pcap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(27, this.SnooperExports.Count);

            SnooperExportSMTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get smtpSnooper exported objects
            {
                if ((exportedObjects is SnooperExportSMTP) && (exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("19.08.2013 15:45:16")))
                {
                    exportedObjectsReference = (SnooperExportSMTP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var emails = exportedObjectBases.Where(i => i is MIMEemail).Cast<MIMEemail>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, emails.Length);

            Assert.AreEqual(emails[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "19.08.2013 15:45:16");
            Assert.AreEqual(emails[0].From, "TestPC02 <testpc02@seznam.cz>");
            Assert.AreEqual(emails[0].To, "testpc03@seznam.cz");
            Assert.AreEqual(emails[0].RawContent, "11 Zku?ebn? dobu nen? mo?n? sjednat zp?tn?.\r\n12 Podle Jouzy pokud firma s pracovn?kem u? podepsala pracovn? smlouvu,\r\n13 nem??e mu n?sledn? vnutit dodatek k t?to smlouv?,\r\n14 v n?m? zku?ebn? dobu se zam?stnancem sjedn?.\r\n.");
            Assert.AreEqual(emails[0].Subject, "11 - 14");

            //Assert.AreEqual(6, this.GetExportedObjectCount());
        }

        [Test]
        public void SMTPTest_email_imap_smtp_collector_filtered_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_imap_smtp_collector_filtered_pcap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);

            SnooperExportSMTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get smtpSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportSMTP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(0, exportedObjectBases.Length);

            Assert.AreEqual(0, this.GetExportedObjectCount());
        }

        [Test]
        public void SMTPTest_email_test_imap_1_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_test_imap_1_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);

            SnooperExportSMTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get smtpSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportSMTP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);
            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(0, exportedObjectBases.Length);

            Assert.AreEqual(0, this.GetExportedObjectCount());
        }

        [Test]
        public void SMTPTest_email_test_imap_1_nm_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.email_test_imap_1_nm_cap));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);

            SnooperExportSMTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get smtpSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportSMTP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(0, exportedObjectBases.Length);

            Assert.AreEqual(0, this.GetExportedObjectCount());
        }
    }
}
