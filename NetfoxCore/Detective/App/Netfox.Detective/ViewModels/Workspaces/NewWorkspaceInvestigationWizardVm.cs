using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Castle.Core;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Helpers;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Models;
using Netfox.Detective.Commands.Investigations;
using Netfox.Detective.Commands.Workspaces;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Investigations;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Netfox.Detective.ViewModels.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Workspaces
{
    public class NewWorkspaceInvestigationWizardVm : DetectiveWindowViewModelBase
    {
        private readonly string _defaultInMemoryConnectionString;

        private readonly string _defaultSQLConnectionString;
        private Workspace Workspace { get; }
        private RelayCommandAsync _createNewInvestigationAsync;
        private RelayCommandAsync _createNewWorkspace;
        private string _investigationName;

        private RelayCommand _openDirectory;
        private bool _storeDatabaseWithInvestigation;
        private string _workspaceName;
        private string _workspacePath;
        private readonly IInvestigationFactory _investigationFactory;
        private readonly IDirectoryInfoFactory _directoryInfoManager;
        private readonly IDetectiveMessenger _detectiveMessenger;
        private readonly CreateWorkspaceCommand _createWorkspaceCommand;
        private readonly CreateInvestigationCommand _createInvestigationCommand;
        private readonly INetfoxSettings _settings;

        public NewWorkspaceInvestigationWizardVm(IDirectoryInfoFactory directoryInfoManager,
            IWindsorContainer applicationWindsorContainer,
            InvestigationInfo investigationInfo, IInvestigationFactory investigationFactory,
            IDetectiveMessenger detectiveMessenger, CreateWorkspaceCommand createWorkspaceCommand,
            CreateInvestigationCommand createInvestigationCommand, INetfoxSettings settings) : base(
            applicationWindsorContainer)
        {
            _defaultInMemoryConnectionString = settings.DefaultInMemoryConnectionString;
            _defaultSQLConnectionString = settings.ConnectionString;
            _storeDatabaseWithInvestigation = settings.StoreDatabaseWithInvestigation;
            _settings = settings;
            _createInvestigationCommand = createInvestigationCommand;
            _createWorkspaceCommand = createWorkspaceCommand;
            _detectiveMessenger = detectiveMessenger;
            _investigationFactory = investigationFactory;
            _directoryInfoManager =
                directoryInfoManager ?? throw new ArgumentNullException(nameof(directoryInfoManager));
            this.InvestigationInfo = investigationInfo;
            this.ViewType = typeof(INewWorkspaceInvestigationWizardView);
            var ts = DateTime.Now.Millisecond;
            this.SqlConnectionStringBuilder = this.CreateDefaultConnectionStringBuilder();
            this.InvestigationName = $"Initial investigation - {ts}";
            this.WorkspaceName = $"NFX workspace - {ts}";
        }

        public NewWorkspaceInvestigationWizardVm(IDirectoryInfoFactory directoryInfoManager,
            IWindsorContainer applicationWindsorContainer, Workspace model,
            InvestigationInfo investigationInfo, IInvestigationFactory investigationFactory,
            IDetectiveMessenger detectiveMessenger,
            CreateWorkspaceCommand createWorkspaceCommand, CreateInvestigationCommand createInvestigationCommand,
            INetfoxSettings settings) :
            this(directoryInfoManager,
                applicationWindsorContainer, investigationInfo, investigationFactory, detectiveMessenger,
                createWorkspaceCommand, createInvestigationCommand, settings)
        {
            // workaround: in some cases this constructor is executed even though it should not be
            if (!string.IsNullOrEmpty(model.Name))
            {
                this.Workspace = model;
                this.InvestigationName = "Initial investigation";
                this.WorkspaceName = model.Name;
                this.SqlConnectionStringBuilder = new SqlConnectionStringBuilder(model.ConnectionString);
            }
        }

        public override string HeaderText => "New workspace wizard";


        public string WorkspaceName
        {
            get => this._workspaceName;
            set
            {
                this._workspaceName = value;
                this.OnPropertyChanged();
                this.InvestigationInfo.InvestigationsDirectoryInfo =
                    _directoryInfoManager.CreateInvestigationsDirectoryInfo(value, this.WorkspaceStoragePath);
                this.OnPropertyChanged(nameof(this.WorkspaceStoragePath));
                this.RegenerateConnectionString();
                this.CreateNewWorkspace.RaiseCanExecuteChanged();
            }
        }

        public string InvestigationName
        {
            get => this._investigationName;
            set
            {
                this._investigationName = value;
                this.SqlConnectionStringBuilder.InitialCatalog = value;
                this.InvestigationInfo.InvestigationName = value;
                this.OnPropertyChanged();
                this.RegenerateConnectionString();
            }
        }

        [SafeForDependencyAnalysis]
        public bool IsInMemory
        {
            get => this.InvestigationInfo.IsInMemory;
            set
            {
                if (this.IsInMemory == value) return;
                this.InvestigationInfo.IsInMemory = value;
                this.OnPropertyChanged();
                this.SqlConnectionStringBuilder = this.CreateDefaultConnectionStringBuilder();
            }
        }

        public string ConnectionString
        {
            get { return this.SqlConnectionStringBuilder.ToString(); }
            set
            {
                try
                {
                    this.SqlConnectionStringBuilder = new SqlConnectionStringBuilder(value);
                    this.OnPropertyChanged();
                }
                catch (Exception)
                {
                    this.Logger?.Error("Connection string is incorrect");
                }
            }
        }

        [SafeForDependencyAnalysis]
        public string UserName
        {
            get => this.SqlConnectionStringBuilder.UserID;
            set
            {
                this.SqlConnectionStringBuilder.UserID = value;
                this.OnPropertyChanged();
                this.RegenerateConnectionString();
            }
        }

        [SafeForDependencyAnalysis]
        public string Password
        {
            get => this.SqlConnectionStringBuilder.Password;
            set
            {
                this.SqlConnectionStringBuilder.Password = value;
                this.OnPropertyChanged();
                this.RegenerateConnectionString();
            }
        }

        [SafeForDependencyAnalysis]
        public string DataSource
        {
            get => this.SqlConnectionStringBuilder.DataSource;
            set
            {
                this.SqlConnectionStringBuilder.DataSource = value;
                this.OnPropertyChanged();
                this.RegenerateConnectionString();
            }
        }

        [SafeForDependencyAnalysis]
        public string WorkspaceStoragePath
        {
            get => this._workspacePath ??
                   Environment.ExpandEnvironmentVariables(_settings.DefaultWorkspaceStoragePath);
            set
            {
                this._workspacePath = value;
                this.OnPropertyChanged();
                this.InvestigationInfo.InvestigationsDirectoryInfo =
                    _directoryInfoManager.CreateInvestigationsDirectoryInfo(this.WorkspaceName, value);
                this.RegenerateConnectionString();
                this.CreateNewWorkspace.RaiseCanExecuteChanged();
            }
        }

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CreateNewWorkspace => this._createNewWorkspace ?? (this._createNewWorkspace =
            new RelayCommandAsync(async () =>
            {
                await this.CreateWorkspace();
                this.Close();
            }, () => !File.Exists(Path.Combine(this.WorkspaceStoragePath, this.WorkspaceName))));

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CreateNewInvestigation
        {
            get
            {
                return this._createNewInvestigationAsync ?? (this._createNewInvestigationAsync = new RelayCommandAsync(
                    async () =>
                    {
                        if (this.Workspace == null)
                        {
                            await this.CreateWorkspace();
                        }
                        else
                        {
                            await this.CreateInvestigation();
                        }

                        this.Close();
                    },
                    () => !File.Exists(
                        Path.Combine(this.WorkspaceStoragePath,
                            this.WorkspaceName))));
            }
        }

        [IgnoreAutoChangeNotification]
        public RelayCommand OpenDirectory => this._openDirectory ?? (this._openDirectory = new RelayCommand(() =>
        {
            var dialog = new FolderBrowserDialog();

            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this._workspacePath = dialog.SelectedPath;
                this.WorkspaceStoragePath = this._workspacePath;
            }
        }));

        [SafeForDependencyAnalysis]
        public object DataSourceDefault =>
            new SqlConnectionStringBuilder(_settings.ConnectionString).DataSource;

        public bool StoreDatabaseWithInvestigation
        {
            get => this._storeDatabaseWithInvestigation;

            set
            {
                this._storeDatabaseWithInvestigation = value;
                this.RegenerateConnectionString();
            }
        }

        [DoNotWire]
        private SqlConnectionStringBuilder SqlConnectionStringBuilder
        {
            get => this.InvestigationInfo.SqlConnectionStringBuilder ??
                   (this.InvestigationInfo.SqlConnectionStringBuilder = this.CreateDefaultConnectionStringBuilder());

            set
            {
                this.InvestigationInfo.SqlConnectionStringBuilder = value;
                if (this.StoreDatabaseWithInvestigation && !this.IsInMemory)
                {
                    this.SqlConnectionStringBuilder.AttachDBFilename =
                        this.InvestigationInfo?.DatabaseFileInfo?.FullName ?? "";
                }

                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ConnectionString));
                this.OnPropertyChanged(nameof(this.UserName));
                this.OnPropertyChanged(nameof(this.Password));
                this.OnPropertyChanged(nameof(this.DataSource));
                this.CreateNewWorkspace.RaiseCanExecuteChanged();
            }
        }

        private InvestigationInfo InvestigationInfo { get; }

        public async Task<bool> CreateWorkspace()
        {
            if (string.IsNullOrEmpty(this.InvestigationName) || string.IsNullOrEmpty(this.WorkspaceStoragePath) ||
                string.IsNullOrEmpty(this.WorkspaceName))
            {
                return false;
            }

            try
            {
                if (this.Workspace == null)
                    this._createWorkspaceCommand.Execute(new CreateWorkspaceCommand.CreateWorkspaceCommandParams
                    {
                        Name = this.WorkspaceName,
                        StoragePath = this.WorkspaceStoragePath,
                        ConnectionString = this.SqlConnectionStringBuilder.ConnectionString
                    });

                await this._createInvestigationCommand.ExecuteAsync(
                    new CreateInvestigationCommand.CreateInvestigationCommandParams
                    {
                        InvestigationInfo = this.InvestigationInfo,
                        Name = this.InvestigationName
                    });
            }
            catch (InvalidOperationException ex)
            {
                this.Logger?.Error("InvestigationWorkspace already exists.", ex);
                return false;
            }

            this.NavigationService.Show(typeof(InvestigationsManagerVm));
            return true;
        }

        public void RegenerateConnectionString()
        {
            this.SqlConnectionStringBuilder = this.SqlConnectionStringBuilder;
        }

        private SqlConnectionStringBuilder CreateDefaultConnectionStringBuilder()
        {
            if (this.IsInMemory) return new SqlConnectionStringBuilder(this._defaultInMemoryConnectionString);
            var csb = new SqlConnectionStringBuilder(this._defaultSQLConnectionString)
            {
                UserID = this.UserName,
                Password = this.Password,
                InitialCatalog = this.InvestigationName
            };
            return csb;
        }

        private async Task<bool> CreateInvestigation()
        {
            if (string.IsNullOrEmpty(this.InvestigationName) || string.IsNullOrEmpty(this.WorkspaceStoragePath) ||
                string.IsNullOrEmpty(this.WorkspaceName))
            {
                return false;
            }

            try
            {
                if (this.Workspace == null) return false;
                // await this.WorkspacesManagerService.CurrentWorkspace.CreateNewInvestigation(this.InvestigationInfo);
                var investigation = await this._investigationFactory.Create(this.InvestigationInfo);
                this._detectiveMessenger.Send(new CreatedInvestigationMessage
                {
                    Investigation = investigation
                });
            }
            catch (InvalidOperationException ex)
            {
                this.Logger?.Error("InvestigationWorkspace already exists.", ex);
                return false;
            }


            this.NavigationService.Show(typeof(InvestigationsManagerVm));
            return true;
        }
    }
}