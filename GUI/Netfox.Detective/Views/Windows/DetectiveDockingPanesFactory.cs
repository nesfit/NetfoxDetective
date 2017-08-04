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
using System.Linq;
using Netfox.Detective.Core;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.ViewModels;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace Netfox.Detective.Views.Windows
{
    /// <summary>
    ///     Used in ApplicationView and ExportContentExplorerView
    /// </summary>
    public class DetectiveDockingPanesFactory : DockingPanesFactory
    {
        protected override void AddPane(RadDocking radDocking, RadPane pane)
        {
            var paneViewModel = pane.DataContext as DetectivePaneViewModelBase;
            if(paneViewModel != null)
            {
                radDocking.PaneStateChange += this.RadDocking_PaneStateChange;
                RadPaneGroup group = null;
                switch(paneViewModel.DockPositionPosition)
                {
                    case DetectiveDockPosition.DockedDocument:
                        group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "DockedDocument") as RadPaneGroup;
                        @group?.Items.Add(pane);
                        return;
                    case DetectiveDockPosition.DockedRight:
                        group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "DockedRight") as RadPaneGroup;
                        @group?.Items.Add(pane);
                        return;
                    case DetectiveDockPosition.DockedBottom:
                        group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "DockedBottom") as RadPaneGroup;
                        @group?.Items.Add(pane);
                        return;
                    case DetectiveDockPosition.DockedLeft:
                        group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "DockedLeft") as RadPaneGroup;
                        @group?.Items.Add(pane);
                        return;
                    case DetectiveDockPosition.FloatingDockable:
                        var fdSplitContainer = radDocking.GeneratedItemsFactory.CreateSplitContainer();
                        group = radDocking.GeneratedItemsFactory.CreatePaneGroup();
                        fdSplitContainer.Items.Add(group);
                        group.Items.Add(pane);
                        radDocking.Items.Add(fdSplitContainer);
                        pane.MakeFloatingDockable();
                        return;
                    case DetectiveDockPosition.FloatingOnly:
                        var foSplitContainer = radDocking.GeneratedItemsFactory.CreateSplitContainer();
                        group = radDocking.GeneratedItemsFactory.CreatePaneGroup();
                        foSplitContainer.Items.Add(group);
                        group.Items.Add(pane);
                        radDocking.Items.Add(foSplitContainer);
                        pane.MakeFloatingOnly();
                        return;
                    case DetectiveDockPosition.DockedTop:
                    default:
                        return;
                }
            }
            base.AddPane(radDocking, pane);
        }

        protected override RadPane CreatePaneForItem(object item)
        {
            var viewModel = item as DetectivePaneViewModelBase;
            Debug.Assert(viewModel != null, "viewModel != null");

            var pane = new DetectivePane
            {
                DataContext = viewModel
            };
            RadDocking.SetSerializationTag(pane, viewModel.HeaderText);
            Debug.Assert(viewModel.View != null, "viewModel.View != null");
            pane.Content = viewModel.View;
            return pane;
        }

        protected override void RemovePane(RadPane pane)
        {
            pane.DataContext = null;
            pane.Content = null;
            pane.ClearValue(RadDocking.SerializationTagProperty);
            pane.RemoveFromParent();
        }

        private void ChangeDockingState(DetectivePaneViewModelBase vm, DetectiveDockPosition newDockingPosition)
        {
            if(vm.DockPositionPosition != newDockingPosition) { vm.DockPositionPosition = newDockingPosition; }
        }

        private void RadDocking_PaneStateChange(object sender, RadRoutedEventArgs e)
        {
            var detectivePane = e.Source as DetectivePane;
            var radPaneGroupTarget = detectivePane?.Parent as RadPaneGroup;
            var detectivePaneVm = detectivePane?.DataContext as DetectivePaneViewModelBase ?? (e.OriginalSource as DetectivePane)?.DataContext as DetectivePaneViewModelBase;
            if(detectivePaneVm == null) { return; }

            if(radPaneGroupTarget == null)
            {
                if(!(e.Source is RadDocking)) { return; }
                this.ChangeDockingState(detectivePaneVm, DetectiveDockPosition.FloatingDockable);
                return;
            }

            var documentGroupName = radPaneGroupTarget.Name;
            if(documentGroupName == string.Empty) {
                this.ChangeDockingState(detectivePaneVm, DetectiveDockPosition.DockedInNewGroup);
            }
            else
            {
                DetectiveDockPosition detectiveDockPosition;
                if(!Enum.TryParse(documentGroupName, out detectiveDockPosition)) { return; }
                this.ChangeDockingState(detectivePaneVm, detectiveDockPosition);
            }
        }
    }
}