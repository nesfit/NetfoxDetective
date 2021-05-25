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
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports.Detail;
using Netfox.Detective.ViewModelsDataEntity.Exports.ModelWrappers;

namespace Netfox.Detective.Views.Exports.ExportObjectDetailViews.Emails
{
    /// <summary>
    ///     Interaction logic for EmailDetailView.xaml
    /// </summary>
    public partial class EmailDetailView : DetectiveExportDetailPaneViewBase, IEmailDetailView
    {
        public EmailDetailView()
        {
            this.InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedPart = this.AattachmentsListBox.SelectedItem as MimePartVm;
            if (selectedPart != null)
            {
                if (File.Exists(selectedPart.FilePath))
                {
                    var explorer = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
                    Process.Start(explorer, selectedPart.FilePath);
                }
            }
        }
    }
}