using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Core;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Infrastructure;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Interfaces.ViewModelsDataEntity.Investigations;
using Netfox.Detective.Messages.Application;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.Messages.Workspaces;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Netfox.Detective.ViewModels.Workspaces;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Investigations
{
    [NotifyPropertyChanged]
    public class InvestigationsManagerVm : DetectiveApplicationPaneViewModelBase
    {
        private Workspace _workspace;

        public override string HeaderText => "Investigation manager";
        private IFileSystem _fileSystem;
        private IDetectiveMessenger _messenger;
        private IInvestigationFactory InvestigationFactory;
        private readonly ISerializationPersistor<Investigation> _investigationSerializationPersistor;

        #region Constructor

        public InvestigationsManagerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IInvestigationsManagerView>());

            this.InvestigationFactory =
                this.ApplicationOrInvestigationWindsorContainer.Resolve<IInvestigationFactory>();
            this.InvestigationInfoLoader =
                this.ApplicationOrInvestigationWindsorContainer.Resolve<IInvestigationInfoLoader>();
            this._fileSystem = this.ApplicationOrInvestigationWindsorContainer.Resolve<IFileSystem>();
            this._investigationSerializationPersistor = this.ApplicationOrInvestigationWindsorContainer
                .Resolve<ISerializationPersistor<Investigation>>();
            this._messenger = this.ApplicationOrInvestigationWindsorContainer.Resolve<IDetectiveMessenger>();
            this._messenger.Register<OpenedInvestigationMessage>(this, this.OpenedInvestigationMessageReceived);
            this._messenger.Register<OpenedWorkspaceMessage>(this, this.OpenedWorkspaceMessageReceived);
            this._messenger.Register<ClosedWorkspaceMessage>(this, this.ClosedWorkspaceMessageReceived);
            this._messenger.Register<CreatedInvestigationMessage>(this, this.CreatedInvestigationMessageReceived);
            this._messenger.Register<ExitedApplicationMessage>(this, this.ExitedApplicationMessageReceived);
        }

        private void ClosedWorkspaceMessageReceived(ClosedWorkspaceMessage msg)
        {
            this.SaveInvestigations();
        }

        private void ExitedApplicationMessageReceived(ExitedApplicationMessage msg)
        {
            this.SaveInvestigations();
        }

        private void SaveInvestigations()
        {
            foreach (var investigation in InnerInvestigations)
            {
                this._investigationSerializationPersistor.Save(investigation);
            }
        }

        private void CreatedInvestigationMessageReceived(CreatedInvestigationMessage msg)
        {
            var investigation = msg.Investigation as Investigation;

            this.InnerInvestigations.Add(investigation);
            this.Workspace.InvestigationsFilePaths.Add(investigation.InvestigationInfo.InvestigationFileRelativePath);

            this.Logger?.Info($"Investigation created: {investigation.InvestigationInfo.InvestigationName}");
            this.CurrentInvestigation = this.Investigations.First();
        }

        private async void OpenedWorkspaceMessageReceived(OpenedWorkspaceMessage msg)
        {
            this.Workspace = msg.Workspace;
            await this.LoadInvestigations(this.Workspace);

            this.NavigationService.Show(this.GetType());
            this.CNewToCreateInvestigation.RaiseCanExecuteChanged();
            this.CDeleteInvestigation.RaiseCanExecuteChanged();
        }

        private IInvestigationInfoLoader InvestigationInfoLoader;

        public ViewModelVirtualizingIoCObservableCollection<InvestigationVm, Investigation> Investigations
        {
            get
            {
                if (this._investigations == null)
                {
                    this._investigations =
                        new ViewModelVirtualizingIoCObservableCollection<InvestigationVm, Investigation>(
                            this.InnerInvestigations, this.ApplicationOrInvestigationWindsorContainer);
                }

                return this._investigations;
            }

            set { _investigations = value; OnPropertyChanged(nameof(Investigations)); }
        }

        private ViewModelVirtualizingIoCObservableCollection<InvestigationVm, Investigation> _investigations;

        public ConcurrentIObservableVirtualizingObservableCollection<Investigation> InnerInvestigations
        {
            get
            {
                return this._innerInvestigations ?? (this._innerInvestigations =
                    new ConcurrentIObservableVirtualizingObservableCollection<Investigation>());
            }
            set { this._innerInvestigations = value; }
        }

        private ConcurrentIObservableVirtualizingObservableCollection<Investigation> _innerInvestigations;

        [IgnoreAutoChangeNotification]
        [DoNotWire]
        public IInvestigationVm CurrentInvestigation
        {
            get
            {
                if (this._currentInvestigation == null)
                {
                    this._currentInvestigation = this.Investigations.FirstOrDefault();
                }

                return this._currentInvestigation;
            }
            set
            {
                if (value == this._currentInvestigation || value == null)
                {
                    return;
                }


                if (value is InvestigationVmNullObject)
                {
                    this._currentInvestigation = this.Investigations.FirstOrDefault();
                    this.OnPropertyChanged();
                    this._messenger.AsyncSend(new OpenedInvestigationMessage
                        {InvestigationVm = this._currentInvestigation});
                    return;
                }

                if (!this.Investigations.Contains(value))
                {
                    return;
                }

                this._currentInvestigation = value;
                this.OnPropertyChanged();
                Task.Run(async () =>
                {
                    if (this._currentInvestigation != null)
                    {
                        await this._currentInvestigation.Init();
                        this.Logger?.Info(
                            $"Investigation selected: {value.Investigation.InvestigationInfo.InvestigationName}");
                        this._messenger.AsyncSend(new OpenedInvestigationMessage
                            {InvestigationVm = this._currentInvestigation});
                    }
                });
            }
        }

        private IInvestigationVm _currentInvestigation;

        private void OpenedInvestigationMessageReceived(OpenedInvestigationMessage msg)
        {
            this.CDeleteInvestigation.RaiseCanExecuteChanged();
        }

        public Workspace Workspace
        {
            get => this._workspace;
            private set
            {
                this._workspace = value;
                this.OnPropertyChanged();
                this.CNewToCreateInvestigation.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand _CNewToCreateInvestigation;

        [IgnoreAutoChangeNotification]
        public RelayCommand CNewToCreateInvestigation => this._CNewToCreateInvestigation ??
                                                           (this._CNewToCreateInvestigation =
                                                               new RelayCommand(
                                                                   () => this.NavigationService.Show(
                                                                       typeof(NewWorkspaceInvestigationWizardVm),
                                                                       this.Workspace),
                                                                   () => this.Workspace != null));

        private RelayCommand _CDeleteInvestigation;

        [IgnoreAutoChangeNotification]
        public RelayCommand CDeleteInvestigation => this._CDeleteInvestigation ?? (this._CDeleteInvestigation =
            new RelayCommand(() => this.RemoveInvestigation(this.CurrentInvestigation),
                () => this.Investigations.Count > 0 && this.CurrentInvestigation != null));


        [IgnoreAutoChangeNotification]
        public ICommand COpenWorkspaceFolder => new RelayCommand(() =>
        {
            if (this.Workspace?.WorkspaceDirectoryInfo.Exists == false)
            {
                return;
            }


            if (this.Workspace?.WorkspaceDirectoryInfo != null)
            {
                var explorer = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
                Process.Start(explorer, this.Workspace?.WorkspaceDirectoryInfo.FullName);
            }
        });

        private void CleanInvestigations()
        {
            this.InnerInvestigations = new ConcurrentIObservableVirtualizingObservableCollection<Investigation>();
            this.Investigations =
                new ViewModelVirtualizingIoCObservableCollection<InvestigationVm, Investigation>(
                    this.InnerInvestigations, this.ApplicationOrInvestigationWindsorContainer);
        }

        public async Task LoadInvestigations(Workspace workspace)
        {
            CleanInvestigations();

            foreach (var investigationFilePath in workspace.InvestigationsFilePaths)
            {
                var investigationFileInfo = this._fileSystem.FileInfo.FromFileName(
                    Path.Combine(this.WorkspaceInvestigationsDIrectoryInfo().FullName, investigationFilePath));
                if (!investigationFileInfo.Exists)
                {
                    continue;
                }

                var investigationInfo = this.InvestigationInfoLoader.Load((FileInfoBase) investigationFileInfo);
                investigationInfo.InvestigationsDirectoryInfo = this.WorkspaceInvestigationsDIrectoryInfo();

                var investigation = await this.InvestigationFactory.Create(investigationInfo) as Investigation;

                this.InnerInvestigations.Add(investigation);
                this.CurrentInvestigation = this.Investigations.FirstOrDefault();
            }
        }

        private DirectoryInfoBase WorkspaceInvestigationsDIrectoryInfo()
        {
            return new DirectoryInfoFactory(this._fileSystem).CreateInvestigationsDirectoryInfo(this.Workspace
                .WorkspaceDirectoryInfo);
        }

        public void RemoveInvestigation(IInvestigationVm investigation)
        {
            this.InnerInvestigations.Remove(investigation.Investigation);
            this.Workspace.InvestigationsFilePaths.Remove(investigation.Investigation.InvestigationInfo
                .InvestigationFileRelativePath);

            this.CurrentInvestigation = new InvestigationVmNullObject();
            this.Logger?.Info(
                $"Investigation deleted: {investigation.Investigation.InvestigationInfo.InvestigationName}");
        }

        #region OpenInvestigationFolder

        [IgnoreAutoChangeNotification]
        public ICommand COpenInvestigationFolder => new RelayCommand(() =>
        {
            if (this.CurrentInvestigation?.Investigation.InvestigationInfo.InvestigationDirectoryInfo.Exists == false)
            {
                return;
            }

            var investigation = this.CurrentInvestigation?.Investigation;
            if (investigation != null)
            {
                var explorer = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "explorer.exe");
                Process.Start(explorer, investigation.InvestigationInfo.InvestigationDirectoryInfo.FullName);
            }
        });

        #endregion

        #endregion
    }
}