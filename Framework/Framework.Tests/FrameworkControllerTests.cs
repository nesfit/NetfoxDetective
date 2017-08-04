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
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.Persistence;
using NUnit.Framework;
using PacketDotNet;

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

        private void PrintResults()
        {
            using(var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>())
            {
                dbx.Frames.Load();
                dbx.L3Conversations.Load();
                dbx.L4Conversations.Load();
                dbx.L7Conversations.Load();

                var allTrackedFramesCount = dbx.L4Conversations.Sum(l4Conv => l4Conv.Frames.Count());
                allTrackedFramesCount += dbx.L3Conversations.Local.Sum(l3Conv => l3Conv.NonL4Frames.Count);
                var l7conv = dbx.L7Conversations.First();
                var rames = l7conv.Frames;
                var l7FramesCount = dbx.L7Conversations.Sum(l7Conv => l7Conv.Frames.Count());
                var l7FramesDistinctCount = dbx.L7Conversations.Sum(l7Conv => l7Conv.Frames.Distinct().Count());
                var l4FramesCount = dbx.L4Conversations.Sum(l4Conv => l4Conv.Frames.Count());
                var nonl4Frames = dbx.L3Conversations.Local.Sum(l3Conv => l3Conv.NonL4Frames.Count());

                Console.WriteLine("Pm frames: {0}", dbx.Frames.Count());
                Console.WriteLine("L4+non L4 (all tracked): {0}", allTrackedFramesCount);

                Console.WriteLine("Frames l4: {0}", l4FramesCount);
                Console.WriteLine("Non Frames l4: {0}", nonl4Frames);

                Console.WriteLine("Frames l7: {0}", l7FramesCount);
                Console.WriteLine("Frames l7 distinct: {0}", l7FramesDistinctCount);
                Console.WriteLine("L3Conversations: {0}", dbx.L3Conversations.Count());
                Console.WriteLine("L4Conversations: {0}, TCP:{1}, UDP:{2}", dbx.L4Conversations.Count(), dbx.L4Conversations.Count(c => c.L4ProtocolType == IPProtocolType.TCP),
                    dbx.L4Conversations.Count(c => c.L4ProtocolType == IPProtocolType.UDP));
                Console.WriteLine("L7Conversations: {0}, TCP:{1}, UDP:{2}", dbx.L7Conversations.Local.Count(), dbx.L7Conversations.Local.Count(c => c.L4ProtocolType == IPProtocolType.TCP),
                   dbx.L7Conversations.Local.Count(c => c.L4ProtocolType == IPProtocolType.UDP));
                
                var allTrackedFrames = dbx.L4Conversations.Local.SelectMany(c4 => c4.Frames).Concat(dbx.L3Conversations.Local.SelectMany(c3 => c3.NonL4Frames));

                var allIpv4Frames = dbx.Frames.Local.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetwork);
                var allTrackedIPv4FramesCount = allTrackedFrames.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetwork);
                Console.WriteLine("Pm ipv4 frames: {0}", allIpv4Frames);
                Console.WriteLine("Tracked ipv4 frames: {0}", allTrackedIPv4FramesCount);

                var allIpv6Frames = dbx.Frames.Local.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetworkV6);
                var allTrackedIPv6FramesCount = allTrackedFrames.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetworkV6);
                Console.WriteLine("Pm ipv6 frames: {0}", allIpv6Frames);
                Console.WriteLine("Tracked ipv6 frames: {0}", allTrackedIPv6FramesCount);

                var allL4framesTracked = dbx.L4Conversations.Local.SelectMany(c => c.Frames);
                Console.WriteLine("allL4framesTracked: {0} vs distinct {1}", allL4framesTracked.Count(), allL4framesTracked.Distinct().Count());

                var nonIpFrames = dbx.Frames.Local.Where(f => f.IpProtocol != IPProtocolType.IP && f.IpProtocol != IPProtocolType.IPV6);
                Console.WriteLine("Non ipv frames: {0}", nonIpFrames.Count());
                Console.WriteLine("PmFrames: {0}, all tracked:{1}, nonIP: {2}, tracked+nonIP: {3}", dbx.Frames.Local.Count(), allTrackedFramesCount, nonIpFrames.Count(),
                    allTrackedFramesCount + nonIpFrames.Count());

                var nullPortFrames = allTrackedFrames.Where(f => f.DstPort == 0 || f.SrcPort == 0);
                Console.WriteLine("Null port frames: {0}", nullPortFrames.Count());
            }
        }

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

        private void StopProcessingStopWatch()
        {
            Stopwatch.Stop();
            Console.WriteLine($"Processing time: {Stopwatch.Elapsed}");
        }

        private void StartProcessingStopwatch()
        {
            Stopwatch = new Stopwatch();
            Stopwatch.Start();
        }

        public static Stopwatch Stopwatch { get; set; }

        private  void CheckConsistency(int framesCount, int l3ConsCount, int l4ConsCount, int l7ConsCount, int pdusCount)
        {
            using (var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>())
            {
                var frames = dbx.Set<PmFrameBase>().Count();
                var l3Cons = dbx.Set<L3Conversation>().Count();
                var l3Stats = dbx.L3ConversationStatistics.Count();
                var l4Cons = dbx.Set<L4Conversation>().Count();
                var l4Stats = dbx.L4ConversationStatistics.Count();
                var l7Cons = dbx.Set<L7Conversation>().Count();
                var l7Stats = dbx.L7ConversationStatistics.Count();
                var pdus = dbx.Set<L7PDU>().Count();

                Console.WriteLine($"Frames: {frames}");
                Console.WriteLine($"l3cons: {l3Cons}");
                Console.WriteLine($"l3Stats: {l3Stats}");
                Console.WriteLine($"l4cons: {l4Cons}");
                Console.WriteLine($"l4Stats: {l4Stats}");
                Console.WriteLine($"l7cons: {l7Cons}");
                Console.WriteLine($"l7Stats: {l7Stats}");
                Console.WriteLine($"pdus: {pdus}");

                
                Assert.AreEqual(l3Cons, l3ConsCount);
                Assert.AreEqual(l4Cons, l4ConsCount);
                Assert.AreEqual(l7Cons, l7ConsCount);
                Assert.AreEqual(pdus, pdusCount);

                Assert.AreEqual(frames, framesCount);

                Assert.IsTrue(l3Stats == l3ConsCount * 2 || l3Stats == l3ConsCount * 2 - 2);
                Assert.AreEqual(l4Stats , l4ConsCount * 2);
                Assert.AreEqual(l7Stats , l7ConsCount * 2);
            }
           
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