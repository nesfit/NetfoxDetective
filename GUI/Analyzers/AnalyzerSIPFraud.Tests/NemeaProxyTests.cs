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
using System.Threading.Tasks;
using Netfox.AnalyzerSIPFraud.Services;
using NUnit.Framework;

namespace AnalyzerSIPFraud.Tests
{
    [TestFixture, Ignore("Explicit")]
    class NemeaProxyTests
    {
        [Test]
        public void ParseTest()
        {
            using (var proxy = new NemeaProxy())
            {
                var msg = proxy.Parse("{\"stats\": { \"caller-count\": 694200, \"callee-count\": 694200, \"invite-count\": 694200, \"calls-per-caller\": 1},\"type\": \"monitoring\"}");
            }
        }

        [Test]
        public async Task ConnectTest()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                using (var textReader = new StreamReader(proxy.NetworkStream))
                {
                    var line = textReader.ReadLine();
                }
            }
        }

        [Test]
        public async Task NemeaReadTest()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                char[] Buffer = new char[1024];
                using (var textReader = new StreamReader(proxy.NetworkStream))
                {
                    var read = textReader.Read(Buffer, 0, 1024);
                }
            }
        }


        [Test]
        public async Task GetJsonMessageTest()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("telnet://172.16.0.1:9999"));
                var msg = proxy.GetJsonMessage();
            }
        }

        [Test]
        public async Task GetMessageTest()
        {
            using(var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                var msg = proxy.GetMessage();
            }
        }


        [Test]
        public async Task ConnectTestVagrant()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                using (var textReader = new StreamReader(proxy.NetworkStream))
                {
                    var line = textReader.ReadLine();
                }
            }
        }

        [Test]
        public async Task NemeaReadTestVagrant()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                char[] Buffer = new char[1024];
                using (var textReader = new StreamReader(proxy.NetworkStream))
                {
                    var read = textReader.Read(Buffer, 0, 1024);
                }
            }
        }


        [Test]
        public async Task GetJsonMessageTestVagrant()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                var msg = proxy.GetJsonMessage();
            }
        }

        [Test]
        public async Task GetMessageTestVagrant()
        {
            using (var proxy = new NemeaProxy())
            {
                await proxy.Connect(new Uri("http://sauvignon.liberouter.org:9999/"));
                var msg = proxy.GetMessage();
            }
        }
    }
}
