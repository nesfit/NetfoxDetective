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

using System.Diagnostics;
using System.Windows.Input;
using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Core.Messages.Base;
using Netfox.Detective.ViewModels.Workspaces;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Investigations
{
    public class InvestigationsManagerVm : DetectiveApplicationPaneViewModelBase
    {
        private WorkspaceVm _workspaceVm;

        public override string HeaderText => "Investigation manager";

        #region Constructor
        public InvestigationsManagerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IInvestigationsManagerView>());
            Messenger.Default.Register<WorkspaceMessage>(this, this.WorkspaceMessageHandler);
        }

        private void WorkspaceMessageHandler(WorkspaceMessage message)
        {
            if(message == null) { return; }
            switch(message.MessageType)
            {
                case WorkspaceMessage.Type.Created:
                    break;
                case WorkspaceMessage.Type.Opened:
                    if(!(message.Workspace is WorkspaceVm)) { return; }
                    this.WorkspaceVm = message.Workspace as WorkspaceVm;
                    this.NavigationService.Show(this.GetType());
                    this.CNewToCreateInvestigation.RaiseCanExecuteChanged();
                    this.CDeleteInvestigation.RaiseCanExecuteChanged();
                    break;
                case WorkspaceMessage.Type.Closed:
                    break;
            }
        }
        #endregion

        #region Properties

        public WorkspaceVm WorkspaceVm
        {
            get { return this._workspaceVm; }
            private set
            {
                this._workspaceVm = value;
                this.OnPropertyChanged();
                this.CNewToCreateInvestigation.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands
        
        #region NewToCreateInvestigation
        private RelayCommand _CNewToCreateInvestigation;

        [IgnoreAutoChangeNotification]
        public RelayCommand CNewToCreateInvestigation
            =>
                this._CNewToCreateInvestigation
                ?? (this._CNewToCreateInvestigation = new RelayCommand(() => this.NavigationService.Show(typeof(NewWorkspaceInvestigationWizardVm), this.WorkspaceVm.Workspace), () => this.WorkspaceVm != null));
        #endregion

        #region DeleteInvestigation
        private RelayCommand _CDeleteInvestigation;

        [IgnoreAutoChangeNotification]
        public RelayCommand CDeleteInvestigation
            =>
                this._CDeleteInvestigation
                ?? (this._CDeleteInvestigation =
                    new RelayCommand(() => this.WorkspaceVm?.RemoveInvestigation(this.WorkspaceVm?.CurrentInvestigation), () => this.WorkspaceVm?.CurrentInvestigation != null));
        #endregion

        #region OpenWorkspaceFolder
        [IgnoreAutoChangeNotification]
        public ICommand COpenWorkspaceFolder => new RelayCommand(() =>
        {
            if(this.WorkspaceVm?.Workspace?.WorkspaceDirectoryInfo.Exists == false) { return; }
            var workspaceVm = this.WorkspaceVm;
            if(workspaceVm?.Workspace?.WorkspaceDirectoryInfo != null) { Process.Start(workspaceVm?.Workspace?.WorkspaceDirectoryInfo.FullName); }
        });
        #endregion

        #region OpenInvestigationFolder
        [IgnoreAutoChangeNotification]
        public ICommand COpenInvestigationFolder => new RelayCommand(() =>
        {
            if(this.WorkspaceVm?.CurrentInvestigation?.Investigation.InvestigationInfo.InvestigationDirectoryInfo.Exists == false) { return; }
            var investigation = this.WorkspaceVm?.CurrentInvestigation?.Investigation;
            if(investigation != null) { Process.Start(investigation?.InvestigationInfo.InvestigationDirectoryInfo.FullName); }
        });
        #endregion

        #endregion
    }
}