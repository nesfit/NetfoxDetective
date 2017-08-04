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
using System.Windows;
using Castle.Core.Logging;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using Netfox.Core.Interfaces;
using Netfox.Detective.ViewModels.Interfaces;
using Netfox.Detective.Views.Windows;

namespace Netfox.Detective.Views.Services
{
    public class SystemServices : ISystemServices, IDetectiveService
    {
        public string OpenFileDialog(string defaultPath, string defaultExt, string filter)
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = defaultPath,
                DefaultExt = defaultExt,
                Filter = filter
            };

            var result = dlg.ShowDialog();

            if(result == true) { return dlg.FileName; }

            return String.Empty;
        }

        public string[] OpenFilesDialog(string defaultPath, string defaultExt, string filter)
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = defaultPath,
                DefaultExt = defaultExt,
                Filter = filter,
                Multiselect = true
            };

            var result = dlg.ShowDialog();

            if(result != true) { return new string[0]; }
            return dlg.FileNames;
        }


        public string SaveFileDialog(string defaultPath, string defaultExt, string filter)
        {
            var dlg = new SaveFileDialog
            {
                InitialDirectory = defaultPath,
                DefaultExt = defaultExt,
                Filter = filter
            };

            var result = dlg.ShowDialog();

            if(result == true) { return dlg.FileName; }

            return String.Empty;
        }

        public MessageBoxResult ShowMessageBox(
            string strMessage,
            string strCaption = null,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Warning)
        {
            //return MessageBox.Show(strMessage, strMessage, button, image);
            //http://www.codeproject.com/Articles/164934/RadMessageBox-for-the-Telerik-WPF-Controls
            //var w = new Window();
            //w.Activate();
            //w.Show();

            var w = ServiceLocator.Current.GetInstance<ApplicationView>();
            return RadMessageBox.Show(w, strMessage, strCaption, button, image);
        }

        #region Implementation of IDetectiveService
        public ILogger Logger { get; set; }
        #endregion
    }
}