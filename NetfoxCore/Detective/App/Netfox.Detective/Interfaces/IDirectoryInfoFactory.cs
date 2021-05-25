using System.IO.Abstractions;
using PostSharp.Patterns.Contracts;

namespace Netfox.Detective.Interfaces
{
    public interface IDirectoryInfoFactory
    {
        DirectoryInfoBase CreateWorkspaceDirectoryInfo([NotEmpty] string workspaceName,
            [NotEmpty] string workspacesStoragePath);

        DirectoryInfoBase CreateInvestigationsDirectoryInfo([NotEmpty] string workspaceName,
            [NotEmpty] string workspacesStoragePath);

        DirectoryInfoBase CreateInvestigationsDirectoryInfo([System.Diagnostics.CodeAnalysis.NotNull] DirectoryInfoBase workspaceDirectoryInfo);

        DirectoryInfoBase CreateSubdirectoryInfo([System.Diagnostics.CodeAnalysis.NotNull] DirectoryInfoBase directoryInfo,
            [NotEmpty] string directory);
    }
}