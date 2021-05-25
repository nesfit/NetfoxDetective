using Castle.Windsor;
using GalaSoft.MvvmLight.Command;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports.ExportsControls
{
    public class CallsVm: DetectiveViewModelBase
    {
        private RelayCommand<ExportResultHelper.Call> _callsDataGridCommand;
        private readonly IDetectiveMessenger _messenger;

        public CallsVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this._messenger = applicationWindsorContainer.Resolve<IDetectiveMessenger>();
        }

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
                        this._messenger.AsyncSend(new SelectedExportResultMessage
                        {
                            ExportVm = call.ResultVm
                        });
                    }
                }));
            }
        }
    }
}