// Copyright (c) 2017 Martin Vondracek
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
    public class EncapsulationSettingsVm : SettingsBaseVm
    {
        public EncapsulationSettingsVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IEncapsulationSettingsTab>());
        }

        public override string HeaderText => "Encapsulation";

        [SafeForDependencyAnalysis]
        public bool DecapsulateGseOverUdp
        {
            get => NetfoxSettings.Default.DecapsulateGseOverUdp;
            set
            {
                NetfoxSettings.Default.DecapsulateGseOverUdp = value;
                this.OnPropertyChanged();
            }
        }

    }
}