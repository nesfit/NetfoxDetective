﻿// Copyright (c) 2017 Jan Pluskal, Matus Dobrotka
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

using Netfox.Detective.Tests;
using Netfox.Snoopers.SnooperTwitter.WPF.Infrastructure;
using NUnit.Framework;

//using Netfox.SnooperTwitter.Interfaces;
//using Netfox.SnooperTwitter.ViewModels;
//using Netfox.SnooperTwitter.Views;

namespace Netfox.Snoopers.SnooperTwitter.Tests
{
    [TestFixture]
    public class SnooperTwitterWindsorInstallerTests : WindsorInvestigationInstallerTestsBase<SnooperTwitterWindsorInstaller>
    {
        #region Overrides of WindsorInvestigationInstallerTestsBase<SnooperTwitterWindsorInstaller>
        protected override int AutoRegisterDetectivePaneViewsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 1;
        protected override int DetectiveInvestigationDataEntityPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 1;
        protected override int InstallRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 9;
        protected override int RegisterOtherTypesRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 1;
        protected override int SnooperRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 2;
        protected override int RegisterServicesRegisterServicesDetectiveScopeServicesAreRegisteredAssertedCount { get; } = 1;
        protected override int AnalyzersRegistrationInvestigationAnalyzerRegisterInvestigationAnalyzerRegisterAreRegisteredAssertedCount { get; } = 1;

        protected override void AssertIEntityViewModels() { }

        protected override void AssertServices() { }

        protected override void AssertViews() {  }

        protected override int DetectiveInvestigationPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredCount { get; } = 1;
        #endregion
    }
}