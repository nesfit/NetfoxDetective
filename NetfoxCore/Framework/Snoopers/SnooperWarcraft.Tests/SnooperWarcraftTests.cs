// Copyright (c) 2018 Jan Pluskal, Matus Dobrotka
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
using System.Globalization;
using System.IO;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperWarcraft.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperWarcraft.Tests
{
    internal class SnooperWarcraftTests : SnooperBaseTests
    {
        [Test]
        public void WarcraftTest_warcraft_xberan33_01_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_1_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 526);
            var instanceFound = false;
            var instanceLeaderFound = false;
            var guildFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Instance) instanceFound = true;
                    else if (message.Type == WarcraftMessageType.InstanceLeader) instanceLeaderFound = true;
                    else if (message.Type == WarcraftMessageType.Guild) guildFound = true;
                }
            }

            Assert.IsTrue(instanceFound);
            Assert.IsTrue(instanceLeaderFound);
            Assert.IsTrue(guildFound);
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_02_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_2_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var bnetFound = false;
            var guildFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.PrivateMessageBnet) bnetFound = true;
                    else if (message.Type == WarcraftMessageType.Guild) guildFound = true;
                }
            }

            Assert.IsTrue(bnetFound);
            Assert.IsTrue(guildFound);
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_03_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_3_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var allFound = true;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var found = false;
                    var message = expObject as SnooperExportedWarcraftMessage;
                    switch (message.Type)
                    {
                        case WarcraftMessageType.Channel:
                        case WarcraftMessageType.Say:
                        case WarcraftMessageType.Yell:
                        case WarcraftMessageType.PrivateMessageBnet:
                        case WarcraftMessageType.PrivateMessageIngame:
                        case WarcraftMessageType.Guild:
                        case WarcraftMessageType.Instance:
                        case WarcraftMessageType.Raid:
                        case WarcraftMessageType.RaidLeader:
                        case WarcraftMessageType.RaidWarning:
                        case WarcraftMessageType.Party:
                        case WarcraftMessageType.PartyLeader:
                            found = true;
                            break;
                        default:
                            break;
                    }

                    allFound &= found;
                }
            }

            Assert.IsTrue(allFound);
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_04_via_FrameworkController2()
        {
            var messagePatterns = new[] // Timestampes, senders and receivers, Text
            {
                new[] {$"26.04.{DateTime.Now.Year} 15:13:52", "Current player name", "Some Battle.net friend", "hey, you got a minute?"},
                new[] {$"26.04.{DateTime.Now.Year} 15:14:12", "Some Battle.net friend", "Current player name", "afther dungon sure :P"},
                new[] {$"26.04.{DateTime.Now.Year} 15:16:14", "Current player name", "Some Battle.net friend", "sure ill wait, just to inform, this isnt actually neth, its her boyfriend, she allowed me to borrow her character for a while. "},
                new[] {$"26.04.{DateTime.Now.Year} 15:16:35", "Some Battle.net friend", "Current player name", "npxz"},
                new[] {$"26.04.{DateTime.Now.Year} 15:16:36", "Current player name", "Some Battle.net friend", "I am working on bachelors thesis related to wow and i need to gather some testing data related to chat"},
                new[] {$"26.04.{DateTime.Now.Year} 15:17:21", "Current player name", "Some Battle.net friend", "I need to find all chat types in wow.. beggining with say or yell and ending with dunno bg/instance chat"},
                new[] {$"26.04.{DateTime.Now.Year} 15:17:45", "Some Battle.net friend", "Current player name",  "can u come vent afther then=?"},
                new[] {$"26.04.{DateTime.Now.Year} 15:17:55", "Current player name", "Some Battle.net friend",  "So if you could help me out for a few minutes that would be nice"},
                new[] {$"26.04.{DateTime.Now.Year} 15:18:24", "Current player name", "Some Battle.net friend",  "I probably can, just need to find it in this pc"},
                new[] {$"26.04.{DateTime.Now.Year} 15:19:46", "Current player name", "Some Battle.net friend", "right, im wainting in afk slackers room" }
            };

            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_bnet_whisper_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var bnetFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.PrivateMessageBnet)
                        bnetFound = true;
                }
            }

            Assert.IsTrue(bnetFound);

            SnooperExportWarcraft exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get WarcraftSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportWarcraft) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(10, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedWarcraftMessage).Cast<SnooperExportedWarcraftMessage>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(10, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Sender, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Receiver, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Text, messagePatterns[i][3]);
            }
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_05_via_FrameworkController2()
        {
            var messagePatterns = new[] // Timestampes, senders and receivers, Text
            {
                new[] { $"26.04.{DateTime.Now.Year} 15:38:26", "Kobits-Magtheridon", "Channel 2. LocalDefense", "lf mate killng alphas for  Caged Mighty Wolf"},
                new[] { $"26.04.{DateTime.Now.Year} 15:39:15", "Drulgir", "Channel LocalDefense", "bergruu up"},
                new[] { $"26.04.{DateTime.Now.Year} 15:39:15", "Oeru", "Channel 3. LookingForGroup", "LFM to kill the invading Horde that have taken over Goldshire!" }
            };

            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_channel_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var chanFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Channel)
                        chanFound = true;
                }
            }

            Assert.IsTrue(chanFound);

            SnooperExportWarcraft exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get WarcraftSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportWarcraft) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(3, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedWarcraftMessage).Cast<SnooperExportedWarcraftMessage>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(3, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Sender, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Receiver, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Text, messagePatterns[i][3]);
            }
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_06_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_guild_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var guildFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Guild)
                        guildFound = true;
                }
            }

            Assert.IsTrue(guildFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 3);
            var msg = this.SnooperExports.First().ExportObjects[2] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Sender.Contains("Nethielka"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_07_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_instance_leader_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var iLeaderFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.InstanceLeader)
                        iLeaderFound = true;
                }
            }

            Assert.IsTrue(iLeaderFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 2);
            var msg = this.SnooperExports.First().ExportObjects[1] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("in"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_08_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_instance_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var instanceFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Instance)
                        instanceFound = true;
                }
            }

            Assert.IsTrue(instanceFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 2);
            var msg = this.SnooperExports.First().ExportObjects[0] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("bg"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_09_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_party_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var partyFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Party)
                        partyFound = true;
                }
            }

            Assert.IsTrue(partyFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 2);
            var msg = this.SnooperExports.First().ExportObjects[1] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("i know"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_10_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_party_leader_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var pLeaderFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.PartyLeader)
                        pLeaderFound = true;
                }
            }

            Assert.IsTrue(pLeaderFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 7);
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_11_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_raid_leader_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var raidLeaderFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.RaidLeader)
                        raidLeaderFound = true;
                }
            }

            Assert.IsTrue(raidLeaderFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 3);
            var msg = this.SnooperExports.First().ExportObjects[2] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("i know"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_12_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_raid_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var raidFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Raid)
                        raidFound = true;
                }
            }

            Assert.IsTrue(raidFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 2);
            var msg = this.SnooperExports.First().ExportObjects[1] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("but if"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_13_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_raid_warning_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var rwFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.RaidWarning)
                        rwFound = true;
                }
            }

            Assert.IsTrue(rwFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 5);
            var msg = this.SnooperExports.First().ExportObjects[4] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("is there any"));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_14_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_say_yell_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var sayFound = false;
            var yellFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.Say) sayFound = true;
                    else if (message.Type == WarcraftMessageType.Yell) yellFound = true;
                }
            }

            Assert.IsTrue(sayFound);
            Assert.IsTrue(yellFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 3);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Any(e => (e as SnooperExportedWarcraftMessage).Text.Contains("Say")));
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Any(e => (e as SnooperExportedWarcraftMessage).Text.Contains("Yell")));
        }

        [Test]
        public void WarcraftTest_warcraft_xberan33_15_via_FrameworkController2()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(PcapPath.GetWdat(PcapPath.Wdat.warcraft_xberan33_whisper_wdat))
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            var whisperFound = false;
            foreach (var export in this.SnooperExports)
            {
                foreach (var expObject in export.ExportObjects)
                {
                    var message = expObject as SnooperExportedWarcraftMessage;
                    if (message.Type == WarcraftMessageType.PrivateMessageIngame)
                        whisperFound = true;
                }
            }

            Assert.IsTrue(whisperFound);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 6);
            var msg = this.SnooperExports.First().ExportObjects[4] as SnooperExportedWarcraftMessage;
            Assert.IsTrue(msg.Text.Contains("thank you"));
        }
    }
}

