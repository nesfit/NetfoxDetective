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

using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Properties;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    public class StartupSettingsVm : SettingsBaseVm
    {
        public StartupSettingsVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IStartUpSettingsTab>());
        }

        public override string HeaderText => "Startup";

        [SafeForDependencyAnalysis]
        public bool AutoLoadLastSession
        {
            get { return NetfoxSettings.Default.AutoLoadLastSession; }
            set
            {
                NetfoxSettings.Default.AutoLoadLastSession = value;
                this.OnPropertyChanged();
            }
        }

    }
}