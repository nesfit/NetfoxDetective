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
    class DeleteWorkspaceCommand : ICommand
    {
        private readonly IDetectiveMessenger _messenger;
        private readonly IDirectoryWrapper _directoryWrapper;
        private readonly ILogger _logger;

        public DeleteWorkspaceCommand(IDetectiveMessenger messenger, IDirectoryWrapper directoryWrapper, ILogger logger)
        {
            this._messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            this._directoryWrapper = directoryWrapper ?? throw new ArgumentNullException(nameof(directoryWrapper));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanExecute(object parameter)
        {
            return parameter is Workspace;
        }

        public void Execute(object parameter)
        {
            var workspace = parameter as Models.WorkspacesAndSessions.Workspace;

            this._logger?.Info($"Workspace deleted: {workspace.Name}");

            Task.Factory.StartNew(() => this._directoryWrapper.Delete(workspace.WorkspaceDirectoryInfo.FullName));

            this._messenger.Send(new DeletedWorkspaceMessage {Workspace = workspace});
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}