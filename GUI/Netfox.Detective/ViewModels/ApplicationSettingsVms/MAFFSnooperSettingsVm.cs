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
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    /// <summary>
    /// ViewModel class holds configuration data fropm MaffSnooperSettings view
    /// </summary>
    /// <seealso cref="SettingsBaseVm" />
    public class MAFFSnooperSettingsVm : SettingsBaseVm
    {

        public override string HeaderText => "MAFFSettings";
        private const int KeepAlive = 10000;

        public static bool StaticGenerateSnapshots { get; set; }
        public static long StaticSnapshotsTimeSeparator { get; set; }
        public static bool StaticObjectRewrite { get; set; }
        public static bool StaticTurnOffConfigurationFile { get; set; }
        [SafeForDependencyAnalysis]
         public bool GenerateSnapshots
         {
            get => StaticGenerateSnapshots;
            set
             {
                 StaticGenerateSnapshots = value;
                 this.OnPropertyChanged("GenerateSnapshots");
             }
         }
         [SafeForDependencyAnalysis]
         public string SnapshotsTimeSeparator
         {
             get => StaticSnapshotsTimeSeparator.ToString();
             set
             {
                 long lValue;
                 Int64.TryParse(value, out lValue);
                 StaticSnapshotsTimeSeparator = (lValue > 0 && lValue < KeepAlive)? lValue : 500;
                 this.OnPropertyChanged("SnapshotsTimeSeparator");
             }
         }

         [SafeForDependencyAnalysis]
         public bool ObjectRewrite
         {
             get => StaticObjectRewrite;
             set
             {
                 StaticObjectRewrite = value;
                 this.OnPropertyChanged("ObjectRewrite");
             }
         }

         [SafeForDependencyAnalysis]
         public bool TurnOffConfigurationFile
         {
             get => StaticTurnOffConfigurationFile;
             set
             {
                StaticTurnOffConfigurationFile = value;
                 this.OnPropertyChanged("TurnOffConfigurationFile");
             }
         }

        /// <summary>
        /// Initializes a new instance of the <see cref="MAFFSnooperSettingsVm"/> class.
        /// </summary>
        /// <param name="applicationOrAppWindsorContainer">The application or application windsor container.</param>
        public MAFFSnooperSettingsVm(WindsorContainer applicationOrAppWindsorContainer) : base(applicationOrAppWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IMAFFSettingsTab>());

            StaticGenerateSnapshots = true;
            StaticObjectRewrite = true;
            StaticTurnOffConfigurationFile = true;
            StaticSnapshotsTimeSeparator = 500;
        }
    }
}
