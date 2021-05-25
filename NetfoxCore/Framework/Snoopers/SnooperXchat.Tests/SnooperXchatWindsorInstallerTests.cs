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

using Castle.Core;
using Netfox.Detective.Tests;
using Netfox.Snoopers.SnooperXchat.WPF.Infrastructure;
using Netfox.Snoopers.SnooperXchat.WPF.ViewModels;
using Netfox.Snoopers.SnooperXchat.WPF.Views.Interface;
using NUnit.Framework;
using XChatView = Netfox.Snoopers.SnooperXchat.WPF.Views.XChatView;

//using Netfox.SnooperXchat.Interfaces;

namespace Netfox.Snoopers.SnooperXchat.Tests
{
    [TestFixture]
    public class SnooperXchatWindsorInstallerTests : WindsorInvestigationInstallerTestsBase<SnooperXchatWindsorInstaller>
    {
        #region Overrides of WindsorInvestigationInstallerTestsBase<SnooperBTCWindsorInstaller>
        protected override int AutoRegisterDetectivePaneViewsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 2;
        protected override int DetectiveInvestigationDataEntityPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 2;
        protected override int InstallRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 11;
        protected override int RegisterOtherTypesRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 1;
        protected override int SnooperRegisterRegisterTypesAllTypesAreRegisteredAssertedCount { get; } = 2;
        protected override int RegisterServicesRegisterServicesDetectiveScopeServicesAreRegisteredAssertedCount { get; } = 1;
        protected override int AnalyzersRegistrationInvestigationAnalyzerRegisterInvestigationAnalyzerRegisterAreRegisteredAssertedCount { get; } = 1;

        protected override void AssertIEntityViewModels() { Assert.IsTrue(this.ContainsComponent(typeof(XchatViewModel), LifestyleType.Custom)); }

        protected override void AssertServices() { }

        protected override void AssertViews() { Assert.IsTrue(this.ContainsComponent(typeof(IXChatView), typeof(XChatView), LifestyleType.Transient)); }

        protected override int DetectiveInvestigationPaneViewModelsRegisterRegisterTypesAllTypesAreRegisteredCount { get; } = 1;
        #endregion
    }
}