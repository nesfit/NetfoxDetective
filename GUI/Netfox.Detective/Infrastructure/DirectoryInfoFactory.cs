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
using System.IO;
using System.IO.Abstractions;
using PostSharp.Patterns.Contracts;

namespace Netfox.Detective.Infrastructure
{
    public class DirectoryInfoFactory: IDirectoryInfoFactory
    {
        private IFileSystem _fileSystem;

        public DirectoryInfoFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public DirectoryInfoBase CreateWorkspaceDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            return _fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspacesStoragePath, workspaceName));
        }
        
        public DirectoryInfoBase CreateInvestigationsDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            var workspaceDirectoryInfo = this.CreateWorkspaceDirectoryInfo(workspaceName, workspacesStoragePath);
            return this.CreateInvestigationsDirectoryInfo(workspaceDirectoryInfo);
        }

        public DirectoryInfoBase CreateInvestigationsDirectoryInfo(DirectoryInfoBase workspaceDirectoryInfo)
        {
            return this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspaceDirectoryInfo.FullName, "Investigations"));
        }

        public DirectoryInfoBase CreateSubdirectoryInfo(DirectoryInfoBase directoryInfo, string directory)
        {
            return this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(directoryInfo.FullName, directory));
        }
    }

    public interface IDirectoryInfoFactory
    {
        DirectoryInfoBase CreateWorkspaceDirectoryInfo([NotEmpty] string workspaceName, [NotEmpty] string workspacesStoragePath);
        DirectoryInfoBase CreateInvestigationsDirectoryInfo([NotEmpty] string workspaceName, [NotEmpty] string workspacesStoragePath);
        DirectoryInfoBase CreateInvestigationsDirectoryInfo([NotNull] DirectoryInfoBase workspaceDirectoryInfo);
        DirectoryInfoBase CreateSubdirectoryInfo([NotNull] DirectoryInfoBase directoryInfo, [NotEmpty] string directory);
    }
}
