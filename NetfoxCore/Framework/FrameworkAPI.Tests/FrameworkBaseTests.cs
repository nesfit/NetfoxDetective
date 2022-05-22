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
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Castle.Windsor;
using Netfox.Core.Database;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.FrameworkAPI.Interfaces;
using Netfox.Persistence;
using NUnit.Framework;
using PacketDotNet;

namespace Netfox.FrameworkAPI.Tests
{
    public class FrameworkBaseTests : UnitTestBaseSetupFixture
    {
        public IFrameworkController FrameworkController { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<PmCaptureBase> PmCaptures { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<PmFrameBase> PmFrames { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<L3Conversation> L3Conversations { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<L4Conversation> L4Conversations { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<L7Conversation> L7Conversations { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<PmCaptureBase> PmCapturesDb { get; set; }


        public override void SetUpSQL()
        {
            base.SetUpSQL();
            this.SetUp();
        }

        public override void SetUpInMemory()
        {
            base.SetUpInMemory();
            this.SetUp();
        }

        private void SetUp()
        {
            this.FrameworkController = this.WindsorContainer.Resolve<IFrameworkController>();
            this.PmCaptures =
                this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmCaptureBase>>();
            this.PmFrames = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>();
            this.L3Conversations =
                this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L3Conversation>>();
            this.L4Conversations =
                this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L4Conversation>>();
            this.L7Conversations =
                this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L7Conversation>>(
                    new Dictionary<string, object>()
                    {
                        {
                            "eagerLoadProperties", new[]
                            {
                                nameof(L7Conversation.UnorderedL7PDUs),
                                nameof(L7Conversation.ConversationFlowStatistics),
                                nameof(L7Conversation.L4Conversation),
                                $"{nameof(L7Conversation.UnorderedL7PDUs)}.{nameof(L7PDU.UnorderedFrameList)}"
                            }
                        }
                    });
        }

        protected void PrintResults()
        {
            using (var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>())
            {
                dbx.Frames.Load();
                dbx.L3Conversations.Load();
                dbx.L4Conversations.Load();
                dbx.L7Conversations.Load();

                var allTrackedFramesCount = dbx.L4Conversations.Sum(l4Conv => l4Conv.Frames.Count);
                allTrackedFramesCount += dbx.L3Conversations.Local.Sum(l3Conv => l3Conv.NonL4Frames.Count);
                var l7FramesCount = dbx.L7Conversations.Sum(a => a.Frames.Count);
                var l7FramesDistinctCount = dbx.L7Conversations.Sum(a => a.Frames.Distinct().Count());
                var l4FramesCount = dbx.L4Conversations.Sum(a => a.Frames.Count);
                var nonl4Frames = dbx.L3Conversations.Local.Sum(a => a.NonL4Frames.Count);

                Console.WriteLine("Pm frames: {0}", dbx.Frames.Count());
                Console.WriteLine("L4+non L4 (all tracked): {0}", allTrackedFramesCount);

                Console.WriteLine("Frames l4: {0}", l4FramesCount);
                Console.WriteLine("Non Frames l4: {0}", nonl4Frames);

                Console.WriteLine("Frames l7: {0}", l7FramesCount);
                Console.WriteLine("Frames l7 distinct: {0}", l7FramesDistinctCount);
                Console.WriteLine("L3Conversations: {0}", dbx.L3Conversations.Count());
                Console.WriteLine("L4Conversations: {0}, TCP:{1}, UDP:{2}", dbx.L4Conversations.Count(),
                    dbx.L4Conversations.Count(c => c.L4ProtocolType == IPProtocolType.TCP),
                    dbx.L4Conversations.Count(c => c.L4ProtocolType == IPProtocolType.UDP));
                Console.WriteLine("L7Conversations: {0}, TCP:{1}, UDP:{2}", dbx.L7Conversations.Local.Count,
                    dbx.L7Conversations.Local.Count(c => c.L4ProtocolType == IPProtocolType.TCP),
                    dbx.L7Conversations.Local.Count(c => c.L4ProtocolType == IPProtocolType.UDP));

                var allTrackedFrames = dbx.L4Conversations.Local.SelectMany(c4 => c4.Frames)
                    .Concat(dbx.L3Conversations.Local.SelectMany(c3 => c3.NonL4Frames)).ToList();

                var allIpv4Frames =
                    dbx.Frames.Local.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetwork);
                var allTrackedIPv4FramesCount =
                    allTrackedFrames.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetwork);
                Console.WriteLine("Pm ipv4 frames: {0}", allIpv4Frames);
                Console.WriteLine("Tracked ipv4 frames: {0}", allTrackedIPv4FramesCount);

                var allIpv6Frames =
                    dbx.Frames.Local.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetworkV6);
                var allTrackedIPv6FramesCount =
                    allTrackedFrames.Count(f => f.SrcAddress.AddressFamily == AddressFamily.InterNetworkV6);
                Console.WriteLine("Pm ipv6 frames: {0}", allIpv6Frames);
                Console.WriteLine("Tracked ipv6 frames: {0}", allTrackedIPv6FramesCount);

                var allL4FramesTracked = dbx.L4Conversations.Local.SelectMany(c => c.Frames).ToList();
                Console.WriteLine("allL4framesTracked: {0} vs distinct {1}", allL4FramesTracked.Count,
                    allL4FramesTracked.Distinct().Count());

                var nonIpFrames = dbx.Frames.Local
                    .Where(f => f.IpProtocol != IPProtocolType.IP && f.IpProtocol != IPProtocolType.IPV6).ToList();
                Console.WriteLine("Non ipv frames: {0}", nonIpFrames.Count);
                Console.WriteLine("PmFrames: {0}, all tracked:{1}, nonIP: {2}, tracked+nonIP: {3}",
                    dbx.Frames.Local.Count, allTrackedFramesCount, nonIpFrames.Count,
                    allTrackedFramesCount + nonIpFrames.Count);

                var nullPortFrames = allTrackedFrames.Where(f => f.DstPort == 0 || f.SrcPort == 0);
                Console.WriteLine("Null port frames: {0}", nullPortFrames.Count());
            }
        }

        protected void CheckConsistency(int framesCount, int l3ConsCount, int l4ConsCount, int l7ConsCount,
            int pdusCount)
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
                Assert.AreEqual(l4Stats, l4ConsCount * 2);
                Assert.AreEqual(l7Stats, l7ConsCount * 2);
            }
        }

        private static Stopwatch _stopwatch;

        protected void StopProcessingStopWatch()
        {
            _stopwatch.Stop();
            Console.WriteLine($"Processing time: {_stopwatch.Elapsed}");
        }

        protected void StartProcessingStopwatch()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }
    }
}