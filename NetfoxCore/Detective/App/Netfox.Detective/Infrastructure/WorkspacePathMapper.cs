using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Netfox.Core.Infrastructure;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Infrastructure
{
    public class WorkspacePathMapper
    {
        private readonly INetfoxSettings _netfoxSettings;
        public WorkspacePathMapper(INetfoxSettings netfoxSettings) { this._netfoxSettings = netfoxSettings ?? throw new ArgumentNullException(nameof(netfoxSettings)); }

        public IEnumerable<WorkspacePath> FromWorkspace(IEnumerable<Workspace> workspaces)
        {
            if(workspaces == null)
            {
                throw new ArgumentNullException(nameof(workspaces));
            }

            return workspaces.Select(w => new WorkspacePath
            {
                Path = Path.Combine(w.WorkspaceDirectoryInfo.FullName, w.Name + "." + this._netfoxSettings.WorkspaceFileExtension)
            });
        }
    }
}