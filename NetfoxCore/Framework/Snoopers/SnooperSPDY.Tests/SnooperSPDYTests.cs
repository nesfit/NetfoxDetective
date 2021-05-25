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
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperSPDY.Tests
{
    public class SnooperSPDYTests : SnooperBaseTests
    {

        [Test][Explicit][Category("Explicit")]
        public void Test_SPDY_test()
        {
            var captureFile = this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.twitter_twitter1_pcapng));
            this.FrameworkController.ProcessCapture(captureFile);

            var conversations = this.L7Conversations.ToArray();

            var privateKey = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));
            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = privateKey };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
        }

       
    }
}