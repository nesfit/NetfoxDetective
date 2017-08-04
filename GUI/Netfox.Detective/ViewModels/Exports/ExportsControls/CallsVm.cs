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

using Castle.Windsor;
using GalaSoft.MvvmLight.CommandWpf;
using Netfox.Core.Messages.Exports;
using Netfox.Core.Messages.Views;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports.ExportsControls
{
    public class CallsVm : DetectiveViewModelBase
    {
        private RelayCommand<ExportResultHelper.Call> _callsDataGridCommand;

        public CallsVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer) { }

        public RelayCommand<ExportResultHelper.Call> CallsDataGridCommand
        {
            get
            {
                return this._callsDataGridCommand ?? (this._callsDataGridCommand = new RelayCommand<ExportResultHelper.Call>(messagesDataGrid =>
                {
                    var call = messagesDataGrid;

                    if(call != null && call.ResultVm != null)
                    {
                        call.ResultVm.SelectDataByDataObject(call.DataVm, true);
                        ExportResultMessage.SendExportResultMessage(call.ResultVm, ExportResultMessage.MessageType.ExportResultSelected);
                        BringToFrontMessage.SendBringToFrontMessage("ExportContentView");
                    }
                }));
            }
        }
    }
}