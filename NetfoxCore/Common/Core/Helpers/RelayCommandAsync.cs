using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Netfox.Core.Helpers
{
    public class RelayCommandAsync : CommandBase
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task> _command;
        private readonly Func<CancellationToken, Task> _commandCancellable;

        public RelayCommandAsync(Func<Task> command)
        {
            this._canExecute = () => true;
            this._command = command;
        }

        public RelayCommandAsync(Func<Task> command, Func<bool> canExecute)
        {
            this._canExecute = canExecute;
            this._command = command;
        }

        public RelayCommandAsync(Func<CancellationToken, Task> commandCancellable, Func<bool> canExecute)
        {
            this._canExecute = canExecute;
            this._commandCancellable = commandCancellable;
        }

        protected RelayCommandAsync()
        {
        }

        public NotifyTaskCompletion Execution { get; protected set; }

        public ICommand CancelCommand => this.CancelCommandInternal;
        protected CancelAsyncCommand CancelCommandInternal { get; } = new CancelAsyncCommand();

        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public override bool CanExecute(object parameter = null) =>
            this._canExecute() && !this.CancelCommandInternal.CommandExecuting;

        public sealed override async void Execute(object parameter = null)
        {
            try
            {
                await this.ExecuteAsync(parameter);
            }
            catch (Exception)
            {
                this.RaiseCanExecuteChanged();
                throw;
            }
        }

        public async Task ExecuteAsync(object parameter)
        {
            this.CancelCommandInternal.NotifyCommandStarting();
            this.AssignExecution(parameter);
            this.RaiseCanExecuteChanged();
            await this.ExecuteTask(parameter);
            this.CancelCommandInternal.NotifyCommandFinished();
            this.RaiseCanExecuteChanged();
        }

        protected virtual void AssignExecution(object parameter)
        {
            if (this._command != null) this.Execution = new NotifyTaskCompletion(this._command(), false);
            else if (this._commandCancellable != null)
                this.Execution = new NotifyTaskCompletion(this._commandCancellable(this.CancelCommandInternal.Token),
                    this.RaiseCanExecuteChanged);
        }

        protected virtual async Task ExecuteTask(object parameter)
        {
            await this.Execution;
        }

        protected sealed class CancelAsyncCommand : CommandBase
        {
            public bool CommandExecuting { get; private set; }
            private CancellationTokenSource _cts = new CancellationTokenSource();
            public CancellationToken Token => this._cts.Token;

            public override bool CanExecute(object parameter)
            {
                return this.CommandExecuting && !this._cts.IsCancellationRequested;
            }

            public override void Execute(object parameter)
            {
                this._cts.Cancel();
                this.RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished()
            {
                this.CommandExecuting = false;
                this.RaiseCanExecuteChanged();
            }

            public void NotifyCommandStarting()
            {
                this.CommandExecuting = true;
                if (!this._cts.IsCancellationRequested) return;
                this._cts = new CancellationTokenSource();
                this.RaiseCanExecuteChanged();
            }
        }
    }

    public class RelayCommandAsync<TResult> : RelayCommandAsync where TResult : class
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Func<TResult, CancellationToken, Task> _commandParrametrized;

        public RelayCommandAsync(Func<TResult, CancellationToken, Task> commandParrametrized,
            Func<object, bool> canExecute)
        {
            this._canExecute = canExecute;
            this._commandParrametrized = commandParrametrized;
        }

        public new NotifyTaskCompletion<TResult> Execution => base.Execution as NotifyTaskCompletion<TResult>;

        protected override void AssignExecution(object parameter)
        {
            base.Execution =
                new NotifyTaskCompletion(
                    this._commandParrametrized(parameter as TResult, this.CancelCommandInternal.Token),
                    this.RaiseCanExecuteChanged);
        }


        public override bool CanExecute(object parameter = null)
        {
            return this._canExecute(parameter) && !this.CancelCommandInternal.CommandExecuting;
        }
    }
}