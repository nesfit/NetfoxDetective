// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Windows.Input;

namespace Netfox.Detective.Views.Resources
{
    public class NullCommand : ICommand
    {
        #region Implementation of ICommand
        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter) => false;

        /// <summary>Defines the method to be called when the command is invoked.</summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter) { throw new NotImplementedException(); }

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
        #endregion
    }
}
