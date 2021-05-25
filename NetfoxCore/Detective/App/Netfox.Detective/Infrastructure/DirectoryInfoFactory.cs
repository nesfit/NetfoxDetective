using System;
using System.IO;
using System.IO.Abstractions;
using IDirectoryInfoFactory = Netfox.Detective.Interfaces.IDirectoryInfoFactory;


namespace Netfox.Detective.Infrastructure
{
    public class DirectoryInfoFactory : IDirectoryInfoFactory
    {
        private IFileSystem _fileSystem;

        public DirectoryInfoFactory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public DirectoryInfoBase CreateWorkspaceDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            return (DirectoryInfoBase) _fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspacesStoragePath,
                workspaceName));
        }

        public DirectoryInfoBase CreateInvestigationsDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            var workspaceDirectoryInfo = this.CreateWorkspaceDirectoryInfo(workspaceName, workspacesStoragePath);
            return this.CreateInvestigationsDirectoryInfo(workspaceDirectoryInfo);
        }

        public DirectoryInfoBase CreateInvestigationsDirectoryInfo(DirectoryInfoBase workspaceDirectoryInfo)
        {
            return (DirectoryInfoBase) this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(
                workspaceDirectoryInfo?.FullName,
                "Investigations"));
        }

        public DirectoryInfoBase CreateSubdirectoryInfo(DirectoryInfoBase directoryInfo, string directory)
        {
            return (DirectoryInfoBase) this._fileSystem.DirectoryInfo.FromDirectoryName(
                Path.Combine(directoryInfo?.FullName, directory));
        }
    }
}