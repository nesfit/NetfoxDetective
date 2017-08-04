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
using System.IO;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperMessenger.Models;
using Netfox.SnooperMessenger.Models.Events;
using NUnit.Framework;

namespace Netfox.SnooperMessenger.Tests
{
    class SnooperMessengerTest : SnooperBaseTests
    {
        [Test][Explicit][Category("Explicit")]
        public void Test_Messenger_test()
        {
            var captureFile = this.PrepareCaptureForProcessing(SnoopersPcaps.Default.messenger_messenger1_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PrivateKeys.Default.fb_pk);
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperMQTT.SnooperMQTT() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperMessenger() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            MessengerSnooperExport exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get MessengerSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as MessengerSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var objs = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(objs.Length, 8);

            Assert.True(objs[0] is MessengerEventConnect);
            Assert.True((objs[0] as MessengerEventConnect).UserId == "100010419973288");

            Assert.True(objs[1] is MessengerEventForegroundState);
            Assert.True((objs[1] as MessengerEventForegroundState).ForegroundState == 1);

            Assert.True(objs[2] is MessengerEventTyping);
            Assert.True((objs[2] as MessengerEventTyping).State == 1);

            Assert.True(objs[3] is MessengerEventSendMessage);
            Assert.True((objs[3] as MessengerEventSendMessage).Body == "Hello world");

            Assert.True(objs[4] is MessengerEventTyping);
            Assert.True((objs[4] as MessengerEventTyping).State == 0);

            Assert.True(objs[5] is MessengerEventSendMessage);
            Assert.True((objs[5] as MessengerEventSendMessage).Attachement == "369239263222822");

            Assert.True(objs[6] is MessengerEventSendMessage);
            Assert.True((objs[6] as MessengerEventSendMessage).Attachement == "144884865685780");

            Assert.True(objs[7] is MessengerEventSendMessage);
            Assert.True((objs[7] as MessengerEventSendMessage).LocationAttachement == "16.57107387183596 49.23181081803079");
        }

        [Test]
        public void Test_Messenger_test2()
        {
            var captureFile = this.PrepareCaptureForProcessing(SnoopersPcaps.Default.messenger_messenger2_pcapng);
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PrivateKeys.Default.fb_pk);
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperMQTT.SnooperMQTT() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperMessenger() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            Assert.AreEqual(this.SnooperExports.Count, 2);
            var objs = this.SnooperExports.ToArray()[1].ExportObjects;
            Assert.AreEqual(objs.Count, 3);

            Assert.True(objs[0] is MessengerEventConnect);
            Assert.True((objs[0] as MessengerEventConnect).UserId == "100010419973288");

            Assert.True(objs[1] is MessengerEventForegroundState);
            Assert.True((objs[1] as MessengerEventForegroundState).ForegroundState == 1);

            Assert.True(objs[2] is MessengerEventReceiveMessage);
            Assert.True((objs[2] as MessengerEventReceiveMessage).Body == "Test");
        }

        /*
        [Test]
        public void Test_Thrift()
        {
            var smData = new Byte[]  {
                
               //0x00, 0x18, 0x0f, 0x31, 0x30, 0x30, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39,
               //0x39, 0x37, 0x33, 0x32, 0x38, 0x38, 0x18, 0x16, 0x4f, 0x62, 0x73, 0x61,
               //0x68, 0x20, 0x73, 0x75, 0x6b, 0x72, 0x6f, 0x6d, 0x6e, 0x65, 0x6a, 0x20,
               //0x73, 0x70, 0x72, 0x61, 0x76, 0x79, 0x16, 0xd8, 0xfe, 0xd3, 0xbc, 0xf7,
               //0xdd, 0x9c, 0xa2, 0xa9, 0x01, 0x96, 0xd6, 0xf3, 0xef, 0xba, 0x0c, 0x00
                
                
               //0x00, 0x18, 0x0F, 0x31, 0x30, 0x30, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x39, 0x37, 0x33, 0x32,
               //0x38, 0x38, 0x26, 0xBE, 0xDC, 0xEA, 0xBA, 0xD7, 0x87, 0xE4, 0xFB, 0xA8, 0x01, 0x96, 0xD6, 0xF3,
               //0xEF, 0xBA, 0x0C, 0x5C, 0x1C, 0x18, 0x11, 0x34, 0x38, 0x2E, 0x37, 0x38, 0x31, 0x37, 0x36, 0x34,
               //0x36, 0x38, 0x36, 0x35, 0x35, 0x36, 0x34, 0x31, 0x18, 0x11, 0x31, 0x38, 0x2E, 0x36, 0x31, 0x38,
               //0x39, 0x30, 0x31, 0x36, 0x33, 0x30, 0x36, 0x38, 0x33, 0x30, 0x31, 0x00, 0x11, 0x00, 0x00
                
                0x00, 0x18, 0x0f, 0x31, 0x30, 0x30, 0x30, 0x31, 0x30, 0x34, 0x31, 0x39, 0x39, 0x37, 0x33, 0x32,
                0x38, 0x38, 0x26, 0xc2, 0xae, 0xc3, 0xb6, 0xe2, 0x80, 0xa0, 0xe2, 0x80, 0x9c, 0xc3, 0x8a, 0xe2,
                0x82, 0xac, 0xc3, 0x9c, 0xe2, 0x80, 0xa1, 0xc2, 0xa9, 0x01, 0x38, 0x0f, 0x33, 0x36, 0x39, 0x32,
                0x33, 0x39, 0x32, 0x36, 0x33, 0x32, 0x32, 0x32, 0x38, 0x32, 0x32, 0x66, 0xc3, 0xb7, 0xc3, 0x9b,
                0xc3, 0x94, 0xe2, 0x88, 0xab, 0x0c, 0x00
            };
            var offset = 1;
            var inStream = new MemoryStream(smData, offset, smData.Length - offset);
            var tProto = new TCompactProtocol(new TStreamTransport(inStream, inStream));
            var msg = new SendMessageRequest();
            msg.Read(tProto);
        }
        */
    }
}
