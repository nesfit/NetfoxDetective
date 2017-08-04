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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Castle.Core;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Core.Messages;
using Netfox.Core.Messages.Base;
using Netfox.Core.Models;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Detective.ViewModelsDataEntity.Investigations;
using PostSharp.Patterns.Model;

namespace Netfox.Detective.ViewModels.Workspaces
{
    [NotifyPropertyChanged]
    public class WorkspaceVm : DetectiveDataEntityViewModelBase, IDisposable
    {
        private InvestigationVm _currentInvestigation;
        private bool _mDisposed;

       public WorkspaceVm(Workspace workspace, IWindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this.ApplicationOrInvestigationWindsorContainer = applicationWindsorContainer;
            this.Workspace = workspace;
            this.Save();
            WorkspaceMessage.SendWorkspaceMessage(this, WorkspaceMessage.Type.Created);
        }

        public Workspace Workspace { get; }

        [SafeForDependencyAnalysis]
        public bool Exists => this.Workspace.WorkspaceFileInfo.Exists;
        public ViewModelVirtualizingIoCObservableCollection<InvestigationVm, Investigation> Investigations { get; private set; }

        [IgnoreAutoChangeNotification]
        [DoNotWire]
        public InvestigationVm CurrentInvestigation
        {
            get
            {
                if(this._currentInvestigation == null) { this.CurrentInvestigation = this.Investigations?.FirstOrDefault(); }
                return this._currentInvestigation;
            }
            set
            {
                if(value == this._currentInvestigation || value == null) { return; }

                if(!this.Investigations.Contains(value)) { return; }

                this._currentInvestigation = value;
                this.OnPropertyChanged();
                Task.Run(async () =>
                {
                    if(this._currentInvestigation != null)
                    {
                        await this._currentInvestigation.Init();
                        this.Logger?.Info($"Investigation selected: {value.Investigation.InvestigationInfo.InvestigationName}");
                        InvestigationMessage.SendInvestigationMessage(this._currentInvestigation, InvestigationMessage.MessageType.CurrentInvestigationChanged);
                    }
                });
            }
        }

        private bool IsOpened { get; set; }

        public new void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            if(!this.IsOpened) { return; }
            WorkspaceMessage.SendWorkspaceMessage(this, WorkspaceMessage.Type.Closing, DetectiveMessage.SendMethod.Blocking);
            WorkspaceMessage.SendWorkspaceMessage(this, WorkspaceMessage.Type.Closed, DetectiveMessage.SendMethod.Blocking);
            this.IsOpened = false;
        }

        public async Task CreateNewInvestigation(InvestigationInfo investigationInfo)
        {
            try
            {
                if(!this.IsOpened) { await this.Open(); }

                await this.Workspace.CreateAndAddNewInvestigation(investigationInfo);
                this.Logger?.Info($"Investigation created: {investigationInfo.InvestigationName}");
                this.CurrentInvestigation = this.Investigations.First();
            }
            catch(Exception ex) {
                this.Logger?.Error($"Unable to create investigation: {investigationInfo?.InvestigationName}", ex);
            }
        }

        public async Task Open()
        {
            WorkspaceMessage.SendWorkspaceMessage(this, WorkspaceMessage.Type.Opening);
            this.IsOpened = true;
            await this.Workspace.LoadInvestigations();
            this.Investigations = new ViewModelVirtualizingIoCObservableCollection<InvestigationVm, Investigation>(this.Workspace.Investigations,
                this.ApplicationOrInvestigationWindsorContainer);

            WorkspaceMessage.SendWorkspaceMessage(this, WorkspaceMessage.Type.Opened, DetectiveMessage.SendMethod.Blocking);
        }

        public void RemoveInvestigation(InvestigationVm investigation)
        {
            this.Workspace.RemoveInvestigation(investigation.Investigation);
            this.Logger?.Info($"Investigation deleted: {investigation.Investigation.InvestigationInfo.InvestigationName}");
        }

        protected virtual void Dispose(bool disposing)
        {
            if(this._mDisposed) { return; }

            if(disposing)
            {
                // Dispose managed data
                //m_managedData.Dispose();
            }
            // Free unmanaged data
            //DataProvider.DeleteUnmanagedData(m_unmanagedData);
            this.Save();
            this._mDisposed = true;
        }

        internal void Save()
        {
            if(this.Workspace == null) { return; }
            var serializer = new DataContractSerializer(typeof(Workspace));
            try
            {
                using(var writer = new FileStream(this.Workspace.WorkspaceFileInfo.FullName, FileMode.Create)) { serializer.WriteObject(writer, this.Workspace); }
            }
            catch(IOException ex) {
                this.Logger?.Error($"Worspace save failed: {this.Workspace?.Name}", ex);
            }

            if(this.Investigations == null) { return; }
            foreach(var investigation in this.Investigations) { investigation.Save(); }
        }

        ~WorkspaceVm()
        {
            // finalizer
            try {
                this.Dispose(false);
            }
            catch(Exception ex) {
                this.Logger?.Error($"Workspace {this.Workspace.Name} cannot be saved", ex);
            }
        }
    }
}