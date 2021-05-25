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

using System.Linq;
using Netfox.Core.Interfaces.Views;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace Netfox.Detective.Views.Exports
{
    /// <summary>
    ///     Interaction logic for ExportContentView.xaml
    /// </summary>
    public partial class ExportContentExplorerView : DetectiveApplicationPaneViewBase, IExportContentExplorerView
    {
        public ExportContentExplorerView()
        {
            this.InitializeComponent();
        }

        private void OnClose(object sender, StateChangeEventArgs e)
        {
            //todo implement hide
            //var documents = e.Panes.Select(p => p.DataContext).OfType<PanelBarPanel>().Where(vm => vm.IsDocument).ToList();
            //foreach (var document in documents)
            //{
            //    ((ApplicationShell)this.DataContext).ViewPanesVMs.Remove(document);
            //}
        }

        private void OnPreviewShowCompass(object sender, PreviewShowCompassEventArgs e)
        {
            var isRootCompass = e.Compass is RootCompass;
            var splitContainer = e.DraggedElement as RadSplitContainer;
            if(splitContainer != null)
            {
                var isDraggingDocument = splitContainer.EnumeratePanes().Any(p => p is RadDocumentPane);
                var isTargetDocument = e.TargetGroup == null || e.TargetGroup.EnumeratePanes().Any(p => p is RadDocumentPane);
                if(isDraggingDocument) { e.Canceled = isRootCompass || !isTargetDocument; }
                else
                { e.Canceled = !isRootCompass && isTargetDocument; }
            }
        }
    }
}