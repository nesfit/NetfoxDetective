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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Castle.Core.Logging;
using Netfox.Core.Collections;
using Netfox.Core.Interfaces;
using Netfox.Core.Messages.Base;
using Netfox.Core.Models;
using Netfox.Core.Properties;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Netfox.Detective.ViewModels.Workspaces;

namespace Netfox.Detective.Services
{
    /// <summary>
    ///     Class implementing management of application sessions.
    /// </summary>
    public class WorkspacesManagerService : DetectiveApplicationServiceBase, INotifyPropertyChanged
    {
        private WorkspaceVm _currentWorkspace;

        public WorkspacesManagerService(
            IInvestigationFactory investigationFactory,
            IInvestigationInfoLoader investigationInfoLoader,
            IWorkspaceFactory workspaceFactory,
            ILogger logger) : base()
        {
            this.InvestigationFactory = investigationFactory;
            this.InvestigationInfoLoader = investigationInfoLoader;
            this.WorkspaceFactory = workspaceFactory;
            this.Logger = logger;
            this.LoadWorkspacesFromSettingsFile();
        }

        public IInvestigationFactory InvestigationFactory { get; }
        public IInvestigationInfoLoader InvestigationInfoLoader { get; }

        public ConcurrentObservableCollection<WorkspaceVm> RecentWorkspaces { get; } = new ConcurrentObservableCollection<WorkspaceVm>();

        public WorkspaceVm CurrentWorkspace
        {
            get => this._currentWorkspace;
            set
            {
                this.LoadWorkspace(value).Wait();
                this.OnPropertyChanged();
            }
        }

        protected IWorkspaceFactory WorkspaceFactory { get; }

        #region InvestigationWorkspace manipulation
        public async Task OpenWorkspace(WorkspaceVm workspace) => await this.LoadWorkspace(workspace);

        public async Task OpenWorkspace(string filePath)
        {
            try
            {
                if(string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    this.Logger?.Error("Trying to open workspace that does not exists!");
                    return;
                }
                var workspace = Workspace.Load(filePath); //new WorkspaceVm(filePath, this.ApplicationWindsorContainer);
                var workspaceVm = this.WorkspaceFactory.Create(workspace);
                this.AddWorkspace(workspaceVm);
                await this.OpenWorkspace(workspaceVm);
            }
            catch(Exception ex) { this.Logger?.Error(ex.GetType().Name, ex); }
        }

        private async Task LoadWorkspace(WorkspaceVm workspace)
        {
            if(workspace == null)
            {
                this.Logger?.Error("Workspace is null");
                return;
            }
            if(this._currentWorkspace == workspace)
            {
                this.Logger?.Warn("Workspace is already loaded");
                return;
            }
            this.CloseCurrentWorkspace();

            await workspace.Open();
            this._currentWorkspace = workspace;
        }

        private void LoadWorkspacesFromSettingsFile()
        {
            var serializedWorkspaces = NetfoxSettings.Default.LastWorkspaces;
            this.FromString(serializedWorkspaces);
        }

        private void SaveWorkspacesToSettingsFile()
        {
            NetfoxSettings.Default.LastWorkspaces = this.ToString();
            NetfoxSettings.Default.Save();
        }

        public void CloseCurrentWorkspace()
        {
            this.SaveWorkspacesToSettingsFile();
            this.CurrentWorkspace?.Close();
        }

        public async Task<WorkspaceVm> CreateWorkspace(string workspaceName, string workspacesStoragePath, string connectionString, InvestigationInfo investigationInfo)
        {
            var workspace = this.WorkspaceFactory.Create(workspaceName, workspacesStoragePath, connectionString);
            var workspaceVm = this.WorkspaceFactory.Create(workspace);
            this.AddWorkspace(workspaceVm);
            WorkspaceMessage.SendWorkspaceMessage(workspaceVm, WorkspaceMessage.Type.Created);
            await this.OpenWorkspace(workspaceVm);
            await workspaceVm.CreateNewInvestigation(investigationInfo);
            return workspaceVm;
        }

        private void AddWorkspace(WorkspaceVm newWorkspace)
        {
            lock(this.RecentWorkspaces)
            {
                if(newWorkspace.Exists) { this.RecentWorkspaces.Insert(0, newWorkspace); }
                else { this.Logger?.Warn($"InvestigationWorkspace {newWorkspace.Workspace.Name} is missing."); }
            }
        }

        public void RemoveWorkspace(WorkspaceVm workspace)
        {
            if(workspace == null) { return; }
            workspace.Close();
            lock(this.RecentWorkspaces)
            {
                this.RecentWorkspaces.Remove(workspace);
                if(this.CurrentWorkspace == workspace) { this.CurrentWorkspace = null; }
            }
            this.SaveWorkspacesToSettingsFile();
            this.Logger?.Info($"Workspace deleted: {workspace.Workspace.Name}");
            //Task.Factory.StartNew(() => Directory.Delete(toDeleteWorkspace.WorkspaceStoragePath, true)); //TODO implement
        }
        #endregion

        #region Serialization and Deserialization
        public override string ToString()
        {
            var doc = new XmlDocument();
            var serializer = new DataContractSerializer(typeof(List<Workspace>), new[]
            {
                typeof(Workspace)
            });
            try
            {
                using(var writer = new MemoryStream())
                {
                    serializer.WriteObject(writer, this.RecentWorkspaces.Select(a => a.Workspace).ToList());
                    writer.Position = 0;
                    doc.Load(writer);
                    return doc.InnerXml;
                }
            }
            catch(IOException ex) { this.Logger?.Error(ex.GetType().Name, ex); }
            return string.Empty;
        }

        public void FromString(string source)
        {
            if(string.IsNullOrEmpty(source)) { return; }
            try
            {
                var deserializer = new DataContractSerializer(typeof(List<Workspace>));
                List<Workspace> loadedWorkspaces;
                using(var fs = new StringReader(source))
                {
                    using(var reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(fs.ReadToEnd()), new XmlDictionaryReaderQuotas()))
                    {
                        loadedWorkspaces = (List<Workspace>) deserializer.ReadObject(reader, false);
                    }
                }
                var workspaces = loadedWorkspaces.OrderBy(s => s.LastRecentlyUsed);
                foreach(var workspace in workspaces)
                {
                    workspace.InvestigationFactory = this.InvestigationFactory;
                    workspace.InvestigationInfoLoader = this.InvestigationInfoLoader;
                    if(!workspace.WorkspaceFileInfo.Exists)
                    {
                        this.Logger?.Error("Workspace { workspace.Name} is missing!");
                        continue;
                    }
                    var workspaceVm = this.WorkspaceFactory.Create(workspace);
                    this.AddWorkspace(workspaceVm);
                }
            }
            catch(Exception ex) { this.Logger?.Error("Workspaces were not loaded", ex); }
        }
        #endregion

        #region InotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}