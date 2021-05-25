using System;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Infrastructure;

namespace Netfox.Web.App.ViewModels.Investigation
{
    public class ExportBaseViewModel : LayoutInvestigationViewModel
    {
        public override string Title => "Export";

        public ISnooperWeb SnooperInfo { get; set; }

        public ExportBaseViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) { }
    }
}
