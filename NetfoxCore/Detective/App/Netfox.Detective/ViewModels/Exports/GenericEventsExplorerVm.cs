using System.Threading.Tasks;
using Castle.Windsor;
using Netfox.Detective.Core.BaseTypes;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages.Exports;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports
{
    public class GenericEventsExplorerVm : DetectiveApplicationPaneViewModelBase
    {
        public GenericEventsExplorerVm(WindsorContainer applicationWindsorContainer) : base(applicationWindsorContainer)
        {
            this.DockPositionPosition = DetectiveDockPosition.DockedLeft;
            var messenger = applicationWindsorContainer.Resolve<IDetectiveMessenger>();

            Parallel.Invoke(() =>
                messenger.Register<SelectedExportResultMessage>(this, this.ExportResultActionHandler));
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => "All events";

        #endregion

        public ExportVm ExportResult { get; set; }

        private void ExportResultActionHandler(SelectedExportResultMessage exportResultMessage)
        {
            this.ExportResult = exportResultMessage.ExportVm as ExportVm;
        }
    }
}