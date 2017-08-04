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

using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using Castle.Core.Logging;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Properties;
using Netfox.Logger;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.ApplicationSettingsVms
{
    public class LoggingSettingsTabVm : SettingsBaseVm
    {
        private string _logingDirectory = NetfoxSettings.Default.AppDataLogPath;

        public LoggingSettingsTabVm(WindsorContainer applicationWindsorContainer, NetfoxLogger netfoxLogger) : base(applicationWindsorContainer)
        {
            this.NetfoxLogger = netfoxLogger;
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<ILoggingSettingsTab>());
        }

        public override string HeaderText => "Logging";
        public NetfoxLogger NetfoxLogger { get; }

        [SafeForDependencyAnalysis]
        public string LogingDirectory
        {
            get { return this._logingDirectory; }
            set
            {
                this._logingDirectory = value;
                this.OnPropertyChanged();
            }
        }
        [SafeForDependencyAnalysis]
        public LoggerLevel BackgroundLoggingLevels
        {
            get { return this.NetfoxLogger.BackgroundLoggerLevel; }
            set { this.NetfoxLogger.BackgroundLoggerLevel = value; }
        }

        [SafeForDependencyAnalysis]
        public ICommand ChangeCommand => new RelayCommand(this.SettingsChanged);

        [SafeForDependencyAnalysis]
        public ICommand OpenDirectoryCommand => new RelayCommand(this.OpenDirectory);

        public void OpenDirectory()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if(result == DialogResult.OK)
            {
                this._logingDirectory = dialog.SelectedPath;
                this.LogingDirectory = this._logingDirectory;
            }
        }

        private void SettingsChanged()
        {
            NetfoxSettings.Default.AppDataLogPath = this.LogingDirectory;
            this.NetfoxLogger.ChangeLoggingDirectory(new DirectoryInfo(this.LogingDirectory));
            this.Logger?.Info($"Log directory changed: {this.LogingDirectory}");
        }
    }
}