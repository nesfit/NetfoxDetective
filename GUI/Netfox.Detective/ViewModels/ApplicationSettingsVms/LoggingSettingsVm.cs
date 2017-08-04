// The MIT License (MIT)
//  
// Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
// Author(s):
// Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Messages;
using Netfox.Core.Messages.Base;
using Netfox.GUI.Detective.Core;
using PostSharp.Patterns.Model;

namespace Netfox.GUI.Detective.ViewModels.ApplicationSettingsVms
{
    public class LoggingSettingsVm : SettingsBaseVm
    {
        public override string HeaderText
        {
            get { return "Logging"; }
        }

       private string _logingDirectory = ApplicationSettings.String("AppDataLogPath");

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
        public ICommand ChangeCommand {
            get { return new RelayCommand(this.SettingsChanged);}
        }

        private void SettingsChanged()
        {
            ApplicationSettings.SetValue("AppDataLogPath", this._logingDirectory);
            LogServiceMessage.SendLogServiceMessage(this.LogingDirectory, LogServiceMessage.MessageType.LogSettingPathChanged);
            SystemMessage.SendSystemMessage(SystemMessage.Type.Info, "Log Directory", "Log directory changed: " + this.LogingDirectory);
        }

        [SafeForDependencyAnalysis]
        public ICommand OpenDirectoryCommand
        {
            get { return new RelayCommand(this.OpenDirectory);}
        }

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

        [SafeForDependencyAnalysis]
        public bool LogInfos
        {
            get { return this.Log("Info"); }
            set
            {
                this.ToLog("Info", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool LogWarning
        {
            get { return this.Log("Warning"); }
            set
            {
                this.ToLog("Warning", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool LogError
        {
            get { return this.Log("Error"); }
            set
            {
                this.ToLog("Error", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool LogCritical
        {
            get { return this.Log("Critical"); }
            set
            {
                this.ToLog("Critical", value);
                this.OnPropertyChanged();
            }
        }

        protected override void SettingsOnSettingsChanged(string propertyName, object value) { if (propertyName == "LogPath") { this.OnPropertyChanged("LogingDirectory"); } }
        private bool Log(string type) { return ApplicationSettings.String("ToLogMessages").Contains(type); }

        private void ToLog(string type, bool value)
        {
            var current = ApplicationSettings.String("ToLogMessages");
            var types = current.Split(';').ToList();
            if (!value) { types.Remove(type); }
            else
            { types.Add(type); }
            current = String.Join(";", types);
            ApplicationSettings.SetValue("ToLogMessages", current);
        }

        public LoggingSettingsVm(WindsorContainer applicationOrAppWindsorContainer) : base(applicationOrAppWindsorContainer) {}
    }
}