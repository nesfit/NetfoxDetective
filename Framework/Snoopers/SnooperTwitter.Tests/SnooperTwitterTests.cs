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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperTwitter.Models;
using Netfox.SnooperTwitter.Models.Events;
using NUnit.Framework;

namespace Netfox.SnooperTwitter.Tests
{
    [TestFixture]
    public class SnooperTwitterTests : SnooperBaseTests
    {
        [Test]
        [Explicit]
        [Category("Explicit")]
        //broken reader
        public void Test_Twitter_test()
        {
            var captureFile = this.PrepareCaptureForProcessing(SnoopersPcaps.Default.twitter_twitter1_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PrivateKeys.Default.fb_pk);
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperSPDY.SnooperSPDY() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperTwitter() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            TwitterSnooperExport exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get TwitterSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as TwitterSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var objs = exportedObjectsReference.ExportObjects.ToArray();

            Assert.AreEqual(3, objs.Length);

            Assert.True(objs[0] is TwitterEventCreateTweet);
            Assert.True((objs[0] as TwitterEventCreateTweet).Text == "Teeest tweet");

            Assert.True(objs[1] is TwitterEventTimelineView);
            Assert.AreEqual("01.04.2016 20:54:43", objs[1].TimeStamp.ToString(new CultureInfo("cs-CZ", false)));

            Assert.True(objs[2] is TwitterEventTimelineView);
            Assert.AreEqual("01.04.2016 20:54:27", objs[2].TimeStamp.ToString(new CultureInfo("cs-CZ", false)));
        }

        [Test]
        public void Test_Twitter_test2()
        {
            var captureFile = this.PrepareCaptureForProcessing(SnoopersPcaps.Default.twitter_twitter2_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PrivateKeys.Default.fb_pk);
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperSPDY.SnooperSPDY() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperTwitter() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            TwitterSnooperExport exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get TwitterSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as TwitterSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var objs = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(12, objs.Length);

            Assert.True(objs[0] is TwitterEventTimelineView);

            Assert.True(objs[1] is TwitterEventCreateTweet);
            Assert.True((objs[1] as TwitterEventCreateTweet).Text == "Lorem tweetsum");

            Assert.True(objs[2] is TwitterEventTimelineView);

            Assert.True(objs[3] is TwitterEventSearch);
            Assert.True((objs[3] as TwitterEventSearch).Query == "se");

            Assert.True(objs[8] is TwitterEventSearch);
            Assert.True((objs[8] as TwitterEventSearch).Query == "securitay");

            Assert.True(objs[9] is TwitterEventUserLookup);
            Assert.True((objs[9] as TwitterEventUserLookup).UserIds.Count == 5);

            Assert.True(objs[10] is TwitterEventUserLookup);
            Assert.True((objs[10] as TwitterEventUserLookup).UserIds.Count == 3);

            Assert.True(objs[11] is TwitterEventSendMessage);
            Assert.True((objs[11] as TwitterEventSendMessage).Text == "Lorem blahsum");
        }
    }
}
