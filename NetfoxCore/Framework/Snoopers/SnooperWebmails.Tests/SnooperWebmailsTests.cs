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

using System.Globalization;
using System.IO;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperHTTP.Models;
using Netfox.Snoopers.SnooperWebmails.Models;
using Netfox.Snoopers.SnooperWebmails.Models.Spotters;
using Netfox.Snoopers.SnooperWebmails.Models.WebmailEvents;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperWebmails.Tests
{
    public class SnooperWebmailsTests : SnooperBaseTests
    {


        [Test]
        [Explicit][Category("Explicit")]
        public void TestSpotterKeyValue()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_live_test_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            var msgObject = this.SnooperExports.ToArray()[0].ExportObjects[4] as SnooperExportedDataObjectHTTP;
            Assert.AreNotEqual(null, msgObject);
            var msg = msgObject.Message;
            var spotter = new SpotterKeyValue();

            spotter.Init(msg);

            /* verify true states */
            Assert.AreEqual(true, spotter.IsSpottable());

            Assert.AreEqual(true, spotter.ContainsOneOf(new[] { "nieco", "cn" }, SpotterBase.SpotterContext.ContentKey));

            Assert.AreEqual(true, spotter.Contains("cnmn", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(true, spotter.ContainsKeyValuePair("mn", "MarkMessagesReadState", SpotterBase.SpotterContext.ContentPair));

            /* verify false states */

            Assert.AreEqual(false, spotter.ContainsOneOf(new[] { "nieco", "ine" }, SpotterBase.SpotterContext.ContentKey));

            Assert.AreEqual(false, spotter.Contains("hocico", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(false, spotter.ContainsKeyValuePair("mn", "GetInboxData", SpotterBase.SpotterContext.AllPair));
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void TestSpotterJson()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_yahoo_rc4_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            var msgObject = this.SnooperExports.ToArray()[0].ExportObjects[41] as SnooperExportedDataObjectHTTP;
            Assert.AreNotEqual(null, msgObject);
            var msg = msgObject.Message;
            var spotter = new SpotterJson();

            spotter.Init(msg);

            /* verify true states */
            Assert.AreEqual(true, spotter.IsSpottable());

            Assert.AreEqual(true, spotter.ContainsOneOf(new[] { "nieco", "Test Netfox" }, SpotterBase.SpotterContext.ContentValue));

            Assert.AreEqual(true, spotter.Contains("message", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(true, spotter.ContainsKeyValuePair("subject", "nova sprava", SpotterBase.SpotterContext.ContentPair));

            /* verify false states */
            Assert.AreEqual(false, spotter.ContainsOneOf(new[] { "nieco", "ine" }, SpotterBase.SpotterContext.ContentKey));

            Assert.AreEqual(false, spotter.Contains("hocico", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(false, spotter.ContainsKeyValuePair("subject", "Confidentioal", SpotterBase.SpotterContext.AllPair));
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void TestSpotterMultipart()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_yahoo_rc4_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            var msgObject = this.SnooperExports.ToArray()[0].ExportObjects[9] as SnooperExportedDataObjectHTTP;
            Assert.AreNotEqual(null, msgObject);
            var msg = msgObject.Message;
            var spotter = new SpotterMultipart(new SpotterPool());

            spotter.Init(msg);

            /* verify true states */
            Assert.AreEqual(true, spotter.IsSpottable());

            Assert.AreEqual(true, spotter.ContainsOneOf(new[] { "nieco", "Inbox" }, SpotterBase.SpotterContext.ContentValue));

            Assert.AreEqual(true, spotter.Contains("successRequests", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(true, spotter.ContainsKeyValuePair("name", "msgid", SpotterBase.SpotterContext.ContentPair));

            /* verify false states */
            Assert.AreEqual(false, spotter.ContainsOneOf(new[] { "nieco", "ine" }, SpotterBase.SpotterContext.ContentKey));

            Assert.AreEqual(false, spotter.Contains("hocico", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(false, spotter.ContainsKeyValuePair("subject", "Confidentioal", SpotterBase.SpotterContext.AllPair));
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void TestSpotterFRPC()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_seznam_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            var msgObject = this.SnooperExports.ToArray()[0].ExportObjects[14] as SnooperExportedDataObjectHTTP;
            Assert.AreNotEqual(null, msgObject);
            var msg = msgObject.Message;
            var spotter = new SpotterFRPC();

            spotter.Init(msg);

            /* verify true states */
            Assert.AreEqual(true, spotter.IsSpottable());

            Assert.AreEqual(true, spotter.ContainsOneOf(new[] { "nieco", "subject" }, SpotterBase.SpotterContext.ContentKey));

            Assert.AreEqual(true, spotter.Contains("user.message.send", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(true, spotter.ContainsKeyValuePair("subject", "Re: test opera", SpotterBase.SpotterContext.ContentPair));

            /* verify false states */
            Assert.AreEqual(false, spotter.ContainsOneOf(new[] { "nieco", "ine" }, SpotterBase.SpotterContext.ContentKey));

            Assert.AreEqual(false, spotter.Contains("hocico", SpotterBase.SpotterContext.AllKey));

            Assert.AreEqual(false, spotter.ContainsKeyValuePair("subject", "Confidentioal", SpotterBase.SpotterContext.AllPair));
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void TestMicrosoftLive()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_live_test_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperWebmails), this.SnooperExports, this.CurrentTestBaseDirectory);

            SnooperExportWebmail exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get WebmailSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportWebmail) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(2, exportedObjectBases.Length);

            var objs = exportedObjectBases.OrderBy(it => it.TimeStamp);
            Assert.AreEqual(2, objs.Count());
            Assert.IsTrue(objs.First() is EventNewMessage);
            Assert.AreEqual(((EventNewMessage)objs.First()).From, "");
            Assert.AreEqual(((EventNewMessage)objs.First()).To, "");
            Assert.AreEqual(((EventNewMessage)objs.First()).Subject, "");
            Assert.AreEqual(((EventNewMessage)objs.First()).TimeStamp.ToString(new CultureInfo("cs-CZ", false)),"22.04.2015 18:56:05");

            Assert.IsTrue(objs.Last() is EventListFolder);
            Assert.AreEqual(((EventListFolder)objs.Last()).TimeStamp.ToString(new CultureInfo("cs-CZ", false)), "22.04.2015 18:56:08");
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void TestGmail()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_gmail_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperWebmails), this.SnooperExports, this.CurrentTestBaseDirectory);

            Assert.AreEqual(2, this.SnooperExports.Count);
            Assert.GreaterOrEqual(9, this.SnooperExports.ToArray()[0].ExportObjects.Count);
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void TestYahoo()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_yahoo_rc4_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperWebmails), this.SnooperExports, this.CurrentTestBaseDirectory);

            Assert.AreEqual(2, this.SnooperExports.Count);
            Assert.GreaterOrEqual(10, this.SnooperExports.ToArray()[0].ExportObjects.Count);
        }

        [Test][Explicit][Category("Explicit")]
        public void TestSeznam()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_seznam_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperWebmails), this.SnooperExports, this.CurrentTestBaseDirectory);

            SnooperExportWebmail exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get WebmailSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportWebmail) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(0, exportedObjectBases.Length);
        }


        //private void FrameworkController_SnooperExport(FcOperationContext fcOperationContext, SnooperExportBase snooperExport) => this.SnooperExports.Add(snooperExport);
    }
}
