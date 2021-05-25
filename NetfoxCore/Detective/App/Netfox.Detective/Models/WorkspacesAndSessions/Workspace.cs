using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Serialization;

namespace Netfox.Detective.Models.WorkspacesAndSessions
{
    [KnownType(typeof(DirectoryInfoWrapper))]
    [DataContract(Name = "Workspace", Namespace = "Netfox.Detective.Models.WorkspacesAndSessions")]
    public class Workspace
    {
        public Workspace(string workspaceName, string workspacesStoragePath, string connectionString)
        {
            this.Name = workspaceName;
            this.ConnectionString = connectionString;
            this.Guid = Guid.NewGuid();
            this.Created = DateTime.Now;
            this.LastRecentlyUsed = this.Created;
        }

        public Workspace()
        {
        }

        [DataMember] public string WorkspaceDirectoryPath { get => WorkspaceDirectoryInfo.FullName; set => WorkspaceDirectoryInfo = new System.IO.DirectoryInfo(value); }

        public DirectoryInfoBase WorkspaceDirectoryInfo { get; set; }

        [DataMember] public string Name { get; private set; }

        [DataMember] public string ConnectionString { get; set; }

        [DataMember] public Guid Guid { get; private set; }

        [DataMember] public DateTime Created { get; private set; }

        [DataMember] public DateTime LastRecentlyUsed { get; set; }

        [DataMember]
        public List<String> InvestigationsFilePaths
        {
            get { return this._investigationFilePaths ?? (this._investigationFilePaths = new List<string>()); }
            set { this._investigationFilePaths = value; }
        }

        private List<String> _investigationFilePaths;
        private String _workspaceDirectoryFullPath;

        public override string ToString() => this.Name;
    }
}