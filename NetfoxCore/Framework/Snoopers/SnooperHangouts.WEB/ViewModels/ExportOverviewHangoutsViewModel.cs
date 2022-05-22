using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.Facades;

namespace Netfox.Snoopers.SnooperHangouts.WEB.ViewModels
{
    public class ExportOverviewHangoutsViewModel : ExportBaseViewModel
    {
      
        public ExportOverviewHangoutsViewModel(
            InvestigationFacade investigationFacade,
            HangfireFacade hangfireFacade,
            CaptureFacade captureFacade,
            ExportFacade exportFacade,
            SnooperHangoutsWeb snooperInfo
            ) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
        }
    }
}
