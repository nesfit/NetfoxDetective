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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Netfox.Core.Database;
using Netfox.Core.Infrastructure;
using Netfox.Core.Models;
using Netfox.Framework.Models;
using Netfox.NetfoxFrameworkAPI.Infrastruture;
using NUnit.Framework;

namespace Netfox.NetfoxFrameworkAPI.Tests
{
    [TestFixture]
    public abstract class UnitTestBaseSetupFixture
    {
        private static readonly Mutex SingleTestMutex = new Mutex();

        /// <summary> The test start Date/Time.</summary>
        private static DateTime _testStart;

        public String TestBaseDirectory = "output\\";
        public WindsorContainer WindsorContainer { get; private set; }

        public DirectoryInfo CurrentTestBaseDirectory
        {
            get
            {
                var stackTrace = new StackTrace(); // get call stack
                var stackFrames = stackTrace.GetFrames();
                var path = Path.Combine(this.TestBaseDirectory, stackFrames.Skip(1).First().GetMethod().Name);
                var currentTestBaseDirectory = new DirectoryInfo(path);
                if(currentTestBaseDirectory.Exists) currentTestBaseDirectory.Delete(true);
                currentTestBaseDirectory.Create();
                return currentTestBaseDirectory;
            }
        }

        public InvestigationInfo InvestigationInfo { get; set; }

        public IObservableNetfoxDBContext ObservableNetfoxDBContext { get; set; }

        public FileInfo PrepareCaptureForProcessing(string filePath)
        {
            var originalFile = new FileInfo(filePath);
            if(!originalFile.Exists)
            {
                originalFile = new FileInfo(Path.Combine(@"..\", filePath));
            }
            if (!originalFile.Exists)
            {
                throw new ArgumentException($"Capture file does not exists: {filePath}",nameof(filePath));
            }
            var newFile =
                originalFile.CopyTo(Path.Combine(this.InvestigationInfo.SourceCaptureDirectoryInfo.CreateSubdirectory(originalFile.Name + Guid.NewGuid()).FullName,
                    originalFile.Name));
            return newFile;
        }

        public void SetPrivateKeyToConversations(IEnumerable<L7Conversation> conversations, string filePath)
        {
            if(!File.Exists(filePath))
            {
                filePath = Path.Combine(@"..\", filePath);
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentException($"Private key file does not exists: {filePath}", nameof(filePath));
            }
            var key = File.ReadAllText(filePath);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = key
                };
            }
        }

        public virtual void SetUpInMemory()
        {
            this.SetUp();
            var netfoxFrameworkApiWindsorInstaller = new NetfoxFrameworkApiWindsorInstaller();
            netfoxFrameworkApiWindsorInstaller.SetUpInMemory(this.WindsorContainer);
        }

        /// <summary> Tests start.</summary>
        public virtual void SetUpSQL()
        {
            this.SetUp();
            var netfoxFrameworkApiWindsorInstaller = new NetfoxFrameworkApiWindsorInstaller();
            netfoxFrameworkApiWindsorInstaller.SetUpSQL(this.WindsorContainer);
        }

        /// <summary> Tests stop.</summary>
        [TearDown]
        public virtual void TearDown()
        {
            var testStop = DateTime.Now;
            var duration = testStop - _testStart;
            Console.WriteLine("Timeduration: " + duration.Hours + ":" + duration.Minutes + ":" + duration.Seconds + "." + duration.Milliseconds);
            this.WindsorContainer = null;
            SingleTestMutex.ReleaseMutex();
        }

        private void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            this.SingleTestWaitOne();
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

            this.WindsorContainer = new WindsorContainer("BaseUnitTestWC", new DefaultKernel(), new DefaultComponentInstaller());
            this.WindsorContainer.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(this.WindsorContainer));
            //this.WindsorContainer.Install(new NetfoxFrameworkApiWindsorInstaller());
            this.WindsorContainer.Install(FromAssembly.InDirectory(new AssemblyFilter("."), new InstallerFactoryFilter(typeof(IDetectiveIvestigationWindsorInstaller))));

            this.InvestigationInfo = this.WindsorContainer.Resolve<InvestigationInfo>();

            this.InvestigationInfo.InvestigationName = "NetfoxUnitTest";
            this.InvestigationInfo.InvestigationsDirectoryInfo = GetTempDirectory();
            this.InvestigationInfo.CreateFileStructure();
            _testStart = DateTime.Now;
        }

        private static DirectoryInfo GetTempDirectory()
        {
            var dir = new DirectoryInfo(@"C:\NetfoxTemp");
            dir.Create();
            return dir;
        }

        protected void SingleTestWaitOne() { SingleTestMutex.WaitOne(); }
    }
}