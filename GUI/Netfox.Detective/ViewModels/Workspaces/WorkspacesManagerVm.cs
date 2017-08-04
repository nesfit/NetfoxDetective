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

using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.Services;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Workspaces
{
    public sealed class WorkspacesManagerVm : DetectiveApplicationPaneViewModelBase
    {
        public WorkspacesManagerVm(WindsorContainer applicationWindsorContainer, WorkspacesManagerService workspacesManagerService, IWorkspacesManagerView workspacesManagerView ) : base(applicationWindsorContainer)
        {
            this.WorkspacesManagerService = workspacesManagerService;
            this.View = workspacesManagerView;
            //DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IWorkspacesManagerView>());
            this.IsHidden = false;
            this.IsSelected = true;
        }

        public override string HeaderText => "Workspace manager";

        [IgnoreAutoChangeNotification]
        public WorkspacesManagerService WorkspacesManagerService { get; }

        public ConcurrentObservableCollection<WorkspaceVm> Workspaces => this.WorkspacesManagerService.RecentWorkspaces;

        #region Commands

        #region SelectStorage
        [IgnoreAutoChangeNotification]
        public ICommand COpenWorkspace => new RelayCommand<WorkspaceVm>(async vm => await this.WorkspacesManagerService.OpenWorkspace(vm), workspace => true);
        #endregion

        [IgnoreAutoChangeNotification]
        public RelayCommand CCreateWorksCommand => new RelayCommand(() => this.NavigationService.Show(typeof(NewWorkspaceInvestigationWizardVm)));
        
        [IgnoreAutoChangeNotification]
        public RelayCommand<WorkspaceVm> CDeleteWorksCommand => new RelayCommand<WorkspaceVm>(this.WorkspacesManagerService.RemoveWorkspace);
        #endregion
    }
}