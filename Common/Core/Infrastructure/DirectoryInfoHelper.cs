using System;
using System.IO;
using System.IO.Abstractions;

namespace Netfox.Core.Infrastructure
{
    public class DirectoryInfoHelper: IDirectoryInfoHelper
    {
        private readonly IFileSystem _fileSystem;

        public DirectoryInfoHelper(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }
        public DirectoryInfoBase GetWorkspaceDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            return this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspacesStoragePath, workspaceName));
        }

        public DirectoryInfoBase GetInvestigationsDirectoryInfo(string workspaceName, string workspacesStoragePath)
        {
            var workspaceDirectoryInfo = this.GetWorkspaceDirectoryInfo(workspaceName, workspacesStoragePath);
            return this.GetInvestigationsDirectoryInfo(workspaceDirectoryInfo);
        }
        public DirectoryInfoBase GetInvestigationsDirectoryInfo(DirectoryInfoBase workspaceDirectoryInfo)
        {
            return this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(workspaceDirectoryInfo.FullName, "Investigations"));
        }

        public DirectoryInfoBase GetSubdirectory(DirectoryInfoBase directoryInfo, string directory)
        {
            return this._fileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(directoryInfo.FullName, directory));
        }
    }

    public interface IDirectoryInfoHelper
    {
        DirectoryInfoBase GetWorkspaceDirectoryInfo(string workspaceName, string workspacesStoragePath);

        DirectoryInfoBase GetInvestigationsDirectoryInfo(string workspaceName, string workspacesStoragePath);
        DirectoryInfoBase GetInvestigationsDirectoryInfo(DirectoryInfoBase workspaceDirectoryInfo);
        DirectoryInfoBase GetSubdirectory(DirectoryInfoBase directoryInfo, string directory);
    }
}
