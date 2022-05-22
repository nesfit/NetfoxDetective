// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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

using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperMinecraft.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperMinecraft.Tests.Tests
{
    internal class SnooperMinecraftTestsSpecificPort : SnooperBaseTests
    {
        [Test]
        public void MinecraftTest_minecraft_xberan33_07_via_FrameworkController2()
        {
            // test at port 41030
            // two conversations, but no chat data
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_at_port_41030_pcap)));
            var conversations = this.L7Conversations.Where(c => c.DestinationEndPoint.Port == 41030 || c.SourceEndPoint.Port == 41030).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 2);
        }

        [Test]
        public void MinecraftTest_minecraft_xberan33_08_via_FrameworkController2()
        {
            // some chat messages at port 27746
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_at_port_27746_pcap)));
            var conversations = this.L7Conversations.Where(c => c.DestinationEndPoint.Port == 27746 || c.SourceEndPoint.Port == 27746).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 8);
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[7] as SnooperExportedMinecraftMessage).Sender == "Server");
        }
        
        [Test]
        public void MinecraftTest_minecraft_xberan33_09_via_FrameworkController2()
        {
            // Chat at port 27999,mainly player enum
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_at_port_27999_pcap)));
            var conversations = this.L7Conversations.Where(c => c.DestinationEndPoint.Port == 27999 || c.SourceEndPoint.Port == 27999).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 10);
        }
        
        [Test]
        public void MinecraftTest_minecraft_xberan33_10_via_FrameworkController2()
        {
            // more chat at oprt 37892
            // mainly normal chat
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.minecraft_xberan33_minecraft_at_port_37892_pcap)));
            var conversations = this.L7Conversations.Where(c => c.DestinationEndPoint.Port == 37892 || c.SourceEndPoint.Port == 37892).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 23);
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[2] as SnooperExportedMinecraftMessage).Sender.Contains("HrobnikCZ"));
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[2] as SnooperExportedMinecraftMessage).Text.Contains("drak blbne"));
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[8] as SnooperExportedMinecraftMessage).Text == "mizi mi");
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[16] as SnooperExportedMinecraftMessage).Sender.Contains("MrQuick"));
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[16] as SnooperExportedMinecraftMessage).Text == "to dela vsem");
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[18] as SnooperExportedMinecraftMessage).Sender == "MAXIPISEK");
            Assert.IsTrue((this.SnooperExports.First().ExportObjects[18] as SnooperExportedMinecraftMessage).Text == "Hello");
        }
    }
}

