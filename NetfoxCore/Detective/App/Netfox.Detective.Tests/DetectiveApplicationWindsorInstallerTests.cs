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
using Castle.Core;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Models;
using Netfox.Detective.Infrastructure;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.Interfaces;
using Netfox.Detective.ViewModels.Workspaces;
using Netfox.Detective.Views.Explorers;
using Netfox.Detective.Views.Services;
using Netfox.Detective.Views.Windows;
using Netfox.Detective.Views.Workspaces;
using NUnit.Framework;

namespace Netfox.Detective.Tests
{
    public class DetectiveApplicationWindsorInstallerTests : WindsorInstallerTestsBase
    {
        public DetectiveApplicationWindsorInstallerTests() { this.WindsorInstaller = new DetectiveApplicationWindsorInstaller(); }

        public DetectiveApplicationWindsorInstaller WindsorInstaller { get; }

        [Test]
        public void AutoRegisterDetectivePaneViewsRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.AutoRegisterDetectivePaneViewsRegister(this.WindsorContainer);

            this.AssertAllHandlersAtLeastCount(27);

            var brokenComponents = this.GetAllHandlers().Where(h => h.ComponentModel.Services.Any(s => s.IsInterface == false)).Where(s=>s.ComponentModel.Name != "Castle.Windsor.WindsorContainer").ToArray();
            if(brokenComponents.Any())
            {
                TestContext.WriteLine("Broken components:");
                foreach(var brokenComponent in brokenComponents) { TestContext.WriteLine(brokenComponent.ComponentModel.Name); }
                Assert.Fail("Broken Components present");
            }

            this.AssertViews();
        }

        [Test]
        public void DetectiveApplicationPaneViewModelBaseRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.DetectiveApplicationPaneViewModelBaseRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(12);
        }

        [Test]
        public void DetectiveWindowViewModelBaseRegister_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.DetectiveWindowViewModelBaseRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(6);
        }

        [Test]
        public void Install_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.Install(this.WindsorContainer, null);
            this.AssertAllHandlersAtLeastCount(58);

            this.AssertServices();
        }

        [Test]
        public void RegisterOtherTypes_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.RegisterOtherTypes(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(5);
        }

        [Test]
        public void AnalyzersRegistration_RegisterTypes_AllTypesAreRegistered()
        {
            this.WindsorInstaller.AnalyzersRegistration(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(0);
        }

        [Test]
        public void RegisterServices_RegisterServices_DetectiveScopeServicesAreRegistered()
        {
            this.WindsorInstaller.ServicesRegister(this.WindsorContainer);
            this.AssertAllHandlersAtLeastCount(5);

            this.AssertServices();
        }

        [Test]
        public void AddFacilities_RegisterFacilities_AllFacilitiesAreRegistered()
        {
            this.WindsorInstaller.AddFacilities(this.WindsorContainer);
            var addedFacilities = this.WindsorContainer.Kernel.GetFacilities();
            var expectedFacilityTypes = new[]
            {
                typeof(StartableFacility), typeof(TypedFactoryFacility)
            };
            foreach(var facilityType in expectedFacilityTypes)
            {
                Assert.IsTrue(addedFacilities.Any(f => facilityType.IsInstanceOfType(f)));
            }
        }

        [Test]
        public void RegisterFactories_RegisterFactories_AllFactoriesAreRegistered()
        {
            this.WindsorInstaller.Install(this.WindsorContainer,null);
            var investigationFactory = this.WindsorContainer.Resolve<IInvestigationFactory>();
            var infestigationInfo = this.WindsorContainer.Resolve <InvestigationInfo>();
            var inestigation = investigationFactory.Create(infestigationInfo);
            Assert.IsNotNull(inestigation);
        }
        
        [Test]
        public void Install_ResolveIInvestigationInfoLoader_ComponentResolved()
        {
            this.WindsorInstaller.Install(this.WindsorContainer, null);
            var component = this.WindsorContainer.Resolve<IInvestigationInfoLoader>();
            Assert.IsNotNull(component);
        }

        protected void AssertServices()
        {
            Assert.IsTrue(this.ContainsComponent(typeof(BgTasksManagerService), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(NavigationService), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(LogService), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(ISystemServices), typeof(SystemServices), LifestyleType.Singleton));
        }

        protected void AssertViews()
        {
            Assert.IsTrue(this.ContainsComponent(typeof(IInvestigationExplorerView), typeof(InvestigationExplorerView), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(IWorkspacesManagerView), typeof(WorkspacesManagerView), LifestyleType.Singleton));
            Assert.IsTrue(this.ContainsComponent(typeof(IApplicationView), typeof(ApplicationView), LifestyleType.Singleton));
        }
    }
}