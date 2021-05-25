using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Castle.Core;
using Castle.MicroKernel;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Application;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.BgTasks;
using Netfox.Detective.ViewModels.Conversations;
using Netfox.Detective.ViewModels.Exports;
using Netfox.Detective.ViewModels.Investigations;
using Netfox.Detective.ViewModels.Outputs;
using Netfox.Detective.ViewModels.Windows;
using Netfox.Detective.ViewModels.Workspaces;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using Netfox.Detective.Views;
using Netfox.Detective.Views.Windows;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels
{
    public class ApplicationShell : DetectiveWindowViewModelBase, ISystemComponent, IApplicationShell
    {
        private RelayCommand _cShowTaskManager;

        private bool _isExited;

        private IDetectiveMessenger _messenger;

        public ApplicationShell(WindsorContainer applicationOrAppWindsorContainer,
            WorkspacesManagerVm workspacesManagerVm) : base(applicationOrAppWindsorContainer)
        {
            this.ApplicationWindsorContainer = applicationOrAppWindsorContainer;
            this.WorkspacesManagerVm = workspacesManagerVm;
            this._messenger = this.ApplicationWindsorContainer.Resolve<IDetectiveMessenger>();
            this.AvailableAnalyzers.Subscribe(analyzer => this.OnPropertyChanged(nameof(this.AvailableAnalyzers)));
        }
#if DEBUG
        public override string HeaderText => "NetFox Detective | DEBUG";
#else
        public override string HeaderText => "NetFox Detective";
#endif

        public ConcurrentObservableCollection<DetectivePaneViewModelBase> ViewPanesVMs { get; } =
            new ConcurrentObservableCollection<DetectivePaneViewModelBase>();

        public ConcurrentObservableCollection<DetectiveWindowViewModelBase> Windows { get; } =
            new ConcurrentObservableCollection<DetectiveWindowViewModelBase>();

        [IgnoreAutoChangeNotification] public IApplicationView ApplicationView { get; set; }

        public WindsorContainer ApplicationWindsorContainer { get; }

        [IgnoreAutoChangeNotification]
        public InvestigationsManagerVm InvestigationsManagerVm =>
            this.ApplicationWindsorContainer.Resolve<InvestigationsManagerVm>();

        [IgnoreAutoChangeNotification] public WorkspacesManagerVm WorkspacesManagerVm { get; }

        public CrossContainerHierarchyResolver CrossContainerHierarchyResolver { get; set; }

        public ConcurrentIObservableVirtualizingObservableCollection<IAnalyzer> AvailableAnalyzers { get; } =
            new ConcurrentIObservableVirtualizingObservableCollection<IAnalyzer>();

        [IgnoreAutoChangeNotification]
        public ICommand CShowWorkspaceManager =>
            new RelayCommand(() => this.NavigationService.Show(typeof(WorkspacesManagerVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowInvestigationManager =>
            new RelayCommand(() => this.NavigationService.Show(typeof(InvestigationsManagerVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowCaptureOverview =>
            new RelayCommand(() => this.NavigationService.Show(typeof(ConversationsOverviewVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowConversationsGroupOverview => new RelayCommand(() =>
            this.Logger?.Error($"Action is not implemented {Environment.StackTrace}"));

        [IgnoreAutoChangeNotification]
        public ICommand CShowConversationsGroupContent => new RelayCommand(() =>
            this.Logger?.Error($"Action is not implemented {Environment.StackTrace}"));

        [IgnoreAutoChangeNotification]
        public ICommand CShowConversation =>
            new RelayCommand(() => this.Logger?.Error($"Action is not implemented {Environment.StackTrace}"));

        //[IgnoreAutoChangeNotification]
        //public ICommand CShowExportOverview => new RelayCommand(() => this.NavigationService.Show(typeof(ExportOverviewVm)));
        [IgnoreAutoChangeNotification]
        public ICommand CShowExportContentExplorer =>
            new RelayCommand(() => this.NavigationService.Show(typeof(ExportContentExplorerVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowSelectiveExport =>
            new RelayCommand(() => this.NavigationService.Show(typeof(SelectiveExportVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowFullTextSearch => new RelayCommand(() =>
            this.Logger?.Error($"Action is not implemented {Environment.StackTrace}"));

        // [IgnoreAutoChangeNotification]
        //public ICommand CShowQuery => new RelayCommand(() => this.NavigationService.Show(typeof(QueryVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowTasks => new RelayCommand(() => this.NavigationService.Show(typeof(BgTasksManagerVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowSettings => new RelayCommand(() => this.NavigationService.Show(typeof(SettingsWindowVm)));

        [IgnoreAutoChangeNotification]
        public ICommand CShowHelp => new RelayCommand(() => this.NavigationService.Show(typeof(HelpDialogVm)));

        [IgnoreAutoChangeNotification]
        public RelayCommand<DetectiveWindowBase> ApplicationExit => new RelayCommand<DetectiveWindowBase>(this.Finish);

        [DoNotWire] public InvestigationVm CurrentInvestigationVm { get; set; }

        [IgnoreAutoChangeNotification]
        public RelayCommand CShowTaskManager => this._cShowTaskManager ?? (this._cShowTaskManager =
            new RelayCommand(() => this.NavigationService.Show(typeof(BgTasksManagerVm))));

        private SettingsWindow SettingsWindow { get; set; }

        public void Finish()
        {
            if (this._isExited)
            {
                return;
            }

            //  this.BgTasksManagerVm.BgTasksManagerService.Stop();
            this._messenger.AsyncSend(new ExitedApplicationMessage());
            //this.WorkspacesManagerVm.WorkspacesManagerService.CloseCurrentWorkspace();
            this.SettingsWindow.Close();
            foreach (var windowsVms in this.Windows)
            {
                windowsVms.Close();
            }

            this._isExited = true;
        }

        public async Task Run()
        {
            await this.Init();
            this.Show();
        }

        public new string ComponentName => "ApplicationShell";

        /// <summary>
        ///     Run at application termination
        /// </summary>
        public void Finish(DetectiveWindowBase window)
        {
            this.Finish();
            window?.Close();
            Application.Current.Shutdown(); // this is workaround since window is always null
        }

        /// <summary>
        ///     Run at application startup
        /// </summary>
        public async Task Init()
        {
            this.NavigationService.NavigateTo += this.NavigationService_NavigateTo;

            this.RegisterMessageHandlers();
            this.SettingsWindow = new SettingsWindow();

            this.NavigationService.Show(typeof(MainOutputVm), false);
            this.NavigationService.Show(typeof(InvestigationExplorerVm), false);
            this.NavigationService.Show(typeof(ConversationHierarchyExplorerVm), false);
            this.NavigationService.Show(typeof(WorkspacesManagerVm));

            await this.UpdateApplicationAnalyzers();
        }

        public void RegisterDetectivePaneView(DetectiveApplicationPaneViewBase detectiveApplicationPaneView)
        {
            var viewModel = detectiveApplicationPaneView.DataContext as DetectiveApplicationPaneViewModelBase;
            Debug.Assert(viewModel != null, "viewModel != null");
            viewModel.View = detectiveApplicationPaneView;
            this.ViewPanesVMs.Add(viewModel);
        }

        public void RegisterMessageHandlers()
        {
            //Messenger.Default.Register<DetailContentCreatedMessage>(this, this.ShowDetailPaneHandler);
            this._messenger.Register<OpenedInvestigationMessage>(this, this.OpenedInvestigationMessageReceived);
        }

        public void RegisterWindowView(DetectiveWindowBase window)
        {
            var viewModel = window.DataContext as DetectiveWindowViewModelBase;
            //todo Debug.Assert(viewModel != null, "viewModel != null");
            if (viewModel != null)
            {
                this.Windows.Add(viewModel);
            }
        }

        private void OpenedInvestigationMessageReceived(OpenedInvestigationMessage msg)
        {
            this.CurrentInvestigationVm = msg.InvestigationVm as InvestigationVm;
        }

        private void NavigateTo(DetectiveViewModelBase viewModel)
        {
            var paneViewModel = viewModel as DetectivePaneViewModelBase;
            if (paneViewModel != null)
            {
                if (!this.ViewPanesVMs.Contains(paneViewModel))
                {
                    this.ViewPanesVMs.Add(paneViewModel);
                }

                this.SelectPane(paneViewModel);
                return;
            }

            var windowViewModel = viewModel as DetectiveWindowViewModelBase;
            if (windowViewModel != null)
            {
                this.SelectWindow(windowViewModel);
                return;
            }

            Debugger.Break(); //TODO implement when breaked
        }

        private void NavigationService_NavigateTo(NavigationService.NavigationServiceArgs arguments)
        {
            try
            {
                var viewModel = arguments.ViewModel;

                if (viewModel is DetectivePaneViewModelBase)
                {
                    var vm = viewModel as DetectivePaneViewModelBase;
                    this.NavigateTo(vm);
                    return;
                }

                if (viewModel is DetectiveWindowViewModelBase)
                {
                    var vm = viewModel as DetectiveWindowViewModelBase;
                    this.NavigateTo(vm);
                    return;
                }

                this.NavigateTo(viewModel);
                this.Logger?.Warn($"{arguments.ViewModel.GetType().Name} is not a DetectivePaneViewModelBase");
            }
            catch (Exception ex)
            {
                this.Logger?.Error($"Navigation failed", ex);
            }
        }

        private void SelectPane(DetectivePaneViewModelBase detectiveViewModel)
        {
            if (detectiveViewModel == null)
            {
                return;
            }

            detectiveViewModel.IsHidden = false;
            detectiveViewModel.IsSelected = true;
        }

        private void SelectWindow(DetectiveWindowViewModelBase detectiveWindowViewModel)
        {
            if (detectiveWindowViewModel == null)
            {
                return;
            }

            detectiveWindowViewModel.IsHidden = false;
        }

        private async Task UpdateApplicationAnalyzers()
        {
            await Task.Run(() =>
            {
                foreach (var availableAnalyzerType in this.CrossContainerHierarchyResolver.AvailableAnalyzerTypes.Where(
                    t => typeof(IAnalyzerApplication).IsAssignableFrom(t)))
                {
                    try
                    {
                        if (this.AvailableAnalyzers.Any(a => a.GetType() == availableAnalyzerType))
                        {
                            return;
                        }

                        this.AvailableAnalyzers.Add(
                            this.ApplicationWindsorContainer.Resolve(availableAnalyzerType) as IAnalyzerApplication);
                    }
                    catch (ComponentResolutionException ex)
                    {
                        this.Logger?.Error($"Initialization of Analyzer {availableAnalyzerType.Name} failed", ex);
                    }
                }
            });
        }
    }
}