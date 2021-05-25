using Castle.Windsor;
using Netfox.Core.Models.Exports;

namespace Netfox.Detective.ViewModelsDataEntity.Exports
{
    public class ExportReportVm : DetectiveDataEntityViewModelBase
    {
        public ExportReportVm(ExportReport model, ExportVm exportResultVm,
            IWindsorContainer investigationWindsorContainer) : base(investigationWindsorContainer, model)
        {
            this.ExportReport = model;
            this.ExportVm = exportResultVm;
        }

        public ExportReport ExportReport { get; }
        public ExportVm ExportVm { get; }
    }
}