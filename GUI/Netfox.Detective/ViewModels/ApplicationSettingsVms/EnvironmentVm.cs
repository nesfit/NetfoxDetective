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
using Castle.Windsor;
using Netfox.GUI.Detective.Core;
using PostSharp.Patterns.Model;

namespace Netfox.GUI.Detective.ViewModels.ApplicationSettingsVms
{
    public class EnvironmentVm : SettingsBaseVm
    {
        public override string HeaderText
        {
            get { return "Environment"; }
        }

        [SafeForDependencyAnalysis]
        public bool BgInfos
        {
            get { return this.Log("BackgroundNotifications", "Info"); }
            set
            {
                this.ToLog("BackgroundNotifications", "Info", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool BgWarning
        {
            get { return this.Log("BackgroundNotifications", "Warning"); }
            set
            {
                this.ToLog("BackgroundNotifications", "Warning", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool BgError
        {
            get { return this.Log("BackgroundNotifications", "Error"); }
            set
            {
                this.ToLog("BackgroundNotifications", "Error", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool BgCritical
        {
            get { return this.Log("BackgroundNotifications", "Critical"); }
            set
            {
                this.ToLog("BackgroundNotifications", "Critical", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool ExInfos
        {
            get { return this.Log("ExplicitNotifications", "Info"); }
            set
            {
                this.ToLog("ExplicitNotifications", "Info", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool ExWarning
        {
            get { return this.Log("ExplicitNotifications", "Warning"); }
            set
            {
                this.ToLog("ExplicitNotifications", "Warning", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool ExError
        {
            get { return this.Log("ExplicitNotifications", "Error"); }
            set
            {
                this.ToLog("ExplicitNotifications", "Error", value);
                this.OnPropertyChanged();
            }
        }

        [SafeForDependencyAnalysis]
        public bool ExCritical
        {
            get { return this.Log("ExplicitNotifications", "Critical"); }
            set
            {
                this.ToLog("ExplicitNotifications", "Critical", value);
                this.OnPropertyChanged();
            }
        }

        protected override void SettingsOnSettingsChanged(string propertyName, object value) { }
        private bool Log(string setting, string type) { return ApplicationSettings.String(setting).Contains(type); }

        private void ToLog(string setting, string type, bool value)
        {
            var current = ApplicationSettings.String(setting);
            var types = current.Split(';').ToList();
            if(!value) { types.Remove(type); }
            else
            { types.Add(type); }
            current = String.Join(";", types);
            ApplicationSettings.SetValue(setting, current);
        }

        public EnvironmentVm(WindsorContainer applicationOrAppWindsorContainer) : base(applicationOrAppWindsorContainer) {}
    }
}