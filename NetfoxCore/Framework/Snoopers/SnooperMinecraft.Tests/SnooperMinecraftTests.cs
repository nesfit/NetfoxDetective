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
using Netfox.Snoopers.SnooperMinecraft.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperMinecraft.Tests
{
    internal class SnooperMinecraftTestsDefaultPort : SnooperBaseTests
    {
        [Test]
        public void MinecraftTest_minecraft_xberan33_01_via_FrameworkController2()
        {
            var messagePatterns = new[]
            {
                new[] {"02.12.2014 18:21:05","[G] [VIP] Eneris: asdas" },
                new[] {"02.12.2014 18:21:06","[G] [VIP] Faelon: cau" },
                new[] {"02.12.2014 18:21:12","[G] [VIP] Faelon: koukam vsichni jsme vip" },
                new[] {"02.12.2014 18:21:17","[G] [VIP] Eneris: hasdjkasldasjkda"},
                new[] {"02.12.2014 18:21:22","[G] [VIP] Faelon: nj blazen ses :D"},
                new[] {"02.12.2014 18:21:26","[G] [VIP] Eneris: hua hua" },
                new[] {"02.12.2014 18:21:27","[G] [VIP] Faelon: asdfx" },
                new[] {"02.12.2014 18:21:30","[G] [VIP] Eneris: hihihi" },
                new[] {"02.12.2014 18:21:32","[G] [VIP] Faelon: :)" },
                new[] {"02.12.2014 18:21:36","[G] [VIP] Eneris: hihihahahohohu" },
                new[] {"02.12.2014 18:21:37","[G] [VIP] Faelon: ok konec" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_search_asd_pcap)));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            //Assert.IsTrue(this.SnooperExports.Count == 1);

            SnooperExportMinecraft exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get MinecraftSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportMinecraft) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(11, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedMinecraftMessage).Cast<SnooperExportedMinecraftMessage>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(11, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Message, messagePatterns[i][1]);
            }
            //Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 11);
        }

        [Test]
        public void MinecraftTest_minecraft_xberan33_02_via_FrameworkController2()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_tell_cs_pcap)));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportMinecraft exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get MinecraftSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportMinecraft) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedMinecraftMessage).Cast<SnooperExportedMinecraftMessage>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(1, messages.Length); //Every exported object should be private message

            Assert.AreEqual(messages[0].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "22.03.2015 14:32:55");
            Assert.AreEqual(messages[0].Message, "To buggy08: cs");
            Assert.AreEqual(messages[0].Sender, "");
            Assert.AreEqual(messages[0].Receiver, "buggy08: c");
        }

        [Test]
        public void MinecraftTest_minecraft_xberan33_03_via_FrameworkController2()
        {
            // this test should parse nothing, there are no valid minecraft data for default port
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_incorrect_pcap)));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 0);
        }

        [Test]
        public void MinecraftTest_minecraft_xberan33_04_via_FrameworkController2()
        {
            // test over bigger pcap
            // actually contains mainly modified data, parses one small chat message
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_bigger_pcap)));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 1);
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[0] as SnooperExportedMinecraftMessage).Text == "s");
        }

        [Test]
        public void MinecraftTest_minecraft_xberan33_05_via_FrameworkController2()
        {
            // 13 different chat messages
            // some fom server , for eample third, some from players
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_server_3_pcap)));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 13);
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[3] as SnooperExportedMinecraftMessage).Sender == "Server");
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[12] as SnooperExportedMinecraftMessage).Sender.Contains("Faelon"));
        }

        [Test]
        public void MinecraftTest_minecraft_xberan33_06_via_FrameworkController2()
        {
            // four chat messages, only one with meainngful content, test this content
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_server_2_pcap)));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 4);
            var test = this.SnooperExports.First().ExportObjects[3] as SnooperExportedMinecraftMessage;
            Assert.IsTrue(test != null && test.Text == "ahoj");
        }
    }
}

