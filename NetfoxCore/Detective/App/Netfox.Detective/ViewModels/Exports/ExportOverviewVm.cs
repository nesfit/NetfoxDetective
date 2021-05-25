using Castle.Windsor;
using GalaSoft.MvvmLight.Threading;
using Netfox.Core.Interfaces.Views;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Detective.ViewModelsDataEntity.Exports;

namespace Netfox.Detective.ViewModels.Exports
{
    public class ExportOverviewVm : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        public ExportOverviewVm(WindsorContainer investigationWindsorContainer, ExportGroupVm model) : base(
            investigationWindsorContainer, model)
        {
            this.ExportGroupVm = model;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                this.View = this.ApplicationOrInvestigationWindsorContainer.Resolve<IExportOverviewView>());
        }

        #region Overrides of DetectivePaneViewModelBase

        public override string HeaderText => ExportGroupVm == null ? "Export overview" : $"Export overview ({ExportGroupVm.Name})";

        #endregion

        public ExportGroupVm ExportGroupVm { get; set; }
    }
}