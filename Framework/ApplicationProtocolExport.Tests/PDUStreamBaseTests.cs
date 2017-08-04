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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Enums;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.Framework.ApplicationProtocolExport.Tests
{
    /// <summary> A PDU stream base tests.</summary>
    [TestFixture]
    public class PDUStreamBaseTests : FrameworkBaseTests
    {
        /// <summary> The conversations.</summary>
        private IEnumerable<L7Conversation> _conversations;

        /// <summary> The stream.</summary>
        private PDUStreamBasedProvider _stream;

        ///// <summary> Tests breaked read.</summary>
        //[Test()]
        //public void ReadEmptyConversationBreakedTest()
        //{
        //    stream = new PDUStreamBasedProvider(_conversations.ToArray()[1], EFcPDUProviderType.Breaked);
        //    var cost = new byte[4];
        //    var arr = stream.Read(cost, 0, 2);
        //    Assert.IsTrue(arr == 0 && stream.Position == 0);
        //}  /// <summary> Tests breaked read.</summary>
        //[Test()]
        //public void ReadEmptyConversationContinueInterlayTest()
        //{
        //    stream = new PDUStreamBasedProvider(_conversations.ToArray()[1], EFcPDUProviderType.ContinueInterlay);
        //    var cost = new byte[4];
        //    var arr = stream.Read(cost, 0, 2);
        //    Assert.IsTrue(arr == 0 && stream.Position == 0);
        //}  /// <summary> Tests breaked read.</summary>
        //[Test()]
        //public void ReadEmptyConversationMixedTest()
        //{
        //    stream = new PDUStreamBasedProvider(_conversations.ToArray()[1], EFcPDUProviderType.Mixed);
        //    var cost = new byte[4];
        //    var arr = stream.Read(cost, 0, 2);
        //    Assert.IsTrue(arr == 0 && stream.Position == 0);
        //}

        [Test]
        public void BinReaderBreakedTest()
        {
            this._stream = new PDUStreamBasedProvider(this._conversations.ToArray()[2], EfcPDUProviderType.Breaked);
            var binaryReader = new BinaryReader(this._stream, Encoding.ASCII);
            var endOfFileFound = false;
            do
            {
                if(binaryReader.PeekChar() < 0) {
                    endOfFileFound = true;
                }
                else
                {
                    int c = binaryReader.ReadByte();
                }
            } while(!endOfFileFound);
        }

        [Test]
        public void BinReaderContinueInterlayTest()
        {
            this._stream = new PDUStreamBasedProvider(this._conversations.ToArray()[2], EfcPDUProviderType.ContinueInterlay);
            var binaryReader = new BinaryReader(this._stream, Encoding.ASCII);
            var endOfFileFound = false;
            do
            {
                var c = binaryReader.PeekChar();
                if(c < 0) {
                    endOfFileFound = true;
                }
                else
                {
                    // stream.Seek(221, SeekOrigin.Begin);
                    c = binaryReader.ReadByte();
                }
            } while(!endOfFileFound);
        }

        [Test]
        public void BinReaderMixedTest()
        {
            this._stream = new PDUStreamBasedProvider(this._conversations.ToArray()[2], EfcPDUProviderType.Mixed);
            var binaryReader = new BinaryReader(this._stream, Encoding.ASCII);
            var endOfFileFound = false;
            do
            {
                if(binaryReader.PeekChar() < 0) {
                    endOfFileFound = true;
                }
                else
                {
                    int c = binaryReader.ReadByte();
                }
            } while(!endOfFileFound);
        }

        /// <summary> Tests breaked read.</summary>
        [Test]
        public void BreakedReadTest()
        {
            this._stream = new PDUStreamBasedProvider(this._conversations.First(conv => conv.L7PDUs.Count() == this._conversations.Max(c => c.L7PDUs.Count())),
                EfcPDUProviderType.Breaked);
            var cost = new byte[4];
            var big = new byte[800];
            var two = this._stream.Read(cost, 0, 2);
            Assert.IsTrue(two == 2 && this._stream.Position == 2);
            var two1 = this._stream.Read(cost, 2, 2);
            Assert.IsTrue(two1 == 2 && this._stream.Position == 4);
            var vxx = this._stream.Read(big, 0, 800);
            Assert.IsTrue(vxx == 6 && this._stream.Position == 10);
            vxx = this._stream.Read(big, 0, 800);
            Assert.IsTrue(vxx == 0 && this._stream.Position == 10);

            var boollen = this._stream.NewMessage();
            Assert.IsTrue(boollen && this._stream.Position == 0);

            two = this._stream.Read(cost, 0, 2);
            Assert.IsTrue(two == 2 && this._stream.Position == 2);
            two1 = this._stream.Read(cost, 2, 2);
            Assert.IsTrue(two1 == 2 && this._stream.Position == 4);
            vxx = this._stream.Read(big, 0, 800);
            Assert.IsTrue(vxx == 468 && this._stream.Position == 472);
            vxx = this._stream.Read(big, 0, 800);
            Assert.IsTrue(vxx == 0 && this._stream.Position == 472);

            boollen = this._stream.NewMessage();
            Assert.IsTrue(boollen && this._stream.Position == 0);

            vxx = this._stream.Read(big, 0, 800);
            Assert.IsTrue(vxx == 461 && this._stream.Position == 461);
            var v0 = this._stream.Read(big, 0, 800);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 461);
        }

        /// <summary> Tests continue interlay read.</summary>
        [Test]
        public void ContinueInterlayReadTest()
        {
            this._stream = new PDUStreamBasedProvider(this._conversations.ToArray()[1], EfcPDUProviderType.ContinueInterlay);
            var cost = new byte[4];
            var big = new byte[800];
            var two = this._stream.Read(cost, 0, 2);
            Assert.IsTrue(two == 2 && this._stream.Position == 2);
            var two1 = this._stream.Read(cost, 2, 2);
            Assert.IsTrue(two1 == 2 && this._stream.Position == 4);
            var v800 = this._stream.Read(big, 0, 800);
            Assert.IsTrue(v800 == 800 && this._stream.Position == 804);
            big = new byte[800];
            v800 = this._stream.Read(big, 0, 800);
            Assert.IsTrue(v800 == 800 && this._stream.Position == 1604);
            v800 = this._stream.Read(big, 0, 800);
            Assert.IsTrue(v800 == 800 && this._stream.Position == 2404);

            var v0 = this._stream.Seek(0, SeekOrigin.Begin);
            big = new byte[30000];
            var vxx = this._stream.Read(big, 0, 22911); // to end of stream
            Assert.IsTrue(v0 == 0 && vxx == 22911 && this._stream.Position == 22911);

            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 22911);
            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 22911);

            var boollen = this._stream.NewMessage();
            Assert.IsTrue(boollen && this._stream.Position == 0);
            vxx = this._stream.Read(big, 0, 4005); // to end of stream
            Assert.IsTrue(v0 == 0 && vxx == 4005 && this._stream.Position == 4005);
            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 4005);
            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 4005);

            boollen = this._stream.NewMessage();
            Assert.IsTrue(!boollen && this._stream.Position == 0);

            //after total end of stream
            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);
        }

        //[Test]
        //public void HTTPweirdEvent2Test()
        //{
        //    var capture = new CaptureProcessor(Pcaps.Default.pcap_mix_mixed_pdu_pcapng);
        //    this._conversations = capture.GetConversations(); //"HTTP"

        //    // Assert.IsTrue(_conversations.Count() == 297);

        //    this.stream = new PDUStreamBasedProvider(this._conversations.ToArray()[0], EFcPDUProviderType.Breaked);
        //    var binaryReader = new BinaryReader(this.stream, Encoding.ASCII);

        //    var arr = new Byte[999999];

        //    var counter = 0;
        //    do
        //    {
        //        if(counter%2 == 0) {
        //            Console.WriteLine("####0000000000000000000000###");
        //        }
        //        else
        //        {
        //            Console.WriteLine("####1111111111111111111111###");
        //        }
        //        var reded = binaryReader.Read(arr, 0, 999999);
        //        Console.BackgroundColor = ConsoleColor.Blue;
        //        Console.WriteLine(Encoding.ASCII.GetString(arr));
        //        Console.WriteLine();
        //        counter++;
        //        arr = new Byte[999999];
        //    } while(this.stream.NewMessage());
        //}

        //[Test]
        //public void HTTPweirdEventTest()
        //{
        //    var capture = new CaptureProcessor(Pcaps.Default.pcap_mix_centrum2_pcapng);
        //    this._conversations = capture.GetConversations(); //"HTTP"

        //    // Assert.IsTrue(_conversations.Count() == 297);

        //    this.stream = new PDUStreamBasedProvider(this._conversations.ToArray()[63], EFcPDUProviderType.Breaked);
        //    var binaryReader = new BinaryReader(this.stream, Encoding.ASCII);

        //    var arr = new Byte[999999];

        //    var counter = 0;
        //    do
        //    {
        //        if(counter%2 == 0) {
        //            Console.WriteLine("####0000000000000000000000###");
        //        }
        //        else
        //        {
        //            Console.WriteLine("####1111111111111111111111###");
        //        }
        //        binaryReader.Read(arr, 0, 999999);
        //        Console.BackgroundColor = ConsoleColor.Blue;
        //        Console.WriteLine(Encoding.ASCII.GetString(arr));
        //        Console.WriteLine();
        //        counter++;
        //    } while(this.stream.NewMessage());
        //}

        /// <summary> Tests mixed read.</summary>
        [Test]
        public void MixedReadTest()
        {
            this._stream = new PDUStreamBasedProvider(this._conversations.First(), EfcPDUProviderType.Mixed);
            var cost = new byte[4];
            var cost1 = new byte[4];
            var big = new byte[2000];
            var two = this._stream.Read(cost, 0, 2);
            Assert.IsTrue(two == 2 && this._stream.Position == 2);
            var two1 = this._stream.Read(cost, 2, 2);
            Assert.IsTrue(two1 == 2 && this._stream.Position == 4);
            var v800 = this._stream.Read(big, 0, 800);
            Assert.IsTrue(v800 == 800 && this._stream.Position == 804);
            big = new byte[2000];
            var vxx = this._stream.Read(big, 0, 800);
            Assert.IsTrue(vxx == 595 && this._stream.Position == 1399);
            long v0 = this._stream.Read(big, 0, 800);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 1399);
            v0 = this._stream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);
            var v10 = this._stream.Seek(10, SeekOrigin.Begin);
            Assert.IsTrue(v10 == 10 && this._stream.Position == 10);
            v0 = this._stream.Seek(-10, SeekOrigin.Current);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);
            v10 = this._stream.Seek(10, SeekOrigin.Current);
            Assert.IsTrue(v10 == 10 && this._stream.Position == 10);
            var v20 = this._stream.Seek(10, SeekOrigin.Current);
            Assert.IsTrue(v20 == 20 && this._stream.Position == 20);
            v10 = this._stream.Seek(-10, SeekOrigin.Current);
            Assert.IsTrue(v10 == 10 && this._stream.Position == 10);
            v0 = this._stream.Seek(-10, SeekOrigin.Current);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);
            v0 = this._stream.Seek(-10, SeekOrigin.Current);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);

            var v2 = this._stream.Seek(2, SeekOrigin.Begin);
            two = this._stream.Read(cost, 0, 2);
            v0 = this._stream.Seek(-4, SeekOrigin.Current);
            Assert.IsTrue(v2 == 2 && this._stream.Position == 0 && v0 == 0 && two == 2);
            two = this._stream.Peek(cost1, 0, 2, 2, SeekOrigin.Current);
            Assert.IsTrue(two == 2 && cost[0] == cost1[0] && cost[1] == cost1[1] && this._stream.Position == 0);

            var v700 = this._stream.Seek(700, SeekOrigin.Begin);
            two = this._stream.Read(cost, 0, 2);
            v0 = this._stream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(v700 == 700 && this._stream.Position == 0 && v0 == 0 && two == 2);
            two = this._stream.Peek(cost1, 0, 700, 2, SeekOrigin.Current);
            Assert.IsTrue(two == 2 && cost[0] == cost1[0] && cost[1] == cost1[1] && this._stream.Position == 0);

            v0 = this._stream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);
            two = this._stream.Read(cost, 0, 2);
            Assert.IsTrue(two == 2 && this._stream.Position == 2);

            v0 = this._stream.Seek(0, SeekOrigin.Begin);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 0);
            vxx = this._stream.Read(big, 0, 1399); // to end of stream
            Assert.IsTrue(vxx == 1399 && this._stream.Position == 1399);
            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 1399);
            v0 = this._stream.Read(big, 0, 10);
            Assert.IsTrue(v0 == 0 && this._stream.Position == 1399);

            var boollen = this._stream.NewMessage();
            Assert.IsTrue(!boollen && this._stream.Position == 0);
            two = this._stream.Read(cost, 0, 2);
            Assert.IsTrue(two == 0 && this._stream.Position == 0);
        }

        /// <summary> Sets the up.</summary>
        [SetUp]
        public void SetUp()
        {
            base.SetUpInMemory();
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.pcap_mix_icq1_cap);
            this.FrameworkController.ProcessCapture(captureFile);
            this._conversations = this.L7Conversations.Where(c => c.ApplicationTags.Contains("aol-messenger")).ToArray();
            Assert.IsTrue(this._conversations.Count() == 4);
        }

        /// <summary> Tera down.</summary>
        [TearDown]
        public void TeraDown()
        {
            this._stream = null;
        }

        [Test]
        public void TestStreamReader()
        {
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.sip_caps_sip_rtcp_small_pcap);
            this.FrameworkController.ProcessCapture(captureFile);
            this._conversations = this.L7Conversations.Where(c => c.ApplicationTags.Contains("SIP")).ToArray();
            var counter = 0;
            string line;
            foreach(var conversation in this._conversations)
            {
                var stream = new PDUStreamBasedProvider(conversation, EfcPDUProviderType.SingleMessage);
                var reader = new PDUStreamReader(stream, Encoding.ASCII);
                do
                {
                    counter++;
                    Console.WriteLine(counter + ". message ######### ");
                    do
                    {
                        line = reader.ReadLine();
                        Console.WriteLine(line);
                    } while(line != null);
                } while(reader.NewMessage());
            }
        }

        [Test]
        public void TestStreamReaderMore()
        {
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.sip_caps_sip_rtcp_pcap);
            this.FrameworkController.ProcessCapture(captureFile);
            this._conversations = this.L7Conversations.Where(c => c.ApplicationTags.Contains("SIP")).ToArray();
            var counter = 0;
            string line;
            foreach(var conversation in this._conversations)
            {
                var stream = new PDUStreamBasedProvider(conversation, EfcPDUProviderType.SingleMessage);
                var reader = new PDUStreamReader(stream, Encoding.ASCII);
                do
                {
                    counter++;
                    Console.WriteLine(counter + ". message ######### ");
                    do
                    {
                        line = reader.ReadLine();
                        Console.WriteLine(line);
                    } while(line != null);
                } while(reader.NewMessage());
            }
        }

        #region hide
        /// <summary> Tests PDU stream base.</summary>
        [Test]
        [Ignore("Not Implemented")]
        public void PDUStreamBaseTest() {}

        /// <summary> Flushes the test.</summary>
        [Test]
        [Ignore("Not Implemented")]
        public void FlushTest() {}

        /// <summary> Tests set length.</summary>
        [Test]
        [Ignore("Not Implemented")]
        public void SetLengthTest() {}

        /// <summary> Writes the test.</summary>
        [Test]
        [Ignore("Not Implemented")]
        public void WriteTest() {}

        /// <summary> Tests seek.</summary>
        [Test]
        [Ignore("Not Implemented")]
        public void SeekTest() {}
        #endregion
    }
}