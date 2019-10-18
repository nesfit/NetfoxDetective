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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Core.Infrastructure;
using Netfox.Detective.Interfaces;
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
