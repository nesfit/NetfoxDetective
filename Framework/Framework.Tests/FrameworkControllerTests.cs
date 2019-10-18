// Copyright (c) 2017 Jan Pluskal
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
using System.Data.SqlClient;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.Framework.Tests
{
    [TestFixture]
    public class FrameworkControllerInMemoryTests : FrameworkControllerTests
    {
        [SetUp]
        public override void SetUpInMemory()
        {
           base.SetUpInMemory();
        }
    }
    
    [TestFixture]
    public class FrameworkControllerInSQLTests : FrameworkControllerTests
    {
        [SetUp]
        public override void SetUpSQL()
        {
           base.SetUpSQL();
        }
    }


    [TestFixture]
    public abstract class FrameworkControllerTests : FrameworkBaseTests
    {

        [Test]
        //[Explicit][Category("Explicit")]
        public void AddCapture()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(Pcaps.Default.email_imap_smtp_pc2_pcap);
            
            this.StartProcessingStopwatch();
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.StopProcessingStopWatch();
            this.PrintResults();
            this.CheckConsistency(1605, 11, 26, 27, 481);

            //Validated by Wireshark
            //Pm frames: 1605
            //L4 + non L4(all tracked): 1605
            //Frames l4: 853
            //Non Frames l4: 752
            //Frames l7: 853
            //Frames l7 distinct: 853
            //L3Conversations: 11
            //L4Conversations: 26, TCP: 16, UDP: 10
            //L7Conversations: 27, TCP: 17, UDP: 10
            //Pm ipv4 frames: 1605
            //Tracked ipv4 frames: 1605
            //Pm ipv6 frames: 0
            //Tracked ipv6 frames: 0
            //allL4framesTracked: 853 vs distinct 853
            //Non ipv frames: 0
            //PmFrames: 1605, all tracked:1605, nonIP: 0, tracked + nonIP: 1605
            //Null port frames: 752



        }

        [Test]
        [Explicit][Category("Explicit")]
        public void AddCaptureBig()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(Pcaps.Default.pcap_mix_all_export_mix_cap);

            this.StartProcessingStopwatch();
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.StopProcessingStopWatch();
            //this.PrintResults();
            this.CheckConsistency(160728, 644, 5661, 6918, 59347);
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void AddCaptureBig2()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(Pcaps.Default.pcap_mix_isa_http_pcap);

            this.StartProcessingStopwatch();
            this.FrameworkController.ProcessCapture(captureFileInfo);
            this.StopProcessingStopWatch();

            //this.PrintResults();  
            this.CheckConsistency(1333079, 676, 8674, 10128, 261753);
        }


        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            if(this.WindsorContainer?.Kernel.HasComponent(typeof(SqlConnectionStringBuilder))??false) {
                Console.WriteLine($"Database {this.WindsorContainer.Resolve<SqlConnectionStringBuilder>().ConnectionString}");
            }
        }
        
    }
}