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
        public Workspace() { }

        [DataMember]
        public DirectoryInfoBase WorkspaceDirectoryInfo { get; set; }

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