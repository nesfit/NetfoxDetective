using System.Collections.Generic;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Interfaces
{
    public interface IWorkspacePathSerializationPersistor : ISerializationPersistor<IEnumerable<WorkspacePath>>
    {
        void Save(IEnumerable<Workspace> workspaces);
        IEnumerable<WorkspacePath> Load();
    }
}