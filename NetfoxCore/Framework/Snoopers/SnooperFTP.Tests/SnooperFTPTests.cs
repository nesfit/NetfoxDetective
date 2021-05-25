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
using ApplicationProtocolExport.Tests;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperFTP.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperFTP.Tests
{
    internal class SnooperFTPTests : SnooperBaseTests
    {
        //    public ConcurrentBag<SnooperExportBase> SnooperExports;

        //        [SetUp]
        //    public new void SetUpSQL()
        //    {
        //      base.SetUpSQL();
        //            this.SnooperExports = new ConcurrentBag<SnooperExportBase>();
        //    }


        [Test]
        public void FTPTest_ftp_xkarpi03_01_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new[] {"22.01.2015 13:01:10", "USER", "22222"  },
                new[] { "22.01.2015 13:01:10", "PASSWORD", "111111"},
                new[] { "22.01.2015 13:01:13", "PATH", "/" },
                new[] { "22.01.2015 13:01:13","PORT","147.229.178.169:27227"},
                new[] {"22.01.2015 13:01:13", "DIRECTORY LIST", "" },
                new[] {"22.01.2015 13:01:37", "PATH", "/transfer"  },
                new[] {"22.01.2015 13:01:37","PORT","147.229.178.169:27229" },
                new[] { "22.01.2015 13:01:37", "DIRECTORY LIST", "" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_01_pcap)
            ));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            //Assert.AreEqual(3, this.SnooperExports.Count);

            SnooperExportFTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get FTPSnooper exported objects
            {
                if ((exportedObjects is SnooperExportFTP) && (exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("22.01.2015 13:01:10")))
                {
                    exportedObjectsReference = (SnooperExportFTP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(8, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedDataObjectFTP).Cast<SnooperExportedDataObjectFTP>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(8, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Command, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Value, messagePatterns[i][2]);
            }

            //Assert.AreEqual(10, this.GetExportedObjectCount());
        }



        [Test]
        public void FTPTest_ftp_xkarpi03_02_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_02_pcap)
            ));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(5, this.SnooperExports.Count);
            Assert.AreEqual(21, this.GetExportedObjectCount());
        }

        [Test]
        public void FTPTest_ftp_xkarpi03_03_upload_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_03_upload_pcap)
            ));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(5, this.SnooperExports.Count);
            Assert.AreEqual(24, this.GetExportedObjectCount());
        }

        [Test]
        public void FTPTest_ftp_xkarpi03_04_delete_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_04_delete_pcap)
            ));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(4, this.SnooperExports.Count);
            Assert.AreEqual(14, this.GetExportedObjectCount());
        }

        [Test]
        public void FTPTest_ftp_xkarpi03_05_download_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new[] {"22.01.2015 15:56:47", "USER", "22222"  },
                new[] { "22.01.2015 15:56:47", "PASSWORD", "111111"},
                new[] {"22.01.2015 15:56:49", "PATH", "/" },
                new[] {"22.01.2015 15:56:49","PORT","147.229.178.169:31083" },
                new[] {"22.01.2015 15:56:49", "DIRECTORY LIST", ""},
                new[] { "22.01.2015 15:56:52", "PATH", "/transfer"},
                new[] { "22.01.2015 15:56:52","PORT","147.229.178.169:31084" },
                new[] {"22.01.2015 15:56:52", "DIRECTORY LIST", ""  },
                new[] {"22.01.2015 15:56:58","PORT","147.229.178.169:31091" },
                new[] {"22.01.2015 15:56:58","DOWNLOAD","05012011.jpg" }
            };
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_05_download_pcap)
            ));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            //Assert.AreEqual(4, this.SnooperExports.Count);

            SnooperExportFTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get FTPSnooper exported objects
            {
                if ((exportedObjects is SnooperExportFTP) && (exportedObjects.TimeStampFirst.ToString(new CultureInfo("cs-CZ", false)).Equals("22.01.2015 15:56:47")))
                {
                    exportedObjectsReference = (SnooperExportFTP)exportedObjects;
                    break;
                }
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(10, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedDataObjectFTP).Cast<SnooperExportedDataObjectFTP>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(10, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Command, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Value, messagePatterns[i][2]);
            }
        }

        [Test]
        public void FTPTest_ftp_xkarpi03_06_text_upload_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_06_text_upload_pcap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(4, this.SnooperExports.Count);
            Assert.AreEqual(12, this.GetExportedObjectCount());
        }

        //private void FrameworkController_SnooperExport(FcOperationContext fcOperationContext, SnooperExportBase snooperExport) => this.SnooperExports.Add(snooperExport);
        //        #region Non controller tests
        //        [Test]
        //        public void FTPTest_ftp_xkarpi03_01()
        //        {
        //            var captureProcessor = new CaptureProcessor(SnoopersPcaps.Default.ftp_ftp_xkarpi03_01_pcap);
        //            captureProcessor.TrackConversations();
        //            var conversations = captureProcessor.GetConversations("ftp-data");
        //            var exportPath = this.currentTestBaseDirectory;
        //            var selectedConversations = new SelectedConversations(conversations);
        //            var snooper = new SnooperFTP(selectedConversations, exportPath);

        //            snooper.Run();

        //            //Assert.IsTrue(snooper.TotalExportedObjectCount == 10);
        //        }

        //        [Test]
        //        public void FTPTest_ftp_xkarpi03_02()
        //        {
        //            var captureProcessor = new CaptureProcessor(SnoopersPcaps.Default.ftp_ftp_xkarpi03_02_pcap);
        //            captureProcessor.TrackConversations();
        //            var conversations = captureProcessor.GetConversations("ftp-data");
        //            var exportPath = this.currentTestBaseDirectory;
        //            var selectedConversations = new SelectedConversations(conversations);
        //            var snooper = new SnooperFTP(selectedConversations, exportPath);

        //            snooper.Run();

        //			//Assert.IsTrue(snooper.TotalExportedObjectCount == 21);
        //		}

        //        [Test]
        //        public void FTPTest_ftp_xkarpi03_03_upload()
        //        {
        //            var captureProcessor = new CaptureProcessor(SnoopersPcaps.Default.ftp_ftp_xkarpi03_03_upload_pcap);
        //            captureProcessor.TrackConversations();
        //            var conversations = captureProcessor.GetConversations("ftp-data");
        //            var exportPath = this.currentTestBaseDirectory;
        //            var selectedConversations = new SelectedConversations(conversations);
        //            var snooper = new SnooperFTP(selectedConversations, exportPath);

        //            snooper.Run();

        //			//Assert.IsTrue(snooper.TotalExportedObjectCount == 24);
        //		}

        //        [Test]
        //        public void FTPTest_ftp_xkarpi03_04_delete()
        //        {
        //            var captureProcessor = new CaptureProcessor(SnoopersPcaps.Default.ftp_ftp_xkarpi03_04_delete_pcap);
        //            captureProcessor.TrackConversations();
        //            var _conversations = captureProcessor.GetConversations("ftp-data");
        //            var exportPath = this.currentTestBaseDirectory;
        //            var selectedConversations = new SelectedConversations(_conversations);
        //            var snooper = new SnooperFTP(selectedConversations, exportPath);

        //            snooper.Run();

        //			//Assert.IsTrue(snooper.TotalExportedObjectCount == 14);
        //		}

        //        [Test]
        //        public void FTPTest_ftp_xkarpi03_05_download()
        //        {
        //            var captureProcessor = new CaptureProcessor(SnoopersPcaps.Default.ftp_ftp_xkarpi03_05_download_pcap);
        //            captureProcessor.TrackConversations();
        //            var conversations = captureProcessor.GetConversations("ftp-data");
        //            var exportPath = this.currentTestBaseDirectory;
        //            var selectedConversations = new SelectedConversations(conversations);
        //            var snooper = new SnooperFTP(selectedConversations, exportPath);

        //            snooper.Run();

        //			//Assert.IsTrue(snooper.TotalExportedObjectCount == 24);
        //		}

        //        [Test]
        //        public void FTPTest_ftp_xkarpi03_06_text_upload()
        //        {
        //            var captureProcessor = new CaptureProcessor(SnoopersPcaps.Default.ftp_ftp_xkarpi03_06_text_upload_pcap);
        //            captureProcessor.TrackConversations();
        //            var conversations = captureProcessor.GetConversations("ftp-data");
        //            var exportPath = this.currentTestBaseDirectory;
        //            var selectedConversations = new SelectedConversations(conversations);
        //            var snooper = new SnooperFTP(selectedConversations, exportPath);

        //            snooper.Run();

        //			//Assert.IsTrue(snooper.TotalExportedObjectCount == 12);
        //		}
        //#endregion
    }
}
