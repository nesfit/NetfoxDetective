// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
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
using System.IO;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Netfox.SnooperFacebook.Tests.Tests
{
    /// <summary>
    /// Test class for benchmark analysis of reconstruction.
    /// </summary>
    [Explicit]
    [Category("Explicit")]
    public class SnooperFacebookPerformanceTests : SnooperBaseTests
    {
        /*************************************************************************************************************
		* BEWARE of TearDown method from SnooperBaseTests, it dumps exported objects and can prolong tests by a lot. *
		*************************************************************************************************************/

        [Test]
        public void Test_Facebook_chat_1_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 1; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 1 itearion chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Test_Facebook_chat_10_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 10; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 10 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Test_Facebook_chat_50_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 50; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 50 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Test_Facebook_chat_100_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 100; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 100 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Test_Facebook_chat_200_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 200; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 200 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Test_Facebook_chat_500_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 500; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 500 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }

        [Test]
        public void Test_Facebook_chat_750_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 750; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 750 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }
        [Test]
        public void Test_Facebook_chat_1000_iterations()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.facebook_fb_chat_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.fb_pk);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);


            var watch = new Stopwatch();
            // clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            watch.Start();
            for (var i = 0; i < 1000; i++)
            {
                this.FrameworkController.ExportData(new List<ISnooper> { new SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            }
            watch.Stop();

            Debug.WriteLine(@"Benchmark 1000 itearions chat: {0} ms", watch.Elapsed.TotalMilliseconds);
        }
    }
}