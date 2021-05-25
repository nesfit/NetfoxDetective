using Castle.Windsor;
using Netfox.Detective.Core.BaseTypes;

namespace Netfox.Detective.ViewModelsDataEntity.Exports.Detail
{
    public abstract class DetectiveExportDetailPaneViewModelBase : DetectiveIvestigationDataEntityPaneViewModelBase
    {
        protected DetectiveExportDetailPaneViewModelBase(IWindsorContainer applicationWindsorContainer, ExportVm model,
            object view)
            : base(applicationWindsorContainer, model)
        {
            this.ExportVm = model;
            this.View = view;
            this.ExportVmObserver = new PropertyObserver<ExportVm>(this.ExportVm);
        }

        public override DetectiveDockPosition DockPositionPosition { get; set; } = DetectiveDockPosition.DockedDocument;
        public ExportVm ExportVm { get; }
        protected PropertyObserver<ExportVm> ExportVmObserver { get; }
    }
}