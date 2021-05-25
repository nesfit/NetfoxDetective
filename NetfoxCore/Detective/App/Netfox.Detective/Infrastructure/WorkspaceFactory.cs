using System;
using System.IO;
using System.IO.Abstractions;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Infrastructure
{
    class WorkspaceFactory : IWorkspaceFactory
    {
        private readonly IFileSystem _fileSystem;

        public WorkspaceFactory(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public Workspace Create(string workspaceName, string workspacesStoragePath, string connectionString)
        {
            var workspacedirectoryInfo =
                this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspacesStoragePath, workspaceName));
            if (workspacedirectoryInfo.Exists)
            {
                throw new InvalidOperationException("InvestigationWorkspace already exists!");
            }

            workspacedirectoryInfo.Create();

            var workspace = new Workspace(workspaceName, workspacesStoragePath, connectionString);
            workspace.WorkspaceDirectoryInfo = (DirectoryInfoBase) workspacedirectoryInfo;

            return workspace;
        }
    }
}