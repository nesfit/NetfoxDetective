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
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperHangouts.Models;
using Netfox.Snoopers.SnooperHangouts.Models.Events;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperHangouts.Tests
{
    class SnooperHangoutsTests : SnooperBaseTests
    {
        [Test]
        [Explicit]
        [Category("Explicit")]
        //Broken tests
        public void Test_Hangouts_test()
        {
            var captureFile = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.hangouts_hangouts1_pcapng));
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            this.SetPrivateKeyToConversations(conversations, PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHangouts() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            HangoutsSnooperExport exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get HangoutsSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as HangoutsSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            //var objs = this.SnooperExports.ToArray().Last().ExportObjects;
            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(6, exportedObjectBases.Length); //Total number of exported objects should be 6

            Assert.True(exportedObjectBases[0] is HangoutsEventActiveClient);
            Assert.True((exportedObjectBases[0] as HangoutsEventActiveClient).Active);

            Assert.True(exportedObjectBases[1] is HangoutsEventTyping);
            Assert.True((exportedObjectBases[1] as HangoutsEventTyping).Type == TypingType.TYPING_TYPE_STOPPED);

            Assert.True(exportedObjectBases[2] is HangoutsEventTyping);
            Assert.True((exportedObjectBases[2] as HangoutsEventTyping).Type == TypingType.TYPING_TYPE_STARTED);

            Assert.True(exportedObjectBases[3] is HangoutsEventChatMessage);
            Assert.True((exportedObjectBases[3] as HangoutsEventChatMessage).Content == "Test");

            Assert.True(exportedObjectBases[4] is HangoutsEventTyping);
            Assert.True((exportedObjectBases[4] as HangoutsEventTyping).Type == TypingType.TYPING_TYPE_STARTED);

            Assert.True(exportedObjectBases[5] is HangoutsEventChatMessage);
            Assert.True((exportedObjectBases[5] as HangoutsEventChatMessage).Content == "");
        }
    }
}
