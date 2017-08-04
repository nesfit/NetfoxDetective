// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Linq;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.ViewModels.ApplicationSettingsVms;
using Netfox.Detective.Views.SettingsTabs;

namespace Netfox.Detective.ViewModels.Windows
{
    public class SettingsWindowVm : DetectiveWindowViewModelBase
    {
        public SettingsWindowVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this.ViewType = typeof(ISettingsWindow);
            this.Tabs = new ConcurrentObservableCollection<SettingsBaseVm>();

            //var currentAssembly = Assembly.GetExecutingAssembly();
            //var viewsTypes = currentAssembly.GetTypes().Where(a => a.IsClass && !a.IsAbstract && a.IsPublic && !a.IsInterface && typeof(SettingsTabBase).IsAssignableFrom(a));

            //foreach(var viewType in viewsTypes)
            //{
            //    try
            //    {
            //        var detectiveView = (DetectiveUserControlViewBase) Activator.CreateInstance(viewType);
            //        var tab = new SettingsTab(detectiveView);
            //        this.Tabs.Add(tab);
            //    }
            //    catch(Exception) {
            //        Debugger.Break();
            //    }
            //    // SettingsTabsHost.Items.Add(tab);
            //}
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var currentAssembly in assemblies)
            {
                var paneViews = currentAssembly.GetTypes().Where(a => a.IsClass && a.IsPublic && !a.IsInterface && !a.IsAbstract && typeof(SettingsTabBase).IsAssignableFrom(a));

                foreach(var paneViewType in paneViews)
                {
                    try
                    {
                        var autoRegisterInterfaces = paneViewType.GetInterfaces().Where(i => i.GetInterfaces().Contains(typeof(IAutoRegisterView)));
                        foreach(var autoRegisterInterface in autoRegisterInterfaces)
                        {
                            var types = new List<Type>
                            {
                                paneViewType,
                                autoRegisterInterface
                            };
                            this.ApplicationOrInvestigationWindsorContainer.Register(Component.For(types).ImplementedBy(paneViewType).LifestyleSingleton());
                        }
                    }
                    catch(ComponentRegistrationException ex)
                    {
                        this.ApplicationOrInvestigationWindsorContainer.Resolve<ILogger>().Error(ex.ToString);

                        Debugger.Break();
                    }
                }

                var paneVms = currentAssembly.GetTypes().Where(a => a.IsClass && a.IsPublic && !a.IsInterface && !a.IsAbstract && typeof(SettingsBaseVm).IsAssignableFrom(a));

                foreach(var paneVm in paneVms)
                {
                    try { this.Tabs.Add(this.ApplicationOrInvestigationWindsorContainer.Resolve(paneVm) as SettingsBaseVm); }
                    catch(ComponentRegistrationException ex)
                    {
                        this.ApplicationOrInvestigationWindsorContainer.Resolve<ILogger>().Error(ex.ToString);

                        Debugger.Break();
                    }
                }
            }
        }

        public override string HeaderText => "Global application settings";

        public ConcurrentObservableCollection<SettingsBaseVm> Tabs { get; }
    }
}