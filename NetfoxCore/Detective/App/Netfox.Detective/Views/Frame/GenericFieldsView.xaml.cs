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

using System.Collections.Generic;
using System.Windows.Controls;
using Netfox.Detective.ViewModels.Frame;
using Telerik.Windows;
using Telerik.Windows.Controls;

namespace Netfox.Detective.Views.Frame
{
    /// <summary>
    ///     Interaction logic for GenericFilelds.xaml
    /// </summary>
    public partial class GenericFieldsView : PacketViewBase
    {
        public ExpandState expandState = new ExpandState();
        private List<string> expandedNodes = new List<string>();
        private string treeViewState = string.Empty;

        public GenericFieldsView()
        {
            /* DataItemProperty = DependencyProperty.Register("Frame", typeof (System.Windows.Controls.Frame), typeof (GenericFieldsView),
                                                           new PropertyMetadata(null,
                                                                                new PropertyChangedCallback(
                                                                                    base.OnDataItemChanged)));
            */
            this.InitializeComponent();
        }

        public FrameVm FrameVm { get; set; }

        public void ExpandNode(ItemsControl control, string id)
        {
            if(control != null)
            {
                for(var i = 0; i < control.Items.Count; i++)
                {
                    var viewModel = control.Items[i] as GenericFiledVm;

                    if(viewModel != null && this.expandedNodes.Contains(viewModel.Id))
                    {
                        this.BringIndexIntoView(control, i);
                        this.TreeView.UpdateLayout();

                        var container = control.ItemContainerGenerator.ContainerFromIndex(i) as RadTreeViewItem;

                        if(container != null)
                        {
                            container.IsExpanded = true;
                            container.UpdateLayout();
                            this.ExpandNode(container, id);
                        }
                    }
                }
            }
        }

        public void UpdateExpandState() { foreach(var expandedNode in this.expandedNodes) { this.ExpandNode(this.TreeView, expandedNode); } }

        private void BringIndexIntoView(ItemsControl itemsControl, int index)
        {
            var treeView = itemsControl as RadTreeView;
            if(treeView != null) { treeView.BringIndexIntoView(index); }
            var treeViewItem = itemsControl as RadTreeViewItem;
            if(treeViewItem != null) { treeViewItem.BringIndexIntoView(index); }
        }

        private void TreeView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fieldVm = this.TreeView.SelectedItem as GenericFiledVm;
            if(fieldVm != null)
            {
                if(this.FrameVm != null)
                {
                    this.FrameVm.SelectedOffset = fieldVm.Offset;
                    this.FrameVm.SelectedLength = fieldVm.Length;
                }
            }
        }

        private void treeViewCollapsed(object sender, RadRoutedEventArgs e)
        {
            var treeViewItem = e.OriginalSource as RadTreeViewItem;

            var collapsedItem = treeViewItem.Item as GenericFiledVm;

            if(collapsedItem != null) { if(this.expandedNodes.Contains(collapsedItem.Id)) { this.expandedNodes.Remove(collapsedItem.Id); } }
        }

        private void treeViewExpanded(object sender, RadRoutedEventArgs e)
        {
            var treeViewItem = e.OriginalSource as RadTreeViewItem;

            var expandedItem = treeViewItem.Item as GenericFiledVm;

            if(expandedItem != null) { if(!this.expandedNodes.Contains(expandedItem.Id)) { this.expandedNodes.Add(expandedItem.Id); } }
        }

        public class ExpandState
        {
            public Dictionary<int, ExpandState> ChildStates = new Dictionary<int, ExpandState>();
            public bool Exapnded;
        }
    }
}