// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet, Hana Slamova
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
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Application;
using Netfox.Detective.Messages.Workspaces;
using Netfox.Detective.Models.WorkspacesAndSessions;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Workspaces
{
    public sealed class WorkspacesManagerVm : DetectiveApplicationPaneViewModelBase
    {
        private Workspace _currentWorkspace;

        private readonly IDetectiveMessenger _messenger;
        private readonly ISerializationPersistor<Workspace> _workspaceSerializationPersistor;
        private readonly IWorkspacePathSerializationPersistor _workspacePathSerializationPersistor;
        private readonly IDirectoryWrapper _directoryWrapper;

        public WorkspacesManagerVm(WindsorContainer applicationWindsorContainer,
            IWorkspacesManagerView workspacesManagerView,
            IDetectiveMessenger messenger,
            ISerializationPersistor<Workspace> workspaceSerializationPersistor,
            IDirectoryWrapper directoryWrapper,
            IWorkspacePathSerializationPersistor workspacePathSerializationPersistor) 
            : base(applicationWindsorContainer)
        {
            this._workspacePathSerializationPersistor = workspacePathSerializationPersistor;
            this._directoryWrapper = directoryWrapper;
            this._workspaceSerializationPersistor = workspaceSerializationPersistor;
            this.View = workspacesManagerView;
            this.IsHidden = false;
            this.IsSelected = true;

            this.LoadWorkspacesFromLastSession();
            
            this._messenger = messenger;
            this._messenger.Register<LoadedWorkspaceMessage>(this, this.LoadedWorkspaceMessageReceived);
            this._messenger.Register<ExitedApplicationMessage>(this, this.ExitedApplicationMessageReceived);
            this._messenger.Register<CreatedWorkspaceMessage>(this, this.CreatedWorkspaceMessageReceived);
        }

        public override string HeaderText => "Workspace manager";
       
        [IgnoreAutoChangeNotification]
        public ICommand COpenWorkspace => new RelayCommand<Workspace>(async vm => await this.Open(vm), workspace => true);

        [IgnoreAutoChangeNotification]
        public RelayCommand CCreateWorksCommand => new RelayCommand(() => this.NavigationService.Show(typeof(NewWorkspaceInvestigationWizardVm)));
        
        [IgnoreAutoChangeNotification]
        public RelayCommand<Workspace> CDeleteWorksCommand => new RelayCommand<Workspace>(this.Remove);
        
        public ConcurrentObservableCollection<Workspace> RecentWorkspaces { get; } = new ConcurrentObservableCollection<Workspace>();
        public Workspace CurrentWorkspace
        {
            get => this._currentWorkspace;
            set
            {
                this.Open(value).Wait();
                this.OnPropertyChanged();
            }
        }
        
        private async void LoadedWorkspaceMessageReceived(LoadedWorkspaceMessage msg)
        {
            var workspace = msg.Workspace;
            this.Add(workspace);
            await this.Open(workspace);
        }

        private async void CreatedWorkspaceMessageReceived(CreatedWorkspaceMessage msg)
        {
            var workspace = msg.Workspace;
            this.Add(workspace);
            await this.Open(workspace);
        }

        private void ExitedApplicationMessageReceived(ExitedApplicationMessage msg)
        {
            this.Close(this.CurrentWorkspace);
        }

        private async Task Open(Workspace workspace)
        {
            if (workspace == null)
            {
                return;
            }
            if (this._currentWorkspace == workspace)
            {
                this.Logger?.Warn("Workspace is already loaded");
                return;
            }
            this.Close(this.CurrentWorkspace);

            this._messenger.Send(new OpenedWorkspaceMessage { Workspace = workspace });
            this._currentWorkspace = workspace;
        }

        public void Close(Workspace workspace)
        {
            this.SaveWorkspacesMetadata();
            this._workspaceSerializationPersistor.Save(workspace);
            this._messenger.Send(new ClosedWorkspaceMessage());
        }

        private void Add(Workspace newWorkspace)
        {
            lock (this.RecentWorkspaces)
            {
                if (!this.RecentWorkspaces.Contains(newWorkspace)) { this.RecentWorkspaces.Insert(0, newWorkspace); }
                else { this.Logger?.Warn($"InvestigationWorkspace {newWorkspace.Name} is missing."); }
            }
        }

        public void Remove(Workspace workspace)
        {
            if (workspace == null) { return; }
            
            this.Close(workspace);
            lock (this.RecentWorkspaces)
            {
                this.RecentWorkspaces.Remove(workspace);
                if (this.CurrentWorkspace == workspace) { this.CurrentWorkspace = null; }
            }
            this.SaveWorkspacesMetadata();
            this.Logger?.Info($"Workspace deleted: {workspace.Name}");

            Task.Factory.StartNew(() => this._directoryWrapper.Delete(workspace.WorkspaceDirectoryInfo.FullName));
        }

        private void SaveWorkspacesMetadata()
        {
            this._workspacePathSerializationPersistor.Save(this.RecentWorkspaces);
        }

        private void LoadWorkspacesFromLastSession()
        {
           var loadedWorkspacePaths = this._workspacePathSerializationPersistor.Load();

           foreach (var path in loadedWorkspacePaths.Select(wp=>wp.Path))
           {
               var workspace = this._workspaceSerializationPersistor.Load(path);
               this.RecentWorkspaces.Add(workspace);
           }
        }
    }
}