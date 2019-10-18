// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using System.Windows.Controls;
using Netfox.Detective.Views;
using Netfox.SnooperMAFF.Interfaces;
using Netfox.SnooperMAFF.Models.Archives;
using Netfox.SnooperMAFF.WPF.View.Events;
using Netfox.SnooperMAFF.WPF.ViewModels;

namespace Netfox.SnooperMAFF.WPF.View
{
    /// <summary>
    /// Interaction logic for MAFFExportsView.xaml
    /// </summary>
    public partial class MAFFExportsView : DetectiveExportDetailPaneViewBase, IMAFFExportsView
    {
        public MAFFExportsView()
        {
            this.InitializeComponent();
        }

        private Archive GetArchiveFromViewBaseObject(object sender)
        {
            var window = sender as DetectiveExportDetailPaneViewBase;
            var dataContext = window?.DataContext as SnooperMAFFViewModel;
            var dataExport = dataContext?.ExportedArchive;
            var archive = dataExport?.Archive;
            return archive;
        }

        private SnooperMAFFViewModel GetDatacontextFromBaseObject(object sender)
        {
            var button = sender as Button;
            return button?.DataContext as SnooperMAFFViewModel;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var archive = this.GetArchiveFromViewBaseObject(sender);
            if (archive == null) { return; }

            DecompressArchiveEvent.GetInstance().LoadZipArchive(archive, "SnooperMAFFViewModel");
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var archive = this.GetArchiveFromViewBaseObject(sender);
            if (archive == null) { return; }

            DecompressArchiveEvent.GetInstance().UnLoadZipArchive(archive, "SnooperMAFFViewModel");
        }

        private void DoubleClickEventOnFilename(object sender, EventArgs e)
        {
            var dataContext = this.GetDatacontextFromBaseObject(sender);
            if (dataContext == null) { return; }

            OpenFileEvent.OpenEvent(dataContext.SelectedObject);
        }

        private void PreviousPartAfterClick(object sender, RoutedEventArgs e)
        {
            var dataContext = this.GetDatacontextFromBaseObject(sender);
            if(dataContext == null) { return; }
            CurrentPartEvent.GetInstance().GetPreviousPart();
        }

        private void NextPartAfterClick(object sender, RoutedEventArgs e)
        {
            var dataContext = this.GetDatacontextFromBaseObject(sender);
            if (dataContext == null) { return; }
            CurrentPartEvent.GetInstance().GetNextPart();
        }
    }
}
