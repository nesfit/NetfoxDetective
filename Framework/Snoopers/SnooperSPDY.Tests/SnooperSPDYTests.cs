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

using System.IO;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.SnooperSPDY.Tests
{
    public class SnooperSPDYTests : SnooperBaseTests
    {

        [Test][Explicit][Category("Explicit")]
        public void Test_SPDY_test()
        {
            var captureFile = this.PrepareCaptureForProcessing(SnoopersPcaps.Default.twitter_twitter1_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PrivateKeys.Default.fb_pk);
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
        }

        /*
        [Test]
        public void TestPduProvider()
        {
            var captureFile = this.PrepareCaptureForProcessing(@"C:\Users\xletav00\Desktop\pcaps\pduTest_clear.pcapng");
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var conversation = conversations.ElementAtOrDefault(0);
            Assert.IsNotNull(conversation);

            var stream = new PDUStreamBasedProvider(conversation, EFcPDUProviderType.ContinueInterlay);
            var reader = new PDUStreamReader(stream, Encoding.ASCII, true);

            this.TestReader(reader);
        }

        [Test]
        public void TestPduDecrypterProvider()
        {
            var captureFile = this.PrepareCaptureForProcessing(@"C:\Users\xletav00\Desktop\pcaps\pduTest_ssl.pcapng");
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var conversation = conversations.ElementAtOrDefault(0);
            Assert.IsNotNull(conversation);
            conversation.Key = new CypherKey { ServerPrivateKey = File.ReadAllText(PrivateKeys.Default.fb_pk) };

            var stream = new PDUDecrypterBase(conversation, EFcPDUProviderType.ContinueInterlay);
            var reader = new PDUStreamReader(stream, Encoding.ASCII, true);

            this.TestReader(reader);
        }

        public void TestReader(PDUStreamReader reader)
        {
            Byte[] msg;
            Boolean nextMsg;

            msg = this.ReadMsg(reader, 20);
            nextMsg = reader.NewMessage();

            msg = this.ReadMsg(reader, 16);
            nextMsg = reader.NewMessage();

            msg = this.ReadMsg(reader, 12);
            nextMsg = reader.NewMessage();

            msg = this.ReadMsg(reader, 942);
            nextMsg = reader.NewMessage();

            msg = this.ReadMsg(reader, 20);
            nextMsg = reader.NewMessage();

            msg = this.ReadMsg(reader, 962);
            nextMsg = reader.NewMessage();

            msg = this.ReadMsg(reader, 126);
            nextMsg = reader.NewMessage();
        }

        public Byte[] ReadMsg(PDUStreamReader reader, int len)
        {
            var buffer = new Byte[len];
            var readBytes = reader.Read(buffer, 0, len);
            Assert.AreEqual(len, readBytes);
            return buffer;
        }
        */
    }
}