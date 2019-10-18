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
using System.IO.Abstractions;
using System.Windows.Input;
using Netfox.Detective.Infrastructure;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Workspaces;

namespace Netfox.Detective.Commands.Workspaces
{
    public class CreateWorkspaceCommand:ICommand
    {

        private readonly IWorkspaceFactory _workspaceFactory;
        private readonly ISerializationPersistor<Models.WorkspacesAndSessions.Workspace> _workspaceSerializationPersistor;
        private readonly IDetectiveMessenger _messenger;
        private readonly IFileSystem _fileSystem;
        public CreateWorkspaceCommand(IWorkspaceFactory workspaceFactory,ISerializationPersistor<Models.WorkspacesAndSessions.Workspace> workpaceSerializationPersistor,
            IDetectiveMessenger messenger, IFileSystem fileSystem)
        {
            this._workspaceFactory = workspaceFactory ?? throw new ArgumentNullException(nameof(workspaceFactory));
            this._workspaceSerializationPersistor = workpaceSerializationPersistor ?? throw new ArgumentNullException(nameof(workpaceSerializationPersistor));
            this._messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            this._fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public bool CanExecute(object parameter)
        {
            var isCreateWorkspaceCommandParamsType = parameter is CreateWorkspaceCommandParams;

            if(isCreateWorkspaceCommandParamsType)
            {
                var parameters = parameter as CreateWorkspaceCommandParams;

                return !string.IsNullOrEmpty(parameters.Name) && !string.IsNullOrEmpty(parameters.StoragePath);
            }
            else { return false; }
        }

        public void Execute(object parameter)
        {
            var input = parameter as CreateWorkspaceCommandParams;
            var workspace = this._workspaceFactory.Create(input.Name, input.StoragePath, input.ConnectionString);

            new DirectoryInfoFactory(this._fileSystem).CreateInvestigationsDirectoryInfo(workspace.WorkspaceDirectoryInfo).Create();

            this._workspaceSerializationPersistor.Save(workspace);

            this._messenger.Send(new CreatedWorkspaceMessage
            {
                Workspace = workspace
            });
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public class CreateWorkspaceCommandParams
        {
            public string Name { get; set; }
            public string StoragePath { get; set; }
            public string ConnectionString { get; set; }
        }
    }
}
