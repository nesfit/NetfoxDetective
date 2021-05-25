using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Castle.Core;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Collections;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.ViewModels.Conversations;
using Netfox.Detective.ViewModels.Exports;
using Netfox.Detective.ViewModelsDataEntity.ConversationsCollections;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Investigations
{
    public class InvestigationExplorerVm : DetectiveApplicationPaneViewModelBase
    {
        private RelayCommandAsync _cAddLog;
        private RelayCommand _cAddNewGroup;
        private RelayCommandAsync _cRemoveSelectedCaptureButtonCommand;
        private InvestigationVm _investigationVm;
        private object _lastSelected;
        private RelayCommandAsync _addCaptureCommand;
        private RelayCommandAsync<RoutedEventArgs> _cRemoveCaptureButtonCommand;
        private IDetectiveMessenger _messenger;

        public InvestigationExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IInvestigationExplorerView>());
            this._messenger = this.ApplicationOrInvestigationWindsorContainer.Resolve<IDetectiveMessenger>();
            Task.Factory.StartNew(() =>
                this._messenger.Register<OpenedInvestigationMessage>(this, OpenedInvestigationMessageReceived));
            this.IsHidden = false;
            this.IsSelected = true;
            this.IsActive = true;
            this.DockPositionPosition = DetectiveDockPosition.DockedLeft;
        }

        public override string HeaderText => "Investigation explorer";

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                this.SetIsSelectedInvestigation(value);
            }
        }

        //private const string DefaultName = "Investigation explorer";
        [DoNotWire]
        public InvestigationVm InvestigationVm
        {
            get { return this._investigationVm; }
            set
            {
                if (this._investigationVm == value)
                {
                    return;
                }

                this._investigationVm = value;
                this.OnPropertyChanged(nameof(InvestigationVm));
            }
        }

        private async Task AddCaptureExecuteAsync()
        {
            await Task.Run(async () =>
            {
                if (this.SystemServices == null)
                {
                    return;
                }

                var captureFiles = this.SystemServices.OpenFilesDialog("", ".cap",
                    "Captures (*.cap, *.pcap,*.pcapng)|*.cap;*.pcap;*.pcapng|All Files (*.*)|*.*");
                foreach (var captureFile in captureFiles)
                {
                    await this.InvestigationVm.AddCaptureAsync(captureFile);
                }
            });
        }

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CAddCapture => this._addCaptureCommand ?? (this._addCaptureCommand =
            new RelayCommandAsync(this.AddCaptureExecuteAsync, () => this.InvestigationVm != null));


        [IgnoreAutoChangeNotification]
        public RelayCommand CAddNewGroup => this._cAddNewGroup ?? (this._cAddNewGroup =
            new RelayCommand(this.AddNewGroupExecute, () => this.InvestigationVm != null));

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync<RoutedEventArgs> CRemoveCaptureButtonCommand => this._cRemoveCaptureButtonCommand ??
            (this._cRemoveCaptureButtonCommand =
                new RelayCommandAsync<RoutedEventArgs>((args, token) => this.RemoveCapture(args),
                    a => this.InvestigationVm != null));

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CRemoveSelectedCaptureButtonCommand
            =>
                this._cRemoveSelectedCaptureButtonCommand
                ?? (this._cRemoveSelectedCaptureButtonCommand =
                    new RelayCommandAsync(
                        () => this.InvestigationVm.RemoveCaptureAsync(this.InvestigationVm.CurrentCapture),
                        () => this.InvestigationVm != null));

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CAddLog => this._cAddLog ?? (this._cAddLog =
            new RelayCommandAsync(() => this.InvestigationVm?.AddSourceLog() ?? Task.CompletedTask,
                () => this.InvestigationVm != null));

        [IgnoreAutoChangeNotification]
        public ICommand SelectionChangedCommand => new RelayCommand<SelectionChangedEventArgs>(this.SelectionChanged);

        [IgnoreAutoChangeNotification]
        public ICommand InvestigationTreeViewDoubleClickCommand =>
            new RelayCommand<RoutedEventArgs>(this.InvestigationTreeViewDoubleClick);

        public ObservableCollection<object> SelectedItems { get; set; } = new ConcurrentObservableCollection<object>();

        public object SelectedItem { get; set; }

        [IgnoreAutoChangeNotification]
        private void AddNewGroupExecute()
        {
            this.InvestigationVm.AddNewExportGroup();
        }

        private void OpenedInvestigationMessageReceived(OpenedInvestigationMessage msg)
        {
            this.SetIsSelectedInvestigation(false);
            this.InvestigationVm = msg.InvestigationVm as InvestigationVm;
            this.SetIsSelectedInvestigation(this.IsSelected);

            this.CAddCapture.RaiseCanExecuteChanged();
            this.CAddNewGroup.RaiseCanExecuteChanged();
            this.CAddLog.RaiseCanExecuteChanged();
        }

        [IgnoreAutoChangeNotification]
        private void InvestigationTreeViewDoubleClick(RoutedEventArgs args)
        {
            var selected = args.OriginalSource;
            if (selected != null && this._lastSelected != null)
            {
                var selectedType = this._lastSelected.GetType();
                if (selectedType == typeof(CaptureVm))
                {
                    this.NavigationService.Show(typeof(ConversationsOverviewVm), this._lastSelected);
                }
                //else if (selectedType == typeof(ConversationVm))
                //{
                //    this.NavigationService.Show(typeof(ConversationVm)); //ConversationDetailView
                //}
                else if (selectedType == typeof(ExportGroupVm))
                {
                    this.NavigationService.Show(typeof(ExportOverviewVm), this._lastSelected);
                }
                else if (typeof(ExportVm).IsAssignableFrom(selectedType))
                {
                    this.Logger?.Error($"Action is not implemented {Environment.StackTrace}");
                    //throw new NotImplementedException();
                    //this.NavigationService.Show(typeof(ExportVm)); //ExportResultView
                }
                else if (selectedType == typeof(ConversationsGroupVm))
                {
                    this.NavigationService.Show(typeof(ConversationsOverviewVm), this._lastSelected);
                }
            }
        }

        private async Task RemoveCapture(RoutedEventArgs args)
        {
            var selected = args.OriginalSource;
            if (selected == null || this._lastSelected == null)
            {
                return;
            }

            await this.InvestigationVm.RemoveCaptureAsync(this._lastSelected as CaptureVm);
        }


        [IgnoreAutoChangeNotification]
        private void SelectionChanged(SelectionChangedEventArgs args)
        {
            this.SelectionChangeExecute(args.RemovedItems, CollectionChangeAction.Remove);
            this.SelectionChangeExecute(args.AddedItems, CollectionChangeAction.Add);
        }

        private void SelectionChangeExecute(IList addedItems, CollectionChangeAction action)
        {
            foreach (var item in addedItems)
            {
                if (item == null) continue;
                this._lastSelected = item;
                if (item is CaptureVm)
                {
                    var selectedCapture = (CaptureVm) item;
                    switch (action)
                    {
                        case CollectionChangeAction.Add:
                            this.InvestigationVm.CurrentCapture = selectedCapture;
                            this.InvestigationVm.SelectedCaptureVms.Add(selectedCapture);
                            break;
                        case CollectionChangeAction.Remove:
                            this.InvestigationVm.SelectedCaptureVms.Remove(selectedCapture);
                            break;
                    }
                }
                else if (item is ConversationsGroupVm)
                {
                    var selectedConversationsGroup = (ConversationsGroupVm) item;
                    switch (action)
                    {
                        case CollectionChangeAction.Add:
                            this.InvestigationVm.CurrentConversationsGroupVm = selectedConversationsGroup;
                            this.InvestigationVm.SelectedConversationsGroupVms.Add(selectedConversationsGroup);
                            break;
                        case CollectionChangeAction.Remove:
                            this.InvestigationVm.SelectedConversationsGroupVms.Remove(selectedConversationsGroup);
                            break;
                    }
                }
                else if (item is ExportGroupVm)
                {
                    var selectedExportGroup = (ExportGroupVm) item;
                    switch (action)
                    {
                        case CollectionChangeAction.Add:
                            this.InvestigationVm.CurrentExportGroup = selectedExportGroup;
                            this.InvestigationVm.SelectedExportGroupVms.Add(selectedExportGroup);
                            break;
                        case CollectionChangeAction.Remove:
                            this.InvestigationVm.SelectedExportGroupVms.Remove(selectedExportGroup);
                            break;
                    }
                }
                else if (item is ExportVm)
                {
                    var selectedExportResult = (ExportVm) item;
                    var newCurrentGroup = this.InvestigationVm.ExportGroupByExportResult(selectedExportResult);
                    if (newCurrentGroup != null)
                    {
                        this.InvestigationVm.CurrentExportGroup = newCurrentGroup;
                        if (this.InvestigationVm.CurrentExportGroup != null)
                        {
                            this.InvestigationVm.CurrentExportGroup.SelectedExportResult = selectedExportResult;
                        }
                    }
                }

                //else if (selected.Equals(ExportsRoot))
                {
                }
            }
        }

        private void SetIsSelectedInvestigation(bool value)
        {
            var investigationVm = this.InvestigationVm;
            if (investigationVm != null)
            {
                investigationVm.IsSelected = value;
            }
        }
    }
}