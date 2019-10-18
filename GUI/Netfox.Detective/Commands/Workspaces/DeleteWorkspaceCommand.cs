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
using System.Threading.Tasks;
using System.Windows.Input;
using Castle.Core.Logging;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Workspaces;
using Netfox.Detective.Models.WorkspacesAndSessions;

namespace Netfox.Detective.Commands.Workspaces
{
    class DeleteWorkspaceCommand: ICommand
    {
        private readonly IDetectiveMessenger _messenger;
        private readonly IDirectoryWrapper _directoryWrapper;
        private readonly ILogger _logger;

        public DeleteWorkspaceCommand(IDetectiveMessenger messenger, IDirectoryWrapper directoryWrapper,ILogger logger)
        {
            this._messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            this._directoryWrapper = directoryWrapper ?? throw new ArgumentNullException(nameof(directoryWrapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanExecute(object parameter) { return parameter is Workspace;}

        public void Execute(object parameter)
        {
            var workspace = parameter as Models.WorkspacesAndSessions.Workspace;

            this._logger?.Info($"Workspace deleted: {workspace.Name}");

            Task.Factory.StartNew(() => this._directoryWrapper.Delete(workspace.WorkspaceDirectoryInfo.FullName));

            this._messenger.Send(new DeletedWorkspaceMessage{Workspace = workspace});
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


    }
}
