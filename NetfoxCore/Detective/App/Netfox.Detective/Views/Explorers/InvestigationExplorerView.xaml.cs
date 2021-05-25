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

using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.Windows;
using System.Windows.Controls;
using Castle.Core;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.ViewModels.Investigations;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using Telerik.Windows;
using Telerik.Windows.DragDrop;
using DragEventArgs = Telerik.Windows.DragDrop.DragEventArgs;

namespace Netfox.Detective.Views.Explorers
{
    /// <summary>
    ///     Interaction logic for InvestigationView.xaml
    /// </summary>
    public partial class InvestigationExplorerView : DetectiveApplicationPaneViewBase, IInvestigationExplorerView
    {
        public InvestigationExplorerView()
        {
            this.InitializeComponent();

            this.RemoveExportgroupButton.IsEnabled = false;
            //this.AddToExportButton.IsEnabled = false;

            //DragDropManager.AddDropHandler(this.InvestigationTreeView, this.DropHandler, true);
            //DragDropManager.AddDragDropCompletedHandler(this.InvestigationTreeView, this.OnDragDropCompleted, true);

            //this.InvestigationTreeView.SelectedItems.CollectionChanged += this.SelectedItems_CollectionChanged;

            //base.Visibility = Visibility.Visible;
            //base.ViewName = DefaultName;

            //    Task.Factory.StartNew(() => Messenger.Default.Register<InvestigationMessage>(this, InvestigationActionHandler));
        }

        //private const string DefaultName = "Investigation explorer";
        [DoNotWire]
        public InvestigationVm Investigation { get; set; }

        protected override void RefreshView(object s, PropertyChangedEventArgs e)
        {
            // InvestigationExplorerVm explorer = s as InvestigationExplorerVm;
            // if (explorer == null) return;
            // InvestigationVm vm = explorer.InvestigationVm;
            //
            // CapturesRoot.ItemsSource = vm?.Captures;
            // ExportsRoot.ItemsSource = vm?.ExportGroups;
        }
        
        private void AddCaptureToExportClick(object sender, RadRoutedEventArgs e)
        {
            //todo
            //var frameworkElement = e.OriginalSource as FrameworkElement;
            //if (frameworkElement == null)
            //    return;

            //var capture = frameworkElement.DataContext as CaptureVm;

            //if (capture != null)
            //{
            //    //this.Investigation.AddCaptureToExport(capture);
            //    ConversationMessage.SendConversationMessage(capture.CurrentConversation, ConversationMessage.MessageType.AddConversationToExport, true);
            //}
        }

        private void AddToExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            //todo
            //if (_context != null)
            //{
            //    var selectedCaptures = InvestigationTreeView.SelectedItems.OfType<CaptureVm>().ToArray();

            //    foreach (var selectedItem in selectedCaptures)
            //    {
            //        _context.AddCaptureToExport(selectedItem);
            //    }
            //    ConversationMessage.SendConversationMessage(null, ConversationMessage.MessageType.AddConversationToExport, true);
            //}
        }

        private void ExportsRoot_OnLostFocus(dynamic sender, RoutedEventArgs e)
        {
            //ExportGroupVm exportGroup = sender.Owner;
        }

        private void InvestigationTreeView_OnItemDoubleClick(object sender, RadRoutedEventArgs e)
        {
            //todo  - DONE
            //var selected = e.OriginalSource;
            //if (selected != null && _lastSelected != null)
            //{
            //    var selectedType = _lastSelected.GetType();
            //    if (selectedType == typeof(CaptureVm))
            //    {
            //        BringToFrontMessage.SendBringToFrontMessage("CaptureView");
            //    }
            //    else if (selectedType == typeof(ConversationVm))
            //    {
            //        BringToFrontMessage.SendBringToFrontMessage("ConversationDetailView");
            //    }
            //    else if (selectedType == typeof(ExportGroupVm))
            //    {
            //        BringToFrontMessage.SendBringToFrontMessage("ExportOverviewView");
            //    }
            //    else if (typeof(ExportVm).IsAssignableFrom(selectedType))
            //    {
            //        BringToFrontMessage.SendBringToFrontMessage("ExportResultView");
            //    }
            //    else if (selectedType == typeof(Netfox.Detective.Models.Base.Frame))
            //    {
            //        BringToFrontMessage.SendBringToFrontMessage("FrameContentView");
            //    }
            //    else if (selectedType == typeof(ConversationsGroupVm))
            //    {
            //        BringToFrontMessage.SendBringToFrontMessage("ConversationsDetailView");
            //    }
            //}
        }

        private void RemoveCaptureButton_OnClick(object sender, RoutedEventArgs e)
        {
            //todo
            //if (_context != null)
            //{
            //    bool? dialogResult = null;
            //    RadWindow.Confirm(new DialogParameters()
            //    {
            //        Content = "Do you realy want to remove selected captures ?",
            //        Closed = (confirmDialog, eventArgs) =>
            //        {
            //            dialogResult = eventArgs.DialogResult;
            //        }
            //    });

            //    if (dialogResult != null && (bool)dialogResult)
            //    {
            //        var selectedCaptures = InvestigationTreeView.SelectedItems.OfType<CaptureVm>().ToArray();
            //        foreach (var selectedItem in selectedCaptures)
            //        {
            //            _context.RemoveCaptureAsync(selectedItem);
            //        }
            //    }
            //}
        }

        private void RemoveCaptureClick(object sender, RadRoutedEventArgs e)
        {
            //todo - DONE
            //var frameworkElement = e.OriginalSource as FrameworkElement;
            //if (frameworkElement == null)
            //    return;

            //var capture = frameworkElement.DataContext as CaptureVm;

            //if (Context != null && capture != null)
            //{
            //    Context.RemoveCaptureAsync(capture);
            //}
        }

        private void RemoveConvGroupClick(object sender, RadRoutedEventArgs e)
        {
            //todo
            //var frameworkElement = e.OriginalSource as FrameworkElement;
            //if (frameworkElement == null)
            //    return;

            //var convGroup = frameworkElement.DataContext as ConversationsGroupVm;

            //if (Context != null && convGroup != null)
            //{
            //    Context.RemoveConversationsGroup(convGroup);
            //}
        }

        private void RemoveExportgroupButton_OnClick(object sender, RoutedEventArgs e)
        {
            //todo
            //if (_context != null)
            //{
            //    bool? dialogResult = null;
            //    RadWindow.Confirm(new DialogParameters()
            //    {
            //        Content = "Do you realy want to remove selected export groups ?",
            //        Closed = (confirmDialog, eventArgs) =>
            //        {
            //            dialogResult = eventArgs.DialogResult;
            //        }
            //    });

            //    if (dialogResult != null && (bool)dialogResult)
            //    {
            //        var selectedEG = InvestigationTreeView.SelectedItems.OfType<ExportGroupVm>().ToArray();
            //        foreach (var selectedItem in selectedEG)
            //        {
            //            _context.RemoveExportGroup(selectedItem);
            //        }
            //    }
            //}
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //todo
            //if (InvestigationTreeView.SelectedItems != null &&
            //    InvestigationTreeView.SelectedItems.Any(item => item.GetType() == typeof(CaptureVm)))
            //{
            //    RemoveCaptureButton.IsEnabled = true;
            //    AddToExportButton.IsEnabled = true;
            //}
            //else
            //{
            //    RemoveCaptureButton.IsEnabled = false;
            //    AddToExportButton.IsEnabled = false;
            //}


            //if (InvestigationTreeView.SelectedItems != null &&
            //        InvestigationTreeView.SelectedItems.Any(item => item.GetType() == typeof(ExportGroupVm)))
            //    RemoveExportgroupButton.IsEnabled = true;
            //else
            //    RemoveExportgroupButton.IsEnabled = false;
        }

        private void TreeView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //todo - almost Done - ExportRoo!!!
            //if (e.AddedItems.Count > 0)
            //{
            //    var selected = e.AddedItems[0];
            //    _lastSelected = selected;
            //    if (selected != null)
            //    {
            //        if (selected.GetType() == typeof(CaptureVm))
            //        {
            //            var selectedCapture = (CaptureVm)selected;
            //            if (selectedCapture != _context.CurrentCapture)
            //            {
            //                _context.CurrentCapture = selectedCapture;
            //                _context.CurrentCapture.CurrentConversation = null;
            //            }
            //            else
            //            {
            //                _context.CurrentCapture = selectedCapture;
            //            }
            //        }
            //        if (selected.GetType() == typeof(ConversationsGroupVm))
            //        {
            //            var selectedConversationsGroup = (ConversationsGroupVm)selected;
            //            if (selectedConversationsGroup != _context.CurrentConversationsGroupVm)
            //            {
            //                _context.CurrentConversationsGroupVm = selectedConversationsGroup;
            //                _context.CurrentConversationsGroupVm.CurrentConversation = null;
            //            }
            //            else
            //            {
            //                _context.CurrentConversationsGroupVm = selectedConversationsGroup;
            //            }
            //        }
            //        else if (selected.GetType() == typeof(ConversationVm))
            //        {

            //            var selectedConversation = (ConversationVm)selected;
            //            _context.SetCurrentCaptureByConversation(selectedConversation);
            //            if (_context.CurrentCapture != null)
            //            {
            //                _context.CurrentCapture.CurrentConversation = selectedConversation;
            //            }
            //            else
            //            {
            //                _context.SetCurrentConversationsGroupByConversation(selectedConversation);
            //                if (_context.CurrentCapture != null)
            //                {
            //                    _context.CurrentCapture.CurrentConversation = selectedConversation;
            //                }
            //            }
            //        }
            //        else if (selected.GetType() == typeof(ExportGroupVm))
            //        {
            //            var selectedExportGroup = (ExportGroupVm)selected;
            //            _context.CurrentExportGroup = selectedExportGroup;
            //        }
            //        else if (selected is ExportVm)
            //        {
            //            var selectedExportResult = (ExportVm)selected;
            //            var newCurrentGroup = _context.ExportGroupByExportResult(selectedExportResult);
            //            if (newCurrentGroup != null)
            //            {
            //                _context.CurrentExportGroup = newCurrentGroup;
            //                if (_context.CurrentExportGroup != null)
            //                {
            //                    _context.CurrentExportGroup.SelectedExportResult = selectedExportResult;
            //                }
            //            }
            //        }
            //        else if (selected.Equals(ExportsRoot))
            //        {

            //        }
            //    }
            //}
        }

        #region Drag and drop
        private void OnDragDropCompleted(object sender, DragDropCompletedEventArgs e)
        {
            //todo
            //var options =
            //    DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            //if (options != null)
            //{
            //    options.DropAction = DropAction.None;

            //    if (options.DropTargetItem != null)
            //    {
            //        var dropPosition = options.DropPosition;

            //        ExportGroupVm newParentGroupVm = null;
            //        var newParentInvestigation = false;
            //        var targetDropItem = options.DropTargetItem;

            //        switch (dropPosition)
            //        {
            //            case DropPosition.After:
            //            case DropPosition.Before:

            //                if (targetDropItem.DataContext.GetType() == typeof(InvestigationVm))
            //                    newParentInvestigation = true;
            //                else
            //                    newParentGroupVm = targetDropItem.ParentItem.DataContext as ExportGroupVm;

            //                break;
            //            case DropPosition.Inside:

            //                if (targetDropItem.DataContext.GetType() == typeof(InvestigationVm))
            //                    newParentInvestigation = true;
            //                else
            //                    newParentGroupVm = targetDropItem.DataContext as ExportGroupVm;

            //                break;
            //        }


            //        foreach (var draggedItem in options.DraggedItems)
            //        {

            //            if (draggedItem.GetType() == typeof(ExportGroupVm))
            //            {
            //                var draggedGroup = draggedItem as ExportGroupVm;
            //                if (draggedGroup != null)
            //                {
            //                    if (newParentInvestigation)
            //                        draggedGroup.MoveGroupToParentId(_context.Investigation.Id);
            //                    else
            //                        draggedGroup.MoveGroup(newParentGroupVm);
            //                }
            //            }
            //            else if (draggedItem.GetType() == typeof(ExportVm))
            //            {
            //                if (!newParentInvestigation)
            //                {
            //                    var draggedResult = draggedItem as ExportVm;
            //                    if (draggedResult != null)
            //                    {
            //                        draggedResult.MoveResult(newParentGroupVm);
            //                    }
            //                }

            //            }

            //        }

            //    }
            //}

            //e.Handled = true;
        }

        private void DropHandler(object sender, DragEventArgs e)
        {
            //todo
            //var options = DragDropPayloadManager.GetDataFromObject(e.Data, TreeViewDragDropOptions.Key) as TreeViewDragDropOptions;
            //if (options != null)
            //{
            //    options.DropAction = DropAction.None;
            //}

            //e.Handled = true;
        }
        #endregion

        private void RadTreeViewItem_Selected(object sender, RadRoutedEventArgs e)
        {

        }
    }
}