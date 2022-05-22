﻿// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using System.Windows.Controls;
using System.Windows.Input;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.Views.Exports.ExportsControls
{
    /// <summary>
    ///     Interaction logic for FilesView.xaml
    /// </summary>
    public partial class CredentialsView : UserControl
    {
        public CredentialsView() { this.InitializeComponent(); }

        private void CredentialsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var credential = this.CredentialsDataGrid.SelectedItem as ExportResultHelper.Credential;

            if(credential != null && credential.ResultVm != null)
            {
                credential.ResultVm.SelectDataByDataObject(credential.DataVm, true);
               
                new DetectiveMvvmLightMessenger().AsyncSend(new SelectedExportResultMessage
                {
                    ExportVm = credential.ResultVm
                });
            }
        }
    }
}