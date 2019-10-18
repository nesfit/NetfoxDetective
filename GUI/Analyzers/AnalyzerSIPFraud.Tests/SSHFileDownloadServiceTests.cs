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
using System.IO;
using System.Linq;
using Netfox.AnalyzerSIPFraud.Services;
using NUnit.Framework;

namespace AnalyzerSIPFraud.Tests
{
    [TestFixture]
    public class SSHFileDownloadServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        }
        [Test, Ignore("ondemand")]
        public void DownloadTest()
        {
            var sshProxy = new SSHFileDownloadService();
            var downloadedFiles = sshProxy.Download(new Uri(@"ssh://pluskal@sauvignon.liberouter.org:/home/shared/pluskal/sip_fraud.pcap"));
            Assert.IsTrue(Enumerable.Any<FileInfo>(downloadedFiles));
            Assert.IsTrue(downloadedFiles?.FirstOrDefault()?.Exists);
        }
    }
}
