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

using System.Linq;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.Framework.Tests
{
   public  class PmFrameBaseTest : FrameworkBaseTests
    {

        [SetUp]
        public override void SetUpInMemory()
        {
            base.SetUpInMemory();
        }
        [Test]
        [Explicit][Category("Explicit")]
        public void OriginalLengthWithoutPaddingTest()
        {
            var captureFileInfo = this.PrepareCaptureForProcessing(Pcaps.Default.pcap_mix_tcp_overflow_seq_mod_cap);
            this.FrameworkController.ProcessCapture(captureFileInfo);
            foreach(var pmFrame in this.PmFrames.Where(f => f.IncludedLength < 60))
            {
                Assert.AreEqual(54,pmFrame.OriginalLengthWithoutPadding);
            }
        }
    }

   

}
