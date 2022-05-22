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
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperMQTT.Models;
using Netfox.Snoopers.SnooperMQTT.Models.Commands;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperMQTT.Tests
{
    class SnooperMQTTTests : SnooperBaseTests
    {
        [Test][Explicit][Category("Explicit")]
        public void Test_MQTT_test()
        {
            var captureFile = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.messenger_messenger1_pcapng));
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            //Assert.AreEqual(this.SnooperExports.Count, 1);
            var objs = this.SnooperExports.FirstOrDefault()?.ExportObjects.Where(o => o is MQTTMsg).Cast<MQTTMsg>().ToArray();
            Assert.AreEqual(objs.Count(), 65);

            var firstObj = objs.FirstOrDefault();

            Assert.True(firstObj.Command is MQTTCommandConnect);
            Assert.True((firstObj.Command as MQTTCommandConnect).ProtocolName == "MQTToT");

            /*Assert.True((objs[24] as MQTTMsg)?.Command is MQTTCommandPublish);
            Assert.True(((objs[24] as MQTTMsg)?.Command as MQTTCommandPublish).Topic == "/typing");
            Assert.True(((objs[24] as MQTTMsg)?.Command as MQTTCommandPublish).Payload.Length == 37);

            Assert.True((objs[50] as MQTTMsg)?.Command is MQTTCommandPingRequest);*/
        }
    }
}