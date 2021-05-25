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

using Netfox.Detective.Infrastructure;
using Netfox.Detective.Interfaces;
using NUnit.Framework;

namespace Netfox.Detective.Tests
{
    public class DetectiveIvestigationWindsorInstallerTests : WindsorInvestigationInstallerTestsBase<DetectiveIvestigationWindsorInstaller>
    {
        #region Overrides of WindsorInvestigationInstallerTestsBase<DetectiveIvestigationWindsorInstallerBase>
        protected override int AutoRegisterDetectivePaneViewsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 27;
        protected override int DetectiveInvestigationDataEntityPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 8;
        protected override int InstallRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 61;
        protected override int RegisterOtherTypesRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 1;
        protected override int SnooperRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 0;
        protected override int RegisterServicesRegisterServicesDetectiveScopeServicesAreRegisteredAssertedCount { get; } = 1;
        protected override int AnalyzersRegistrationInvestigationAnalyzerRegisterInvestigationAnalyzerRegisterAreRegisteredAssertedCount { get; } = 0;
        protected override void AssertIEntityViewModels() { }
        protected override void AssertServices() { }
        protected override void AssertViews() { }
        protected override int DetectiveInvestigationPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredCount { get; } = 0;
        #endregion



        [Test]
        public void RegisterViewModelModelResolver_ResolveViewModelModelResolver_ComponentResolved()
        {
            this.WindsorInstaller.Install(this.WindsorContainer, null);
            var component = this.WindsorContainer.Resolve<ICrossContainerHierarchyResolver>();
            Assert.IsNotNull(component);
        }
    }
}