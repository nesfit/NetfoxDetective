// Copyright (c) 2017 Martin Vondracek
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

using Netfox.Core.Infrastructure;
using Netfox.FrameworkAPI.Tests;
using NUnit.Framework;

namespace Netfox.Framework.Tests.L7PDUTracking.DVB
{
    [TestFixture]
    public class L7DvbS2GseDecapsulatorBlockTests : FrameworkBaseTests
    {
        [SetUp]
        public override void SetUpInMemory()
        {
            base.SetUpInMemory();
            var settings = this.WindsorContainer.Resolve<INetfoxSettings>();
            settings.DecapsulateGseOverUdp = true;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            var settings = this.WindsorContainer.Resolve<INetfoxSettings>();
            settings.DecapsulateGseOverUdp = false;
        }
        
        [Test]
        public void UDP_DVB_S2_GSE_8_UPs_in_BB_frame()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.GSE_UDP_DVB_S2_GSE_8_UPs_in_BB_frame_pcap));
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.PrintResults();
            this.CheckConsistency(framesCount: 1+8, l3ConsCount: 1+1, l4ConsCount: 1, l7ConsCount: 1, pdusCount: 1);
        }

        [Test]
        public void UDP_DVB_S2_GSE_complete_UP() 
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.GSE_UDP_DVB_S2_GSE_complete_UP_pcap));
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.PrintResults();
            this.CheckConsistency(framesCount: 1+1, l3ConsCount: 1+1, l4ConsCount: 1, l7ConsCount: 1, pdusCount: 1);
        }
        
        [Test]
        public void UDP_DVB_S2_GSE_complete_UP_2()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.GSE_UDP_DVB_S2_GSE_complete_UP_2_pcap));
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.PrintResults();
            this.CheckConsistency(framesCount: 2+2, l3ConsCount: 1+1, l4ConsCount: 1, l7ConsCount: 1, pdusCount: 2);
        }
        
        [Test]
        public void UDP_DVB_S2_GSE_fragmented_UP()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.GSE_UDP_DVB_S2_GSE_fragmented_UP_pcap));
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.PrintResults();
            this.CheckConsistency(framesCount: 6+2, l3ConsCount: 1+1, l4ConsCount: 1, l7ConsCount: 1, pdusCount: 6);
        }

        [Test]
        public void UDP_DVB_S2_GSE_intermediate_UP()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.GSE_UDP_DVB_S2_GSE_intermediate_UP_pcap));
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.PrintResults();
            this.CheckConsistency(framesCount: 1, l3ConsCount: 1, l4ConsCount: 1, l7ConsCount: 1, pdusCount: 1);
        }
    }
}