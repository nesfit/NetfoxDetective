using System;
using System.Windows.Input;

namespace Netfox.Core.Helpers
{
    public abstract class CommandBase : ICommand
    {
        public abstract bool CanExecute(object parameter);

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>Defines the method to be called when the command is invoked.</summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public abstract void Execute(object parameter);

        public void RaiseCanExecuteChanged() { CommandManager.InvalidateRequerySuggested(); }
    }
}