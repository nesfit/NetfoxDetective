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
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Castle.Core;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Helpers;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Models;
using Netfox.Core.Properties;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Workspaces
{
    public class NewWorkspaceInvestigationWizardVm : DetectiveWindowViewModelBase
    {
        private readonly string _defaultInMemoryConnectionString = NetfoxSettings.Default.DefaultInMemoryConnectionString;
        private readonly string _defaultSQLConnectionString = NetfoxSettings.Default.ConnectionString;
        private Workspace Workspace { get; }
        private RelayCommandAsync _createNewInvestigationAsync;
        private RelayCommandAsync _createNewWorkspace;
        private string _investigationName;

        private RelayCommand _openDirectory;
        private bool _storeDatabaseWithInvestigation = NetfoxSettings.Default.StoreDatabaseWithInvestigation;
        private string _workspaceName;
        private string _workspacePath;

        public NewWorkspaceInvestigationWizardVm(IWindsorContainer applicationWindsorContainer, InvestigationInfo investigationInfo) : base(applicationWindsorContainer)
        {
            this.InvestigationInfo = investigationInfo;
            this.ViewType = typeof(INewWorkspaceInvestionWizardView);
            var ts = DateTime.Now.Millisecond;
            this.SqlConnectionStringBuilder = this.CreateDefaultConnectionStringBuilder();
            this.InvestigationName = $"Initial investigation - {ts}";
            this.WorkspaceName = $"NFX workspace - {ts}";
        }

        public NewWorkspaceInvestigationWizardVm(IWindsorContainer applicationWindsorContainer, Workspace model, InvestigationInfo investigationInfo) : this(
            applicationWindsorContainer, investigationInfo)
        {
            this.Workspace = model;
            this.InvestigationName = "Initial investigation";
            this.WorkspaceName = model.Name;
            this.SqlConnectionStringBuilder = new SqlConnectionStringBuilder(model.ConnectionString);
        }

        public override string HeaderText => "New workspace wizard";

        public WorkspacesManagerService WorkspacesManagerService { get; set; }

        public string WorkspaceName
        {
            get => this._workspaceName;
            set
            {
                this._workspaceName = value;
                this.OnPropertyChanged();
                this.InvestigationInfo.InvestigationsDirectoryInfo = Workspace.GetInvestigationsDirectoryInfo(value, this.WorkspaceStoragePath);
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
                if(this.IsInMemory == value) return;
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
                catch(Exception) { this.Logger?.Error("Connection string is incorrect"); }
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
            get => this._workspacePath ?? Environment.ExpandEnvironmentVariables(NetfoxSettings.Default.DefaultWorkspaceStoragePath);
            set
            {
                this._workspacePath = value;
                this.OnPropertyChanged();
                this.InvestigationInfo.InvestigationsDirectoryInfo = Workspace.GetInvestigationsDirectoryInfo(this.WorkspaceName, value);
                this.RegenerateConnectionString();
                this.CreateNewWorkspace.RaiseCanExecuteChanged();
            }
        }

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CreateNewWorkspace => this._createNewWorkspace ?? (this._createNewWorkspace = new RelayCommandAsync(async () =>
        {
            await this.CreateWorkspace();
            this.Close();
        }, () => !File.Exists(Path.Combine(this.WorkspaceStoragePath, this.WorkspaceName))));

        [IgnoreAutoChangeNotification]
        public RelayCommandAsync CreateNewInvestigation => this._createNewInvestigationAsync ?? (this._createNewInvestigationAsync = new RelayCommandAsync(async () =>
        {
            if(this.Workspace == null) { await this.CreateWorkspace(); }
            else { await this.CreateInvestigation(); }

            this.Close();
        }, () => !File.Exists(Path.Combine(this.WorkspaceStoragePath, this.WorkspaceName))));

        [IgnoreAutoChangeNotification]
        public RelayCommand OpenDirectory => this._openDirectory ?? (this._openDirectory = new RelayCommand(() =>
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if(result == DialogResult.OK)
            {
                this._workspacePath = dialog.SelectedPath;
                this.WorkspaceStoragePath = this._workspacePath;
            }
        }));

        [SafeForDependencyAnalysis]
        public object DataSourceDefault => new SqlConnectionStringBuilder(NetfoxSettings.Default.ConnectionString).DataSource;

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
            get => this.InvestigationInfo.SqlConnectionStringBuilder ?? (this.InvestigationInfo.SqlConnectionStringBuilder = this.CreateDefaultConnectionStringBuilder());
            set
            {
                this.InvestigationInfo.SqlConnectionStringBuilder = value;
                if(this.StoreDatabaseWithInvestigation && !this.IsInMemory)
                {
                    this.SqlConnectionStringBuilder.AttachDBFilename = this.InvestigationInfo?.DatabaseFileInfo?.FullName ?? "";
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
            if(string.IsNullOrEmpty(this.InvestigationName) || string.IsNullOrEmpty(this.WorkspaceStoragePath) || string.IsNullOrEmpty(this.WorkspaceName)) { return false; }

            try
            {
                if(this.Workspace == null)
                    await this.WorkspacesManagerService.CreateWorkspace(this.WorkspaceName, this.WorkspaceStoragePath, this.SqlConnectionStringBuilder.ConnectionString,
                        this.InvestigationInfo);
            }
            catch(InvalidOperationException ex)
            {
                this.Logger?.Error("InvestigationWorkspace already exists.", ex);
                return false;
            }
            this.NavigationService.Show(typeof(InvestigationsManagerVm));
            return true;
        }

        public void RegenerateConnectionString() { this.SqlConnectionStringBuilder = this.SqlConnectionStringBuilder; }

        private SqlConnectionStringBuilder CreateDefaultConnectionStringBuilder()
        {
            if(this.IsInMemory) return new SqlConnectionStringBuilder(this._defaultInMemoryConnectionString);
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
            if(string.IsNullOrEmpty(this.InvestigationName) || string.IsNullOrEmpty(this.WorkspaceStoragePath) || string.IsNullOrEmpty(this.WorkspaceName)) { return false; }

            try
            {
                if(this.Workspace == null) return false;
                await this.WorkspacesManagerService.CurrentWorkspace.CreateNewInvestigation(this.InvestigationInfo);
            }
            catch(InvalidOperationException ex)
            {
                this.Logger?.Error("InvestigationWorkspace already exists.", ex);
                return false;
            }
            this.NavigationService.Show(typeof(InvestigationsManagerVm));
            return true;
        }
    }
}