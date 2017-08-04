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
using Netfox.Detective.Infrastructure;
using NUnit.Framework;

namespace Netfox.Detective.Tests
{
    [TestFixture]
    public abstract class WindsorInvestigationInstallerTestsBase<TInstaller> : WindsorInstallerTestsBase where TInstaller : DetectiveIvestigationWindsorInstallerBase, new() { 
        protected TInstaller WindsorInstaller { get; } = new TInstaller();

        protected abstract int AutoRegisterDetectivePaneViewsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; }
        protected abstract int DetectiveInvestigationPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredCount { get; }
        protected abstract int DetectiveInvestigationDataEntityPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; }
        protected abstract int InstallRegisterTypesAllTypesAreRegisteredAssertedCount { get; }
        protected abstract int RegisterOtherTypesRegisterTypesAllTypesAreRegisteredAssertedCount { get; }
        protected abstract int SnooperRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; }
        protected abstract int RegisterServicesRegisterServicesDetectiveScopeServicesAreRegisteredAssertedCount { get; }
        protected abstract int AnalyzersRegistrationInvestigationAnalyzerRegisterInvestigationAnalyzerRegisterAreRegisteredAssertedCount { get; }
        [Test]
        public void AutoRegisterDetectivePaneViewsRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.AutoRegisterDetectivePaneViewsRegister(this.WindsorContainer);

            this.AssertAllHandlersAtLeastCount(this.AutoRegisterDetectivePaneViewsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount);

            if(this.GetAllHandlers().Any(h => !h.ComponentModel.Services.Any(s => s.IsInterface)))
            {
                var brokenComponents = this.GetAllHandlers().Where(h => h.ComponentModel.Services.Any(s => s.IsInterface == false)).ToArray();
                if(brokenComponents.Any())
                {
                    TestContext.WriteLine("Broken components:");
                    foreach(var brokenComponent in brokenComponents) { TestContext.WriteLine(brokenComponent.ComponentModel.Name); }
                    Assert.Fail("Broken Components present");
                }
            }

            this.AssertViews();
        }

        [Test]
        public void DetectiveInvestigationPaneViewModelsRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.DetectiveInvestigationPaneViewModelBaseRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(this.DetectiveInvestigationPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredCount);
        }

        [Test]
        public void DetectiveInvestigationDataEntityPaneViewModelsRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.DetectiveIvestigationDataEntityPaneViewModelBaseRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(this.DetectiveInvestigationDataEntityPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount);
        }

        [Test]
        public void Install_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.Install(this.WindsorContainer, null);
            this.AssertAllHandlersAtLeastCount(this.InstallRegisterTypesAllTypesAreRegisteredAssertedCount);

            this.AssertViews();
            this.AssertServices();
            this.AssertIEntityViewModels();
        }

        [Test]
        public void RegisterOtherTypes_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.RegisterOtherTypes(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(this.RegisterOtherTypesRegisterTypesAllTypesAreRegisteredAssertedCount);
        }

        [Test]
        public void SnooperRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.SnooperRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(this.SnooperRegisterRegisterTypesAllTypesAreRegisteredAssertedCount);
        }
        [Test]
        public void RegisterServices_RegisterServices_DetectiveScopeServicesAreRegistered()
        {
            this.WindsorInstaller.ServicesRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(this.RegisterServicesRegisterServicesDetectiveScopeServicesAreRegisteredAssertedCount);

            this.AssertServices();
        }

        [Test]
        public void AnalyzersRegistration_InvestigationAnalyzerRegister_InvestigationAnalyzerRegisterAreRegistered()
        {
            this.WindsorInstaller.AnalyzersRegistration(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(this.AnalyzersRegistrationInvestigationAnalyzerRegisterInvestigationAnalyzerRegisterAreRegisteredAssertedCount);

            this.AssertServices();
        }

        protected abstract void AssertIEntityViewModels();

        protected abstract void AssertServices();

        protected abstract void AssertViews();
    }
}