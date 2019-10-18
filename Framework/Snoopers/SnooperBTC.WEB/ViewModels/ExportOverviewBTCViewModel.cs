using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.Facades;

namespace Netfox.SnooperBTC.WEB.ViewModels
{
    public class ExportOverviewBTCViewModel : ExportBaseViewModel
    {

        public ExportOverviewBTCViewModel(
            InvestigationFacade investigationFacade,
            HangfireFacade hangfireFacade,
            CaptureFacade captureFacade,
            ExportFacade exportFacade,
            SnooperBTCWeb snooperInfo) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
        }
    }
}
