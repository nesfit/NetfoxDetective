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

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Security.Principal;
//using Netfox.Framework.L7ConversationTracking.ConversationStore;
//using Netfox.Framework.Models;
//using Netfox.Framework.Properties;
//using Netfox.Framework.UnitTests;
//using NUnit.Framework;

//namespace Netfox.Framework.L7ConversationTracking
//{
//    /// <summary> A defragment and reassemble time intensive tests.</summary>
//    [TestFixture]
//    [Category("LongRunning")]
//    internal class DefragmentAndReassembleTimeIntensiveTests : UnitTestBaseSetupFixture
//    {
//        /// <summary> The test start Date/Time.</summary>
//        private static DateTime _testStart;

//        /// <summary> Tests conversation tracker banchmark new.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public void ConversationTrackerBanchmarkNewTest()
//        {
//            var windowsIdentity = WindowsIdentity.GetCurrent();
//            if(windowsIdentity != null) {
//                if(windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal") { return; }
//            }
//            else
//            {
//                return;
//            }

//            var banchTest = new BanchmarkTester(this.currentTestBaseDirectory);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_10_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_20_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_30_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_40_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_50_pcap);

//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_100_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_200_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_300_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_400_pcap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_500_pcap);

//            banchTest.Print();
//        }

//        //[Test]
//        //[Category("LongRunning")]
//        //public static void ConversationTrackerBanchmarkTest()
//        //{
//        //    var windowsIdentity = WindowsIdentity.GetCurrent();
//        //    if (windowsIdentity != null)
//        //    {
//        //        if (windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal")
//        //            return;
//        //    }
//        //    else
//        //    {
//        //        return;
//        //    }
//        //    var totalTime = new TimeSpan[5];
//        //    var openCap = new TimeSpan[5];
//        //    var btTrack = new TimeSpan[5];
//        //    var convTrack = new TimeSpan[5];
//        //    var convs = new int[5];
//        //    var timer = new Stopwatch();
//        //    StreamWriter writer = new StreamWriter(@"..\..\..\TestingData\\banchmark\ConversationTrackerBanchmarkTest.csv");
//        //    writer.WriteLine(@"10, 20, 30, 40, 50");
//        //    Console.WriteLine(@"10, 20, 30, 40, 50");
//        //    for (var i = 0; i < 10; i++)
//        //    {
//        //        timer.Reset();
//        //        timer.Start();
//        //        var capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\10.pcap");
//        //        capture.OpenCapture();
//        //        openCap[0] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[0] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[0] = timer.Elapsed;
//        //        timer.Stop();
//        //        var conversations = capture.GetConversations();
//        //        totalTime[0] = timer.Elapsed;
//        //        convs[0] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\20.pcap");
//        //        capture.OpenCapture();
//        //        openCap[1] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[1] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[1] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[1] = timer.Elapsed;
//        //        convs[1] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\30.pcap");
//        //        capture.OpenCapture();
//        //        openCap[2] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[2] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[2] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[2] = timer.Elapsed;
//        //        convs[2] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\40.pcap");
//        //        capture.OpenCapture();
//        //        openCap[3] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[3] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[3] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[3] = timer.Elapsed;
//        //        convs[3] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\50.pcap");
//        //        capture.OpenCapture();
//        //        openCap[4] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[4] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[4] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();

//        //        writer.WriteLine(totalTime[0] + "," + totalTime[1] + "," + totalTime[2] + "," + totalTime[3] + "," + totalTime[4]);
//        //        Console.WriteLine(totalTime[0] + "," + totalTime[1] + "," + totalTime[2] + "," + totalTime[3] + "," + totalTime[4]);
//        //    }
//        //    writer.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4]);
//        //    Console.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4]);
//        //    writer.Close();
//        //}

//        //[Test]
//        //[Category("LongRunning")]
//        //public static void ConversationTrackerBanchmarkHugeTest()
//        //{
//        //    var windowsIdentity = WindowsIdentity.GetCurrent();
//        //    if (windowsIdentity != null)
//        //    {
//        //        if (windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal")
//        //            return;
//        //    }
//        //    else
//        //    {
//        //        return;
//        //    }
//        //    var totalTime = new TimeSpan[5];
//        //    var openCap = new TimeSpan[5];
//        //    var btTrack = new TimeSpan[5];
//        //    var convTrack = new TimeSpan[5];
//        //    var convs = new int[5];
//        //    var timer = new Stopwatch();
//        //    StreamWriter writer = new StreamWriter(@"..\..\..\TestingData\\banchmark\ConversationTrackerBanchmarkHugeTest.csv");
//        //    writer.WriteLine(@"100, 200, 300, 400, 500");
//        //    Console.WriteLine(@"100, 200, 300, 400, 500");
//        //    for (var i = 0; i < 10; i++)
//        //    {
//        //        timer.Reset();
//        //        timer.Start();
//        //        var capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\100.pcap");
//        //        capture.OpenCapture();
//        //        openCap[4] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[4] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[4] = timer.Elapsed;
//        //        timer.Stop();
//        //        var conversations = capture.GetConversations();
//        //        totalTime[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\200.pcap");
//        //        capture.OpenCapture();
//        //        openCap[4] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[4] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[4] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\300.pcap");
//        //        capture.OpenCapture();
//        //        openCap[4] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[4] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[4] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\400.pcap");
//        //        capture.OpenCapture();
//        //        openCap[4] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[4] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[4] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\500.pcap");
//        //        capture.OpenCapture();
//        //        openCap[4] = timer.Elapsed;
//        //        capture.TrackBidirectionalFlows();
//        //        btTrack[4] = timer.Elapsed;
//        //        capture.TrackConversations();
//        //        convTrack[4] = timer.Elapsed;
//        //        timer.Stop();
//        //        conversations = capture.GetConversations();
//        //        totalTime[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();

//        //        writer.WriteLine(totalTime[0] + "," + totalTime[1] + "," + totalTime[2] + "," + totalTime[3] + "," + totalTime[4]);
//        //        Console.WriteLine(totalTime[0] + "," + totalTime[1] + "," + totalTime[2] + "," + totalTime[3] + "," + totalTime[4]);
//        //    }
//        //    writer.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4]);
//        //    Console.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4]);
//        //    writer.Close();
//        //}

//        /// <summary> Tests conversation tracker generated 2.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public static void ConversationTrackerGenerated2Test()
//        {
//            var windowsIdentity = WindowsIdentity.GetCurrent();
//            if(windowsIdentity != null) {
//                if(windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal") { return; }
//            }
//            else
//            {
//                return;
//            }

//            var banchTest = new BanchmarkTester(@"..\..\..\TestingData\\banchmark\ConversationTrackerGenerated2Test");
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_12_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_25_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_50_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_75_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_100_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_150_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_2_200_cap);
//            banchTest.Print();
//        }

//        //[Test]
//        //[Category("LongRunning")]
//        //public static void ConversationTrackerGenerated2Test()
//        //{
//        //    var windowsIdentity = WindowsIdentity.GetCurrent();
//        //    if (windowsIdentity != null)
//        //    {
//        //        if (windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal")
//        //            return;
//        //    }
//        //    else
//        //    {
//        //        return;
//        //    }
//        //    var time = new TimeSpan[7];
//        //    var convs = new int[7];
//        //    var frames = new int[7];
//        //    var mem = new long[7];
//        //    var timer = new Stopwatch();
//        //    StreamWriter writer = new StreamWriter(@"..\..\..\TestingData\\banchmark\ConversationTrackerGenerated2Test.csv");
//        //    writer.WriteLine(@"12, 25, 50, 75, 100, 150, 200");
//        //    Console.WriteLine(@"12, 25, 50, 75, 100, 150, 200");
//        //    for (var i = 0; i < 1; i++)
//        //    {
//        //        timer.Reset();
//        //        timer.Start();
//        //        var capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\12.cap");
//        //        var conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[0] = timer.Elapsed;
//        //        convs[0] = conversations.Count();
//        //        frames[0] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[0] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\25.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[1] = timer.Elapsed;
//        //        convs[1] = conversations.Count();
//        //        frames[1] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[1] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\50.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[2] = timer.Elapsed;
//        //        convs[2] = conversations.Count();
//        //        frames[2] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[2] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\75.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[3] = timer.Elapsed;
//        //        convs[3] = conversations.Count();
//        //        frames[3] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[3] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\100.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();
//        //        frames[4] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[4] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\150.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[5] = timer.Elapsed;
//        //        convs[5] = conversations.Count();
//        //        frames[5] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[5] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\2\200.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[6] = timer.Elapsed;
//        //        convs[6] = conversations.Count();
//        //        frames[6] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[6] = GC.GetTotalMemory(true);

//        //        writer.WriteLine(time[0] + "," + time[1] + "," + time[2] + "," + time[3] + "," + time[4] + "," + time[5] + "," + time[6]);
//        //        Console.WriteLine(time[0] + "," + time[1] + "," + time[2] + "," + time[3] + "," + time[4] + "," + time[5] + "," + time[6]);
//        //    }
//        //    writer.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4] + "," + convs[5] + "," + convs[6]);
//        //    Console.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4] + "," + convs[5] + "," + convs[6]);

//        //    writer.WriteLine(frames[0] + "," + frames[1] + "," + frames[2] + "," + frames[3] + "," + frames[4] + "," + frames[5] + "," + frames[6]);
//        //    Console.WriteLine(frames[0] + "," + frames[1] + "," + frames[2] + "," + frames[3] + "," + frames[4] + "," + frames[5] + "," + frames[6]);

//        //    writer.WriteLine(mem[0] + "," + mem[1] + "," + mem[2] + "," + mem[3] + "," + mem[4] + "," + mem[5] + "," + mem[6]);
//        //    Console.WriteLine(mem[0] + "," + mem[1] + "," + mem[2] + "," + mem[3] + "," + mem[4] + "," + mem[5] + "," + mem[6]);
//        //    writer.Close();
//        //}
//        /// <summary> Tests conversation tracker generated 4.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public static void ConversationTrackerGenerated4Test()
//        {
//            var windowsIdentity = WindowsIdentity.GetCurrent();
//            if(windowsIdentity != null) {
//                if(windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal") { return; }
//            }
//            else
//            {
//                return;
//            }

//            var banchTest = new BanchmarkTester(@"..\..\..\TestingData\\banchmark\ConversationTrackerGenerated4Test");
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_12_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_25_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_50_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_75_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_100_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_150_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_4_200_cap);
//            banchTest.Print();
//        }

//        //[Test]
//        //[Category("LongRunning")]
//        //public static void ConversationTrackerGenerated4Test()
//        //{
//        //    var windowsIdentity = WindowsIdentity.GetCurrent();
//        //    if (windowsIdentity != null)
//        //    {
//        //        if (windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal")
//        //            return;
//        //    }
//        //    else
//        //    {
//        //        return;
//        //    }
//        //    var time = new TimeSpan[7];
//        //    var convs = new int[7];
//        //    var frames = new int[7];
//        //    var mem = new long[7];
//        //    var timer = new Stopwatch();
//        //    StreamWriter writer = new StreamWriter(@"..\..\..\TestingData\\banchmark\ConversationTrackerGenerated4Test.csv");
//        //    writer.WriteLine(@"12, 25, 50, 75, 100, 150, 200");
//        //    Console.WriteLine(@"12, 25, 50, 75, 100, 150, 200");
//        //    for (var i = 0; i < 1; i++)
//        //    {
//        //        timer.Reset();
//        //        timer.Start();
//        //        var capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\12.cap");
//        //        var conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[0] = timer.Elapsed;
//        //        convs[0] = conversations.Count();
//        //        frames[0] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[0] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\25.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[1] = timer.Elapsed;
//        //        convs[1] = conversations.Count();
//        //        frames[1] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[1] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\50.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[2] = timer.Elapsed;
//        //        convs[2] = conversations.Count();
//        //        frames[2] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[2] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\75.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[3] = timer.Elapsed;
//        //        convs[3] = conversations.Count();
//        //        frames[3] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[3] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\100.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();
//        //        frames[4] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[4] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\150.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[5] = timer.Elapsed;
//        //        convs[5] = conversations.Count();
//        //        frames[5] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[5] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\200.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[6] = timer.Elapsed;
//        //        convs[6] = conversations.Count();
//        //        frames[6] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[6] = GC.GetTotalMemory(true);
//        //        time[6] = timer.Elapsed;
//        //        convs[6] = conversations.Count();
//        //        frames[6] = conversations.Sum(conv => conv.Frames.Count());

//        //        writer.WriteLine(time[0] + "," + time[1] + "," + time[2] + "," + time[3] + "," + time[4] + "," + time[5] + "," + time[6]);
//        //        Console.WriteLine(time[0] + "," + time[1] + "," + time[2] + "," + time[3] + "," + time[4] + "," + time[5] + "," + time[6]);
//        //    }
//        //    writer.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4] + "," + convs[5] + "," + convs[6]);
//        //    Console.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4] + "," + convs[5] + "," + convs[6]);

//        //    writer.WriteLine(frames[0] + "," + frames[1] + "," + frames[2] + "," + frames[3] + "," + frames[4] + "," + frames[5] + "," + frames[6]);
//        //    Console.WriteLine(frames[0] + "," + frames[1] + "," + frames[2] + "," + frames[3] + "," + frames[4] + "," + frames[5] + "," + frames[6]);

//        //    writer.WriteLine(mem[0] + "," + mem[1] + "," + mem[2] + "," + mem[3] + "," + mem[4] + "," + mem[5] + "," + mem[6]);
//        //    Console.WriteLine(mem[0] + "," + mem[1] + "," + mem[2] + "," + mem[3] + "," + mem[4] + "," + mem[5] + "," + mem[6]);
//        //    writer.Close();
//        //}

//        /// <summary> Tests conversation tracker isa splited.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public static void ConversationTrackerISASplitedTest()
//        {
//            var windowsIdentity = WindowsIdentity.GetCurrent();
//            if(windowsIdentity != null) {
//                if(windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal") { return; }
//            }
//            else
//            {
//                return;
//            }

//            var banchTest = new BanchmarkTester(@"..\..\..\TestingData\\banchmark\ConversationTrackerISASplitedTest");
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_12_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_25_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_50_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_75_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_100_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_150_cap);
//            banchTest.BanchmarkTest(Pcaps.Default.benchmark_isa_200_cap);
//            banchTest.Print();
//        }

//        //[Test]
//        //[Category("LongRunning")]
//        //public static void ConversationTrackerISASplitedTest()
//        //{
//        //    var windowsIdentity = WindowsIdentity.GetCurrent();
//        //    if (windowsIdentity != null)
//        //    {
//        //        if (windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal")
//        //            return;
//        //    }
//        //    else
//        //    {
//        //        return;
//        //    }
//        //    var time = new TimeSpan[7];
//        //    var convs = new int[7];
//        //    var frames = new int[7];
//        //    var mem = new long[7];
//        //    var timer = new Stopwatch();
//        //    StreamWriter writer = new StreamWriter(@"..\..\..\TestingData\\banchmark\ConversationTrackerISASplitedTest.csv");
//        //    writer.WriteLine(@"12, 25, 50, 75, 100, 150, 200");
//        //    Console.WriteLine(@"12, 25, 50, 75, 100, 150, 200");
//        //    for (var i = 0; i < 10; i++)
//        //    {
//        //        timer.Reset();
//        //        timer.Start();
//        //        var capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\isa\12.cap");
//        //        var conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[0] = timer.Elapsed;
//        //        convs[0] = conversations.Count();
//        //        frames[0] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[0] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\isa\25.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[1] = timer.Elapsed;
//        //        convs[1] = conversations.Count();
//        //        frames[1] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[1] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\isa\50.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[2] = timer.Elapsed;
//        //        convs[2] = conversations.Count();
//        //        frames[2] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[2] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\isa\75.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[3] = timer.Elapsed;
//        //        convs[3] = conversations.Count();
//        //        frames[3] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[3] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\isa\100.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[4] = timer.Elapsed;
//        //        convs[4] = conversations.Count();
//        //        frames[4] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[4] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\4\150.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[5] = timer.Elapsed;
//        //        convs[5] = conversations.Count();
//        //        frames[5] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[5] = GC.GetTotalMemory(true);

//        //        timer.Reset();
//        //        timer.Start();
//        //        capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\isa\200.cap");
//        //        conversations = capture.GetConversations();
//        //        timer.Stop();
//        //        time[6] = timer.Elapsed;
//        //        convs[6] = conversations.Count();
//        //        frames[6] = conversations.Sum(conv => conv.Frames.Count());
//        //        mem[6] = GC.GetTotalMemory(true);
//        //        time[6] = timer.Elapsed;
//        //        convs[6] = conversations.Count();
//        //        frames[6] = conversations.Sum(conv => conv.Frames.Count());

//        //        writer.WriteLine(time[0] + "," + time[1] + "," + time[2] + "," + time[3] + "," + time[4] + "," + time[5] + "," + time[6]);
//        //        Console.WriteLine(time[0] + "," + time[1] + "," + time[2] + "," + time[3] + "," + time[4] + "," + time[5] + "," + time[6]);
//        //    }
//        //    writer.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4] + "," + convs[5] + "," + convs[6]);
//        //    Console.WriteLine(convs[0] + "," + convs[1] + "," + convs[2] + "," + convs[3] + "," + convs[4] + "," + convs[5] + "," + convs[6]);

//        //    writer.WriteLine(frames[0] + "," + frames[1] + "," + frames[2] + "," + frames[3] + "," + frames[4] + "," + frames[5] + "," + frames[6]);
//        //    Console.WriteLine(frames[0] + "," + frames[1] + "," + frames[2] + "," + frames[3] + "," + frames[4] + "," + frames[5] + "," + frames[6]);

//        //    writer.WriteLine(mem[0] + "," + mem[1] + "," + mem[2] + "," + mem[3] + "," + mem[4] + "," + mem[5] + "," + mem[6]);
//        //    Console.WriteLine(mem[0] + "," + mem[1] + "," + mem[2] + "," + mem[3] + "," + mem[4] + "," + mem[5] + "," + mem[6]);
//        //    writer.Close();
//        //}

//        /// <summary> Tests conversation tracker profile.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public static void ConversationTrackerProfileTest()
//        {
//            var windowsIdentity = WindowsIdentity.GetCurrent();
//            if(windowsIdentity != null) {
//                if(windowsIdentity.Name != "PLUSKAL-NTB\\jan.pluskal") { return; }
//            }
//            else
//            {
//                return;
//            }
//            var capture = new CaptureProcessor(Pcaps.Default.benchmark_100_pcap);
//            var conversations = capture.GetConversations();
//            //capture = new CaptureProcessor.CaptureProcessor(@"..\..\..\TestingData\\banchmark\500.pcap");
//            //conversations = capture.GetConversations();
//        }

//        /// <summary> Pv 4 TCP reassembling all in HTTP pcaps.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public void IPv4TCPReassemblingAllInHTTPPcaps()
//        {
//            TestStart();
//            Console.WriteLine("Test IPv4TCPReassemblingAllInHTTPPcaps");
//            var manager = new CaptureManager();
//            manager.AddCapture(Pcaps.Default.http_caps_http_pc1_conversation_cap);
//            manager.AddCapture(Pcaps.Default.http_caps_http_pc2_conversation_cap);
//            manager.AddCapture(Pcaps.Default.http_caps_http_pc3_conversation_cap);
//            manager.AddCapture(Pcaps.Default.http_caps_http_pc4_conversation_cap);
//            var conversations = manager.GetConversations();

//            var pdusLists = manager.GetL7PDUs();
//            var paylen = pdusLists.Sum(pdu => pdu.GetPDUByteArr().Length);

//            var virtFrames = manager.Captures.First().PmCaptureProcessor.GetVirtualFramesCount();
//            Console.WriteLine("Paylen {0}, virtFrames {1}", paylen, virtFrames);
//            TestStop();
//            Assert.IsTrue(paylen == 53249096 && virtFrames == 0);
//        }

//        /// <summary> Pv 4 TCP reassembling banchmark.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public void IPv4TCPReassemblingBanchmark()
//        {
//            TestStart();
//            Console.WriteLine("Test IPv4TCPReassemblingAllInHTTPPcaps");
//            var manager = new CaptureManager();
//            var stopWatch = Stopwatch.StartNew();

//            stopWatch.Start();
//            manager.AddCapture(Pcaps.Default.testing_set_50_pcap);
//            var conversations = manager.GetConversations().ToArray();
//            stopWatch.Stop();
//            var frames = (UInt64) conversations.Sum(conv => conv.Frames.Count());
//            Console.WriteLine("50.pcap conv track: {0}ms convs: {1}, frames: {2}", stopWatch.ElapsedMilliseconds, conversations.Count(), frames);

//            stopWatch.Restart();
//            var pdusLists = manager.GetL7PDUs();
//            stopWatch.Stop();
//            Console.WriteLine("50.pcap reassemble: {0}", stopWatch.ElapsedMilliseconds);

//            stopWatch.Restart();
//            var paylen = (UInt64) pdusLists.Sum(pdu => pdu.ExtractedBytes);
//            stopWatch.Stop();
//            Console.WriteLine("50.pcap pdu data array: {0}ms len: {1}", stopWatch.ElapsedMilliseconds, paylen);
//            manager.RemoveCapture(Pcaps.Default.testing_set_50_pcap);

//            stopWatch.Restart();
//            manager.AddCapture(Pcaps.Default.testing_set_100_pcapng);
//            conversations = manager.GetConversations().ToArray();
//            stopWatch.Stop();
//            frames = (UInt64) conversations.Sum(conv => conv.Frames.Count());
//            Console.WriteLine("100.pcap conv track: {0}ms convs: {1}, frames: {2}", stopWatch.ElapsedMilliseconds, conversations.Count(), frames);

//            stopWatch.Restart();
//            pdusLists = manager.GetL7PDUs();
//            stopWatch.Stop();
//            Console.WriteLine("100.pcap reassemble: {0}", stopWatch.ElapsedMilliseconds);

//            stopWatch.Restart();
//            paylen = (UInt64) pdusLists.Sum(pdu => pdu.ExtractedBytes);
//            stopWatch.Stop();
//            Console.WriteLine("100.pcap pdu data array: {0}ms len: {1}", stopWatch.ElapsedMilliseconds, paylen);
//            manager.RemoveCapture(Pcaps.Default.testing_set_100_pcapng);

//            stopWatch.Restart();
//            manager.AddCapture(Pcaps.Default.testing_set_150_pcapng);
//            conversations = manager.GetConversations().ToArray();
//            stopWatch.Stop();
//            frames = (UInt64) conversations.Sum(conv => conv.Frames.Count());
//            Console.WriteLine("150.pcap conv track: {0}ms convs: {1}, frames: {2}", stopWatch.ElapsedMilliseconds, conversations.Count(), frames);

//            stopWatch.Restart();
//            pdusLists = manager.GetL7PDUs();
//            stopWatch.Stop();
//            Console.WriteLine("150.pcap reassemble: {0}", stopWatch.ElapsedMilliseconds);

//            stopWatch.Restart();
//            paylen = (UInt64) pdusLists.Sum(pdu => pdu.ExtractedBytes);
//            stopWatch.Stop();
//            Console.WriteLine("150.pcap pdu data array: {0}ms len: {1}", stopWatch.ElapsedMilliseconds, paylen);
//            manager.RemoveCapture(Pcaps.Default.testing_set_150_pcapng);

//            stopWatch.Restart();
//            manager.AddCapture(Pcaps.Default.testing_set_220_pcap);
//            conversations = manager.GetConversations().ToArray();
//            stopWatch.Stop();
//            frames = (UInt64) conversations.Sum(conv => conv.Frames.Count());
//            Console.WriteLine("220.pcap conv track: {0}ms convs: {1}, frames: {2}", stopWatch.ElapsedMilliseconds, conversations.Count(), frames);

//            stopWatch.Restart();
//            pdusLists = manager.GetL7PDUs();
//            stopWatch.Stop();
//            Console.WriteLine("220.pcap reassemble: {0}", stopWatch.ElapsedMilliseconds);

//            stopWatch.Restart();
//            paylen = (UInt64) pdusLists.Sum(pdu => pdu.ExtractedBytes);
//            stopWatch.Stop();
//            Console.WriteLine("220.pcap pdu data array: {0}ms len: {1}", stopWatch.ElapsedMilliseconds, paylen);
//            manager.RemoveCapture(Pcaps.Default.testing_set_220_pcap);

//            TestStop();
//        }

//        /// <summary> Pv 4 TCP reassembling isa 800.</summary>
//        [Test]
//        [Category("LongRunning")]
//        public void IPv4TCPReassemblingISA800()
//        {
//            TestStart();
//            Console.WriteLine("Test IPv4TCPReassemblingISA800");
//            var manager = new CaptureManager();
//            manager.AddCapture(Pcaps.Default.pcap_mix_isa_http_pcap);
//            var conversations = manager.GetConversations();

//            var pdusLists = manager.GetL7PDUs();
//            //var paylen = pdusLists.Sum(pdu => pdu.GetPDUByteArr().Length);
//            Console.WriteLine("conversations {0}", conversations.Count());
//            Console.WriteLine("PDUs {0}", pdusLists.Count());
//            //var virtFrames = manager.Captures.First().CaptureProcessor.GetVirtualFramesCount();
//            //System.Console.WriteLine("Paylen {0}, virtFrames {1}", paylen, virtFrames);
//            //Assert.IsTrue(paylen == 53570938 && virtFrames == 24);
//            TestStop();
//        }

//        /// <summary> Tests start.</summary>
//        private static void TestStart()
//        {
//            _testStart = DateTime.Now;
//        }

//        /// <summary> Tests stop.</summary>
//        private static void TestStop()
//        {
//            var testStop = DateTime.Now;
//            var duration = testStop - _testStart;
//            Console.WriteLine("Timeduration: " + duration.Hours + ":" + duration.Minutes + ":" + duration.Seconds + "." + duration.Milliseconds);
//        }

//        /// <summary> A banchmark tester.</summary>
//        private class BanchmarkTester
//        {
//            /// <summary> List of benchmarks.</summary>
//            private readonly List<Benchmark> _benchmarkList = new List<Benchmark>();

//            /// <summary> The tests.</summary>
//            private readonly int _tests;

//            /// <summary> The output.</summary>
//            private string _output;

//            /// <summary> Constructor.</summary>
//            /// <param name="output"> The output. </param>
//            /// <param name="tests">  (Optional) the tests. </param>
//            public BanchmarkTester(string output, int tests = 10)
//            {
//                this._output = output;
//                this._tests = tests;
//            }

//            /// <summary> Tests banchmark.</summary>
//            /// <param name="pcapFilePath"> Full pathname of the pcap file. </param>
//            public void BanchmarkTest(string pcapFilePath) => this._benchmarkList.Add(new Benchmark(pcapFilePath, this._tests));

//            /// <summary> Prints the given stream.</summary>
//            public void Print()
//            {
//                this.Print(Console.Out);
//                var path = Path.Combine(Path.GetDirectoryName(this._output), Path.GetFileNameWithoutExtension(this._output) + ".csv");

//                using(var writer = new StreamWriter(path)) { this.Print(writer); }
//            }

//            /// <summary> Prints the given stream.</summary>
//            /// <param name="stream"> The stream. </param>
//            private void Print(TextWriter stream)
//            {
//                stream.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", "FileName", "FileSize", "averageOpenCap", "averagebtTrack", "averageconvTrack",
//                    "averagetotalTime", "averagemem", "minOpenCap", "minbtTrack", "minconvTrack", "mintotalTime", "minmem", "frames", "conversations", "openAndIndexing");

//                foreach(var benchmark in this._benchmarkList)
//                {
//                    stream.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", Path.GetFileNameWithoutExtension(benchmark.PcapFilePath),
//                        this.PrepareCaptureForProcessing(benchmark.PcapFilePath).Length/1000000, benchmark.AverageOpenCap.TotalSeconds, benchmark.AveragebtTrack.TotalSeconds,
//                        benchmark.AverageconvTrack.TotalSeconds, benchmark.AveragetotalTime.TotalSeconds, (double) benchmark.Averagemem/1000000, benchmark.MinOpenCap.TotalSeconds,
//                        benchmark.MinbtTrack.TotalSeconds, benchmark.MinconvTrack.TotalSeconds, benchmark.MintotalTime.TotalSeconds, (double) benchmark.Minmem/1000000,
//                        benchmark.Frames[0], benchmark.Convs[0], benchmark.openCap[0].TotalSeconds);
//                }
//            }

//            /// <summary> A benchmark.</summary>
//            private class Benchmark
//            {
//                /// <summary> The averagemem.</summary>
//                public readonly long Averagemem;

//                /// <summary> The bt track.</summary>
//                public readonly TimeSpan[] BtTrack;

//                /// <summary> The convs.</summary>
//                public readonly int[] Convs;

//                /// <summary> The convert track.</summary>
//                public readonly TimeSpan[] ConvTrack;

//                /// <summary> The frames.</summary>
//                public readonly int[] Frames;

//                /// <summary> The memory.</summary>
//                public readonly long[] Mem;

//                /// <summary> The minmem.</summary>
//                public readonly long Minmem;

//                /// <summary> The open capability.</summary>
//                public readonly TimeSpan[] openCap;

//                /// <summary> Full pathname of the pcap file.</summary>
//                public readonly string PcapFilePath;

//                /// <summary> The tests.</summary>
//                public readonly int Tests;

//                /// <summary> The total time.</summary>
//                public readonly TimeSpan[] TotalTime;

//                /// <summary> The timer.</summary>
//                private readonly Stopwatch Timer;

//                /// <summary> The timer total.</summary>
//                private readonly Stopwatch TimerTotal;

//                /// <summary> The averagebt track.</summary>
//                public TimeSpan AveragebtTrack;

//                /// <summary> The averageconv track.</summary>
//                public TimeSpan AverageconvTrack;

//                /// <summary> The average open capability.</summary>
//                public TimeSpan AverageOpenCap;

//                /// <summary> The averagetotal time.</summary>
//                public TimeSpan AveragetotalTime;

//                /// <summary> The minbt track.</summary>
//                public TimeSpan MinbtTrack;

//                /// <summary> The minconv track.</summary>
//                public TimeSpan MinconvTrack;

//                /// <summary> The minimum open capability.</summary>
//                public TimeSpan MinOpenCap;

//                /// <summary> The mintotal time.</summary>
//                public TimeSpan MintotalTime;

//                /// <summary> Constructor.</summary>
//                /// <exception cref="ArgumentOutOfRangeException">
//                ///     Thrown when one or more arguments are outside
//                ///     the required range.
//                /// </exception>
//                /// <param name="pcapFilePath"> Full pathname of the pcap file. </param>
//                /// <param name="tests">        (Optional) the tests. </param>
//                public Benchmark(string pcapFilePath, int tests = 10)
//                {
//                    if(tests <= 1) { throw new ArgumentOutOfRangeException("tests argument must be greater then 1"); }

//                    this.PcapFilePath = pcapFilePath;
//                    this.Tests = tests;

//                    this.TotalTime = new TimeSpan[tests];
//                    this.openCap = new TimeSpan[tests];
//                    this.BtTrack = new TimeSpan[tests];
//                    this.ConvTrack = new TimeSpan[tests];
//                    this.Convs = new int[tests];
//                    this.Frames = new int[tests];
//                    this.TimerTotal = new Stopwatch();
//                    this.Timer = new Stopwatch();
//                    this.Mem = new long[tests];
//                    this.Test();

//                    this.AverageOpenCap = new TimeSpan(Convert.ToInt64(this.openCap.Skip(1).Average(timeSpan => timeSpan.Ticks)));
//                    this.AveragebtTrack = new TimeSpan(Convert.ToInt64(this.BtTrack.Skip(1).Average(timeSpan => timeSpan.Ticks)));
//                    this.AverageconvTrack = new TimeSpan(Convert.ToInt64(this.ConvTrack.Skip(1).Average(timeSpan => timeSpan.Ticks)));
//                    this.AveragetotalTime = new TimeSpan(Convert.ToInt64(this.TotalTime.Skip(1).Average(timeSpan => timeSpan.Ticks)));
//                    this.Averagemem = Convert.ToInt64(this.Mem.Skip(1).Average(m => m));

//                    this.MinOpenCap = new TimeSpan(Convert.ToInt64(this.openCap.Skip(1).Min(timeSpan => timeSpan.Ticks)));
//                    this.MinbtTrack = new TimeSpan(Convert.ToInt64(this.BtTrack.Skip(1).Min(timeSpan => timeSpan.Ticks)));
//                    this.MinconvTrack = new TimeSpan(Convert.ToInt64(this.ConvTrack.Skip(1).Min(timeSpan => timeSpan.Ticks)));
//                    this.MintotalTime = new TimeSpan(Convert.ToInt64(this.TotalTime.Skip(1).Min(timeSpan => timeSpan.Ticks)));
//                    this.Minmem = Convert.ToInt64(this.Mem.Skip(1).Min(m => m));
//                }

//                /// <summary> Tests this object.</summary>
//                private void Test()
//                {
//                    try
//                    {
//                        File.Delete(Path.Combine(Path.GetDirectoryName(this.PcapFilePath), Path.GetFileNameWithoutExtension(this.PcapFilePath) + ".index"));
//                        File.Delete(Path.Combine(Path.GetDirectoryName(this.PcapFilePath), Path.GetFileNameWithoutExtension(this.PcapFilePath)));
//                    }
//                    catch(Exception ex) {
//                        Console.WriteLine(ex.Message);
//                    }
//                    for(var i = 0; i < this.Tests; i++)
//                    {
//                        this.TimerTotal.Reset();
//                        this.Timer.Reset();
//                        GC.Collect();
//                        GC.WaitForPendingFinalizers();
//                        GC.Collect();
//                        this.TimerTotal.Start();
//                        this.Timer.Start();
//                        var capture = new CaptureProcessor(this.PcapFilePath);
//                        capture.OpenCapture();
//                        this.openCap[i] = this.Timer.Elapsed;
//                        this.Timer.Restart();
//                        capture.TrackL4Flows();
//                        this.BtTrack[i] = this.Timer.Elapsed;
//                        this.Timer.Restart();
//                        capture.TrackConversations();
//                        this.ConvTrack[i] = this.Timer.Elapsed;
//                        this.TimerTotal.Stop();
//                        this.Timer.Stop();
//                        this.Mem[i] = Process.GetCurrentProcess().WorkingSet64;
//                        var conversations = capture.GetConversations();
//                        this.TotalTime[i] = this.TimerTotal.Elapsed;
//                        var csConversationValues = conversations as CsConversationValue[] ?? conversations.ToArray();
//                        this.Convs[i] = csConversationValues.Count();
//                        this.Frames[i] = csConversationValues.Sum(conv => conv.Frames.Count());
//                    }
//                }
//            }
//        }
//    }
//}
namespace Netfox.Framework.Tests
{
}