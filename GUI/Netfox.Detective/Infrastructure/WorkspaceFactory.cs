// Copyright (c) 2018 Hana Slamova
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
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Infrastructure
{
    class WorkspaceFactory:IWorkspaceFactory
    {
        private readonly IFileSystem _fileSystem;

        public WorkspaceFactory(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public Workspace Create(string workspaceName, string workspacesStoragePath, string connectionString)
        { 
            var workspacedirectoryInfo = this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspacesStoragePath, workspaceName));
            if (workspacedirectoryInfo.Exists) { throw new InvalidOperationException("InvestigationWorkspace already exists!"); }
            workspacedirectoryInfo.Create();
          
            var workspace = new Workspace(workspaceName, workspacesStoragePath, connectionString);
            workspace.WorkspaceDirectoryInfo = workspacedirectoryInfo;

            return workspace;
        }

    }
}
