using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Interfaces
{
    public interface IWorkspaceFactory
    {
        Workspace Create(string workspaceName, string workspacesStoragePath, string connectionString);
    }
}