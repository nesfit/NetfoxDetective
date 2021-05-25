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

using Castle.Core;
using Netfox.Detective.Tests;
using Netfox.Framework.CaptureProcessor.ApplicationRecognizer;
using Netfox.Framework.CaptureProcessor.Infrastructure;
using Netfox.Framework.Models.Interfaces;
using Netfox.Nbar;
using Netfox.NBARDatabase;
using Netfox.RTP;
using NUnit.Framework;

namespace Netfox.Framework.Tests.Infrastructure
{
    [TestFixture()]
   public class CaptureProcessorWindsorInstallerTests: WindsorInstallerTestsBase
    {
        public CaptureProcessorWindsorInstallerTests() { this.WindsorInstaller = new CaptureProcessorWindsorInstaller(); }

        public CaptureProcessorWindsorInstaller WindsorInstaller { get; }
        [Test]
        public void Install_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.Install(this.WindsorContainer, null);
            this.AssertAllHandlersAtLeastCount(33);

            this.AssertRecognizers();
        }

        [Test]
        public void RegisterOtherTypes_RegisterTypes_AllTypesAreRegistered()
        {
            //this.WindsorInstaller.OtherTypesRegister(this.WindsorContainer);
            this.WindsorInstaller.OtherTypesRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(17);

            this.AssertRecognizers();
        }

        protected void AssertRecognizers()
        {
            Assert.IsTrue(this.ContainsComponent(typeof(IApplicationRecognizer), typeof(ApplicationRecognizerDefault), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(ApplicationRecognizerDefault), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(ApplicationRecognizerRTP), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(ApplicationRecognizerNBAR), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(ApplicationRecognizerNBAR), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(NBARProtocolPortDatabase), LifestyleType.Singleton));
        }

    }
}
