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
using System.Reflection;
using System.Threading.Tasks;
using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Views;

namespace Netfox.Detective.ViewModels.Exports
{
    public class ExportContentExplorerVm : DetectiveApplicationPaneViewModelBase
    {
        //private ExportVm _context;
        //private List<KeyValuePair<IExportView, ViewAvialableQuality>> avialableExportDataViewPane = new List<KeyValuePair<IExportView, ViewAvialableQuality>>();
        //private List<KeyValuePair<IExportView, ViewAvialableQuality>> avialableExportExplorerViewPane = new List<KeyValuePair<IExportView, ViewAvialableQuality>>();
        //private List<KeyValuePair<IExportView, ViewAvialableQuality>> avialableExportResultViewPane = new List<KeyValuePair<IExportView, ViewAvialableQuality>>();
        //private List<IExportView> ExplorersViews = new List<IExportView>();
        //private List<IExportView> ExportViewsData = new List<IExportView>();
        //private List<IExportView> ExportViewsResults = new List<IExportView>();
        //private Dictionary<IExportView, RadPane> focusableViews = new Dictionary<IExportView, RadPane>();
        //private Dictionary<RadPane, IExportView> focusableViewsInv = new Dictionary<RadPane, IExportView>();
        //private bool isUserSelect;
        //private List<IExportView> lastSelectedExportExplorerViewPane = new List<IExportView>();
        //private List<IExportView> lastSelectedExportViewPane = new List<IExportView>();
        //private List<IExportView> registeredViews = new List<IExportView>();

        public ExportContentExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IExportContentExplorerView>());
            //Task.Factory.StartNew(() =>
            //{
            //    Messenger.Default.Register<ExportResultMessage>(this, this.ExportResultActionHandler);
            //    Messenger.Default.Register<ExportDataMessage>(this, this.ExportDataActionHandler);
            //    Messenger.Default.Register<BringToFrontExportViewMessage>(this, this.BringToFrontHandler);
            //});
            Parallel.Invoke(this.ScanAssembliesForeExportPaneViewAndRegisterThem);
        }

        public void RegisterDetectivePaneView(DetectiveExportDetailPaneViewBase detectiveApplicationDetailPaneView)
        {
            var viewModel = detectiveApplicationDetailPaneView.DataContext as DetectiveApplicationPaneViewModelBase;
            Debug.Assert(viewModel != null, "viewModel != null");
            viewModel.View = detectiveApplicationDetailPaneView;
            this.ExportPanesVMs.Add(viewModel);
        }

        //public ConcurrentObservableCollection<ExportPageDocumentPane> ExportViewsPanes { get; }
        //public ConcurrentObservableCollection<ExportExplorerDocumentPane> ExplorersViewsPanes { get; }
        //public ConcurrentObservableCollection<ExportOutputDocumentPane> BottomViewsPanes { get; }

        private void ScanAssembliesForeExportPaneViewAndRegisterThem()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var viewsTypes =
                currentAssembly.GetTypes()
                    .Where(
                        a =>
                            a.IsClass && a.IsPublic && !a.IsInterface && !a.IsAbstract && a.GetConstructor(Type.EmptyTypes) != null
                            && typeof(DetectiveExportDetailPaneViewBase).IsAssignableFrom(a));

            foreach(var viewType in viewsTypes)
            {
                var type = viewType;
                DispatcherHelper.RunAsync(() =>
                {
                    var detectiveView = (DetectiveExportDetailPaneViewBase) Activator.CreateInstance(type);
                    //skip DataEntityPaneViewModels
                    if(detectiveView.DataContext != null) { this.RegisterDetectivePaneView(detectiveView); }
                });
            }
        }

        //private void BringToFront(IExportView view)
        //{

        //    //if (this.focusableViews.ContainsKey(view))

        #region Overrides of DetectivePaneViewModelBase
        public override string HeaderText => "Export content explorer";
        public ConcurrentObservableCollection<DetectivePaneViewModelBase> ExportPanesVMs { get; } = new ConcurrentObservableCollection<DetectivePaneViewModelBase>();
        #endregion

        //    //{
        //    //    var toShowRadPane = this.focusableViews[view];

        //    //    if (this.ExportViewsPanes.Contains(toShowRadPane)) { this.MainExportPanesGroup.SelectedItem = toShowRadPane; }

        //    //    if (this.ExplorersViewsPanes.Contains(toShowRadPane)) { this.LeftExportPanesGroup.SelectedItem = toShowRadPane; }
        //    //}
        //}

        //private void BringToFrontHandler(BringToFrontExportViewMessage message)
        //{
        //    /*   if (focusableViews.ContainsKey(message.ExportView))
        //    {
        //        RadPane toShowRadPane = focusableViews[message.ExportView];

        //        if (ExportViewsPanes.Contains(toShowRadPane))
        //            MainExportPanesGroup.SelectedItem = toShowRadPane;

        //        if (ExplorersViewsPanes.Contains(toShowRadPane))
        //            LeftExportPanesGroup.SelectedItem = toShowRadPane;
        //    }*/

        //    /*  foreach (var exportPageDocumentPane in ExportViewsPanes)
        //    {

        //    }*/
        //}

        //private void ExportDataActionHandler(ExportDataMessage message)
        //{
        //    this.isUserSelect = false;

        //    this.avialableExportDataViewPane.Clear();

        //    foreach (var exportView in this.registeredViews) { exportView.ExportDataContext = message.ExportVm as ExportDataVm; }

        //    if (message.Type == ExportDataMessage.MessageType.DataSelected)
        //    {
        //        this.SelectExportPagePane(true);
        //    }
        //    else if (message.Type == ExportDataMessage.MessageType.DataSelectedUser) { this.SelectExportPagePane(false); }

        //    var t = new Thread(this.ResetUserSelect);
        //    t.Start();
        //}

        //private void ExportResultActionHandler(ExportResultMessage message)
        //{
        //    if (message.Type == ExportResultMessage.MessageType.ExportResultSelected)
        //    {
        //        this._context = message.ExportVm as ExportVm;

        //        //this.Visibility = (this._context != null ? Visibility.Visible : Visibility.Hidden);
        //        //this.ControlVisible = (this._context != null? Visibility.Visible : Visibility.Collapsed);

        //        this.avialableExportResultViewPane.Clear();
        //        this.avialableExportExplorerViewPane.Clear();
        //        this.avialableExportDataViewPane.Clear();

        //        this.isUserSelect = false;
        //        foreach (var exportView in this.registeredViews) { exportView.ExportDataContext = null; }

        //        if (this._context != null) { foreach (var exportView in this.registeredViews) { exportView.ExportResultContext = this._context; } }

        //        this.SelectExportPagePane(true);

        //        this.SelectExplorerPane();

        //        var t = new Thread(this.ResetUserSelect);
        //        t.Start();
        //    }
        //}

        //private void exportView_ViewAvialable(IExportView view, ViewAvialableQuality quality)
        //{
        //    switch(view.ViewType) {
        //        case ViewType.ExportedDataView:
        //            this.avialableExportDataViewPane.Add(new KeyValuePair<IExportView, ViewAvialableQuality>(view, quality));
        //            break;
        //        case ViewType.ExportResultView:
        //            this.avialableExportResultViewPane.Add(new KeyValuePair<IExportView, ViewAvialableQuality>(view, quality));
        //            break;
        //        case ViewType.ExportResultExplorer:
        //            this.avialableExportExplorerViewPane.Add(new KeyValuePair<IExportView, ViewAvialableQuality>(view, quality));
        //            break;
        //    }
        //}

        //private void FillPanes<TBaseType, TTargetType>(ConcurrentObservableCollection<TTargetType> target, ItemsControl control, List<IExportView> list1, List<IExportView> list2)
        //    where TBaseType : IExportView where TTargetType : RadPane
        //{
        //    var currentAssembly = Assembly.GetAssembly(typeof(ExportViewBase));
        //    var viewsTypes = currentAssembly.GetTypes().Where(a => a.IsClass && a.IsPublic && !a.IsInterface && !a.IsAbstract && typeof(TBaseType).IsAssignableFrom(a));

        //    foreach (var viewType in viewsTypes)
        //    {
        //        if (viewType != null)
        //        {
        //            var exportView = (IExportView)Activator.CreateInstance(viewType);
        //            var exportViewControl = (UserControl)exportView;

        //            this.registeredViews.Add(exportView);

        //            var newView = (TTargetType)Activator.CreateInstance(typeof(TTargetType), exportViewControl);

        //            target.Add(newView);

        //            /*   if (list1 != null)
        //            {
        //                if (exportView.IsExportedDataView || list2 == null)
        //                    list1.Add(exportView);
        //                else
        //                {
        //                    list2.Add(exportView);
        //                }
        //            }*/

        //            control.Items.Add(newView);

        //            exportView.ViewAvialable += this.exportView_ViewAvialable;

        //            this.focusableViews.Add(exportView, newView);
        //            this.focusableViewsInv.Add(newView, exportView);
        //        }
        //    }
        //}

        //private void LeftExportPanesGroup_OnSelectionChanged(object sender, RadSelectionChangedEventArgs e)
        //{
        //    if (this.isUserSelect && this.LeftExportPanesGroup.SelectedPane != null && this.focusableViewsInv.ContainsKey(this.LeftExportPanesGroup.SelectedPane))
        //    {
        //        var selectedViewPane = this.focusableViewsInv[this.LeftExportPanesGroup.SelectedPane];

        //        if (this.lastSelectedExportExplorerViewPane.Contains(selectedViewPane)) { this.lastSelectedExportExplorerViewPane.Remove(selectedViewPane); }

        //        this.lastSelectedExportExplorerViewPane.Insert(0, selectedViewPane);
        //    }
        //}

        //private void MainExportPanesGroup_OnSelectionChanged(object sender, RadSelectionChangedEventArgs e)
        //{
        //    if (this.isUserSelect && this.MainExportPanesGroup.SelectedPane != null && this.focusableViewsInv.ContainsKey(this.MainExportPanesGroup.SelectedPane))
        //    {
        //        var selectedViewPane = this.focusableViewsInv[this.MainExportPanesGroup.SelectedPane];

        //        if (this.lastSelectedExportViewPane.Contains(selectedViewPane)) { this.lastSelectedExportViewPane.Remove(selectedViewPane); }

        //        this.lastSelectedExportViewPane.Insert(0, selectedViewPane);
        //    }
        //}

        //private void ResetUserSelect()
        //{
        //    Thread.Sleep(1000);
        //    this.isUserSelect = true;
        //}

        //private void SelectExplorerPane()
        //{
        //    foreach (var exportView in this.lastSelectedExportExplorerViewPane)
        //    {
        //        if (this.avialableExportExplorerViewPane.Any(v => v.Key == exportView))
        //        {
        //            this.BringToFront(exportView);
        //            return;
        //        }
        //    }

        //    if (this.avialableExportExplorerViewPane.Any())
        //    {
        //        this.avialableExportExplorerViewPane.Sort((x, y) => x.Value - y.Value);
        //        this.BringToFront(this.avialableExportExplorerViewPane.First().Key);
        //    }
        //}

        //private void SelectExportPagePane(bool enableResultsView)
        //{
        //    var explorerSelected = false;
        //    foreach (var exportView in this.lastSelectedExportExplorerViewPane)
        //    {
        //        if (this.avialableExportExplorerViewPane.Any(v => v.Key == exportView))
        //        {
        //            this.BringToFront(exportView);
        //            explorerSelected = true;
        //            break;
        //        }
        //    }

        //    if (!explorerSelected)
        //    {
        //        if (this.avialableExportExplorerViewPane.Any())
        //        {
        //            this.avialableExportExplorerViewPane.Sort((x, y) => x.Value - y.Value);
        //            this.BringToFront(this.avialableExportExplorerViewPane.First().Key);
        //        }
        //    }

        //    var expotedDataViewSelected = false;
        //    foreach (var exportView in this.lastSelectedExportViewPane)
        //    {
        //        if (this.avialableExportDataViewPane.Any(v => v.Key == exportView))
        //        {
        //            this.BringToFront(exportView);
        //            expotedDataViewSelected = true;
        //            break;
        //        }

        //        if (enableResultsView)
        //        {
        //            if (this.avialableExportResultViewPane.Any(v => v.Key == exportView))
        //            {
        //                this.BringToFront(exportView);
        //                expotedDataViewSelected = true;
        //                break;
        //            }
        //        }
        //    }

        //    if (!expotedDataViewSelected)
        //    {
        //        if (this.avialableExportDataViewPane.Any())
        //        {
        //            this.avialableExportDataViewPane.Sort((x, y) => x.Value - y.Value);
        //            this.BringToFront(this.avialableExportDataViewPane.First().Key);
        //            expotedDataViewSelected = true;
        //        }

        //        if (!expotedDataViewSelected)
        //        {
        //            if (enableResultsView)
        //            {
        //                if (this.avialableExportResultViewPane.Any())
        //                {
        //                    this.avialableExportResultViewPane.Sort((x, y) => x.Value - y.Value);
        //                    this.BringToFront(this.avialableExportResultViewPane.First().Key);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}