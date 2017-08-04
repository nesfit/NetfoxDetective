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
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.Framework.ApplicationProtocolExport.Tests
{
    /// <summary> Decrypter tests</summary>
    [TestFixture]
    public class PDUDecrypterBaseTests : SnooperBaseTests
    {
        [Test]
        [Ignore("Not imeplemtented")]
        public void TestAesDataDecrypter() { }

        /// <summary>
        ///     Test for TLS 1.2 decryption. Check one of decrypted packets in the middle of conversation.
        ///     Also check that read, seek and peek are functional.
        /// </summary>
        [Test]
        public void TestDecrypterCbc_first_message()
        {
            /* TLS 1.2 CBC conversation */
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.webmail_webmail_gmail_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);
            var conversations = this.L7Conversations.ToArray();
            var decryptersCbcTls12 = new List<PDUDecrypterBase>();
            this.AddCypherKeyToConversations(conversations, decryptersCbcTls12);

            foreach(var d in decryptersCbcTls12)
            {
                DumpAllMessages(d);

                d.Reset();
                d.Seek(0, SeekOrigin.Begin);
                var testString = "GET /mail/gxlu?email=test.netfox%40gmail.com&zx=1419951548565 HTTP/1.1";
                var testStringLen = testString.Length;
                var dt = new byte[testStringLen];
                var l = d.Peek(dt, 0, 0, testStringLen, SeekOrigin.Begin);
                Assert.AreEqual(l, testStringLen);
                var decrypted = Encoding.ASCII.GetString(dt, 0, l);
                Assert.AreEqual(testString, decrypted);
            }
        }

        /// <summary>
        ///     Test for TLS 1.2 decryption. Check one of decrypted packets in the middle of conversation.
        ///     Also check that read, seek and peek are functional.
        /// </summary>
        [Test]
        public void TestDecrypterCbc_second_message()
        {
            /* TLS 1.2 CBC conversation */
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.webmail_webmail_gmail_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);
            var conversations = this.L7Conversations.ToArray();
            var decryptersCbcTls12 = new List<PDUDecrypterBase>();
            this.AddCypherKeyToConversations(conversations, decryptersCbcTls12);

            foreach (var d in decryptersCbcTls12)
            {
                DumpAllMessages(d);

                d.Reset();
                d.Seek(0, SeekOrigin.Begin);
                d.NewMessage();
                var testString = "HTTP/1.1 204 No Content";
                var testStringLen = testString.Length;
                var dt = new byte[testStringLen];
                var l = d.Peek(dt, 0, 0, testStringLen, SeekOrigin.Begin);
                Assert.AreEqual(l, testStringLen);
                var decrypted = Encoding.ASCII.GetString(dt, 0, l);
                Assert.AreEqual(testString, decrypted);
            }
        }

        /// <summary>
        ///     Verify that GCM block cipher mode is functional.
        ///     TODO Bouncy Castle implementation of GCM works with MAC length up to 128b only
        /// </summary>
        [Test]
        public void TestDecrypterGcm()
        {
            /* TLS 1.2 gcm conversation */
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.webmail_webmail_live_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);
            var conversations = this.L7Conversations.ToArray();
            var decryptersGcmTls12 = new List<PDUDecrypterBase>();
            this.AddCypherKeyToConversations(conversations, decryptersGcmTls12);

            foreach(var d in decryptersGcmTls12) Assert.Throws(typeof(ArgumentException),
                () => DumpAllMessages(d));
        }

        [Test]
        public void TestDecrypterRc4_first_message()
        {
            /* TLS 1.2 stream conversation */
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.webmail_webmail_yahoo_rc4_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);
            var conversations = this.L7Conversations.ToArray();
            var decryptersStreamTls12 = new List<PDUDecrypterBase>();
            this.AddCypherKeyToConversations(conversations, decryptersStreamTls12);

            foreach(var d in decryptersStreamTls12)
            {
                DumpAllMessages(d);

                d.Reset();
                d.Seek(0, SeekOrigin.Begin);
                var test = "GET /neo/launch?.rand=27cq8enu8o8ae";
                var dt = new byte[35];
                var l = d.Peek(dt, 0, 0, 35, SeekOrigin.Begin);
                Assert.AreEqual(l, 35);
                var decrypted = Encoding.ASCII.GetString(dt, 0, l);
                Assert.AreEqual(test, decrypted);
            }
        }

        [Test]
        public void TestDecrypterRc4_second_message()
        {
            /* TLS 1.2 stream conversation */
            var captureFile = this.PrepareCaptureForProcessing(Pcaps.Default.webmail_webmail_yahoo_rc4_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);
            var conversations = this.L7Conversations.ToArray();
            var decryptersStreamTls12 = new List<PDUDecrypterBase>();
            this.AddCypherKeyToConversations(conversations, decryptersStreamTls12);

            foreach(var d in decryptersStreamTls12)
            {
                DumpAllMessages(d);

                d.Reset();
                d.Seek(0, SeekOrigin.Begin);

                d.NewMessage();
                var test = "HTTP/1.1 200 OK";
                var dt = new byte[15];
                var l = d.Peek(dt, 0, 0, 15, SeekOrigin.Begin);
                Assert.AreEqual(l, 15);
                var decrypted = Encoding.ASCII.GetString(dt, 0, l);
                Assert.AreEqual(test, decrypted);
            }
        }

        [Test]
        [Ignore("Not imeplemtented")]
        public void TestRsaKeyDecrypter() { }

        private void AddCypherKeyToConversations(L7Conversation[] conversations, List<PDUDecrypterBase> decryptersStreamTls12)
        {
            this.SetPrivateKeyToConversations(conversations, PrivateKeys.Default.fw_pk_pem);
            foreach(var conversation in conversations)
            {
                decryptersStreamTls12.Add(new PDUDecrypterBase(conversation, EfcPDUProviderType.Mixed));
            }
        }

        private static void DumpAllMessages(PDUDecrypterBase d)
        {
            do
            {
                var data = new byte[512];
                var len = 0;
                while((len = d.Read(data, 0, 512)) > 0)
                {
                    Console.Write(Encoding.ASCII.GetString(data, 0, len));
                    data = new byte[512];
                }
                Console.Write("\n");
            } while(d.NewMessage());
        }
    }
}