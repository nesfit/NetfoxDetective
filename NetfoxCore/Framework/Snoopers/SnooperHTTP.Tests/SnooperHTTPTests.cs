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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperHTTP.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperHTTP.Tests
{
    internal class SnooperHTTPTests : SnooperBaseTests
    {
        public SnooperHTTPTests()
        {
            this.SnoopersToUse = new List<ISnooper> { new SnooperHTTP() };
        }

        [Test]
        public void HTTPTest_http_malyweb3()
        {
            var httpMessagePatterns = new[] //Messages - timestampes, HttpRequestHeader and HttpResponseHeader
             {
                new [] { "22.11.2013 14:52:17", "Accept: */*; \nReferer: http://www.fit.vutbr.cz/; \nAccept-Language: cs,en-US;q=0.7,en;q=0.3; \nUser-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0; Avant Browser); \nAccept-Encoding: gzip, deflate; \nHost: www.fit.vutbr.cz; \nConnection: Keep-Alive; \n", ""},
                new [] { "22.11.2013 14:52:23", "", "Date: Fri, 22 Nov 2013 14:52:24 GMT; \nServer: Apache; \nCache-Control: max-age=604800; \nExpires: Fri, 29 Nov 2013 14:52:24 GMT; \nLast-Modified: Fri, 29 Jun 2012 15:06:59 GMT; \nETag: \"7a1fd3-800c-4fedc493\"; \nAccept-Ranges: bytes; \nContent-Length: 32780; \nContent-Type: image/jpeg; \n" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.http_caps_malyweb3_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            SnooperExportHTTP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get SnooperExportHTTP exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportHTTP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(2, exportedObjectBases.Length); //Total number of exported objects should be 2

            var httpObjects = exportedObjectBases.Where(i => i is SnooperExportedDataObjectHTTP).Cast<SnooperExportedDataObjectHTTP>().OrderBy(it => it.TimeStamp).ToArray();

            HTTPMsg[] messages = new HTTPMsg[httpObjects.Length];
            int j = 0;
            foreach (SnooperExportedDataObjectHTTP o in httpObjects)
            {
                messages[j] = o.Message;
                j++;
            }
            Assert.AreEqual(2, messages.Length);
            Assert.AreEqual(httpMessagePatterns.Length, messages.Length); //Both arrays should have same length

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), httpMessagePatterns[i][0]);
                Assert.AreEqual(messages[i].HttpRequestHeader.ToString(), httpMessagePatterns[i][1]);
                Assert.AreEqual(messages[i].HttpResponseHeader.ToString(), httpMessagePatterns[i][2]);
            }
        }

        [Test]
        public void HTTPTest_http_performance()
        {

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_gmail_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var sw = new Stopwatch();
            sw.Start();
            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);
            sw.Stop();
            Console.WriteLine("################# SnooperHTTP time elapsed: {0}, Total Conversations: {1} ######################", sw.Elapsed, conversations.Count());
        }

        [Test]
        public void Test_HTTPSleuth_with_decryption()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_gmail_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);

            Assert.AreEqual(1, this.SnooperExports.Count);
            Assert.AreEqual(42, this.GetExportedObjectCount());
        }

    }
}