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
using Netfox.Core.Models;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages;
using Netfox.Detective.Messages.Investigations;

namespace Netfox.Detective.Commands.Investigations
{
    public class CreateInvestigationCommand: IAsyncCommand
    {
        private readonly IDetectiveMessenger _messenger;
        private readonly IInvestigationFactory _investigationFactory;
        public CreateInvestigationCommand(IDetectiveMessenger messenger, IInvestigationFactory investigationFactory)
        {
            this._messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            this._investigationFactory = investigationFactory ?? throw new ArgumentNullException(nameof(investigationFactory));
        }

        public bool CanExecute(object parameter)
        {
            if(parameter is CreateInvestigationCommandParams)
            {
                var parameters = parameter as CreateInvestigationCommandParams;

                return !string.IsNullOrEmpty(parameters.Name);

            }
            else { return false; }
        }

        public void Execute(object parameter)
        {
            var parameters = parameter as CreateInvestigationCommandParams;

            var investigation =  this._investigationFactory.Create(parameters.InvestigationInfo);
            investigation.Wait();
            

            this._messenger.Send(new CreatedInvestigationMessage
            {
                Investigation = investigation.Result
        });
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public async Task ExecuteAsync(object parameter)
        {
            var parameters = parameter as CreateInvestigationCommandParams;

            var investigation = await this._investigationFactory.Create(parameters.InvestigationInfo);

            this._messenger.Send(new CreatedInvestigationMessage
            {
                Investigation = investigation
            });
        }

        public class CreateInvestigationCommandParams
        {
            public string  Name { get; set; }
            public InvestigationInfo InvestigationInfo { get; set; }
            
        }
    }
}
