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
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AlphaChiTech.Virtualization;
using Netfox.Core.Interfaces;
using Netfox.Core.Models;
using Netfox.Core.Properties;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.Base;

namespace Netfox.Detective.Models.WorkspacesAndSessions
{
    [DataContract(Name = "Workspace", Namespace = "Netfox.Detective.Models.WorkspacesAndSessions")]
    public class Workspace
    {
        private ConcurrentIObservableVirtualizingObservableCollection<Investigation> _investigations;
        private DirectoryInfo _investigationsDirectoryInfo;
        private FileInfo _workspaceFileInfo;

        #region Constructors
        public Workspace(IInvestigationFactory investigationFactory,IInvestigationInfoLoader investigationInfoLoader, string workspaceName, string workspacesStoragePath, string connectionString)
        {
            this.InvestigationFactory = investigationFactory;
            this.InvestigationInfoLoader = investigationInfoLoader;
            this.Name = workspaceName;
            this.ConnectionString = connectionString;
            this.WorkspaceDirectoryInfo = GetWorkspaceDirectoryInfo(this.Name,workspacesStoragePath);
            if(this.WorkspaceDirectoryInfo.Exists) { throw new InvalidOperationException("InvestigationWorkspace already exists!"); }
            this.WorkspaceDirectoryInfo.Create();
            this.Guid = Guid.NewGuid();
            this.Created = DateTime.Now;
            this.LastRecentlyUsed = this.Created;

            this.InvestigationsDirectoryInfo.Create();
        }
        #endregion

        public static DirectoryInfo GetWorkspaceDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            return new DirectoryInfo(Path.Combine(workspacesStoragePath, workspaceName));
        }

        public static DirectoryInfo GetInvestigationsDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
                var workspaceDirectoryInfo = GetWorkspaceDirectoryInfo(workspaceName, workspacesStoragePath);
                return GetInvestigationsDirectoryInfo(workspaceDirectoryInfo);
        }
        public static DirectoryInfo GetInvestigationsDirectoryInfo(DirectoryInfo workspaceDirectoryInfo)
        {
            return new DirectoryInfo(Path.Combine(workspaceDirectoryInfo.FullName, "Investigations"));
        }

        #region Misc
        public override string ToString() => this.Name;
        #endregion

        #region Member properties
        [DataMember]
        public DirectoryInfo WorkspaceDirectoryInfo { get; set; }

        [XmlIgnore]
        public FileInfo WorkspaceFileInfo
            =>
                this._workspaceFileInfo
                ?? (this._workspaceFileInfo = new FileInfo(Path.Combine(this.WorkspaceDirectoryInfo.FullName, this.Name + "." + NetfoxSettings.Default.WorkspaceFileExtension)));

        [XmlIgnore]
        public DirectoryInfo InvestigationsDirectoryInfo => this._investigationsDirectoryInfo ?? (this._investigationsDirectoryInfo = GetInvestigationsDirectoryInfo(this.WorkspaceDirectoryInfo));

        public IInvestigationFactory InvestigationFactory { get; set; }
        public IInvestigationInfoLoader InvestigationInfoLoader { get; set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public Guid Guid { get; private set; }

        [DataMember]
        public DateTime Created { get; private set; }

        [DataMember]
        public DateTime LastRecentlyUsed { get; set; }

        #endregion

        #region Investigations
        public ConcurrentIObservableVirtualizingObservableCollection<Investigation> Investigations
            => this._investigations ?? (this._investigations = new ConcurrentIObservableVirtualizingObservableCollection<Investigation>());

        private List<String> _investigationFilePaths;

        [DataMember]
        public List<String> InvestigationsFilePaths
        {
            get { return this._investigationFilePaths ?? (this._investigationFilePaths = new List<string>()); }
            set { this._investigationFilePaths = value; }
        }

        public async Task CreateAndAddNewInvestigation(InvestigationInfo investigationInfo)
        {
            investigationInfo.CreateFileStructure();
            await this.CreateAndLoadInvestigation(investigationInfo);
            this.InvestigationsFilePaths.Add(investigationInfo.InvestigationFileRelativePath);
        }

        private async Task CreateAndLoadInvestigation(InvestigationInfo investigationInfo)
        {
            var investigation = this.InvestigationFactory.Create(investigationInfo);
            await investigation.Initialize();
            this.Investigations.Add(investigation);
        }

        public void RemoveInvestigation(Investigation investigation)
        {
            this.Investigations.Remove(investigation);
            this.InvestigationsFilePaths.Remove(investigation.InvestigationInfo.InvestigationFileRelativePath);
        }
        #endregion

        public  static Workspace Load(string filePath)
        {
            var deserializer = new DataContractSerializer(typeof(Workspace));
            Workspace workspace;
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas())) { workspace = (Workspace)deserializer.ReadObject(reader, true); }
            }
            workspace.LastRecentlyUsed = DateTime.Now;
            return workspace;
        }

        public async Task LoadInvestigations()
        {
            foreach (var investigationFilePath in this.InvestigationsFilePaths)
            {
                var investigationFileInfo = new FileInfo(Path.Combine(this.InvestigationsDirectoryInfo.FullName, investigationFilePath));
                if (!investigationFileInfo.Exists) { continue; }

                var investigationInfo = this.InvestigationInfoLoader.Load(investigationFileInfo);
                investigationInfo.InvestigationsDirectoryInfo = this.InvestigationsDirectoryInfo;

                await this.CreateAndLoadInvestigation(investigationInfo);
            }
        }
    }
}