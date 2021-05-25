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

using System.Windows;
using System.Windows.Controls;
using Netfox.Detective.Views;
using Netfox.Snoopers.SnooperMAFF.Interfaces;
using Netfox.Snoopers.SnooperMAFF.Models.Archives;
using Netfox.Snoopers.SnooperMAFF.WPF.View.Events;
using Netfox.Snoopers.SnooperMAFF.WPF.ViewModels;

namespace Netfox.Snoopers.SnooperMAFF.WPF.View
{
    /// <summary>
    /// Interaction logic for MAFFExportsViewVisualization.xaml
    /// </summary>
    public partial class MAFFExportsViewVisualization : DetectiveExportDetailPaneViewBase, IMAFFExportsViewVisualization
    {
        public MAFFExportsViewVisualization()
        {
            this.InitializeComponent();
        }

        private Archive GetArchiveFromViewBaseObject(object sender)
        {
            var window = sender as DetectiveExportDetailPaneViewBase;
            var dataContext = window?.DataContext as SnooperMAFFViewVisualizationModel;
            var dataExport = dataContext?.ExportedArchive;
            var archive = dataExport?.Archive;
            return archive;
        }

        private SnooperMAFFViewVisualizationModel GetDatacontextFromBaseObject(object sender)
        {
            var button = sender as Button;
            var dataContext = button?.DataContext as SnooperMAFFViewVisualizationModel;
            return dataContext;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var archive = this.GetArchiveFromViewBaseObject(sender);
            if (archive == null) { return; }

            DecompressArchiveEvent.GetInstance().LoadZipArchive(archive, "SnooperMAFFViewVisualizationModel");

            //Safe call, We check not null values above
            ((SnooperMAFFViewVisualizationModel)((DetectiveExportDetailPaneViewBase)sender).DataContext).CallEventHandler();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            var archive = this.GetArchiveFromViewBaseObject(sender);
            if (archive == null) { return; }

            DecompressArchiveEvent.GetInstance().UnLoadZipArchive(archive, "SnooperMAFFViewVisualizationModel");
        }

        private void PreviousPartAfterClick(object sender, RoutedEventArgs e)
        {
            var dataContext = this.GetDatacontextFromBaseObject(sender);
            if (dataContext == null) { return; }
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
