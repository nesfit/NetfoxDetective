using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.Facades;

namespace Netfox.Snoopers.SnooperTwitter.WEB.ViewModels
{
    public class ExportOverviewTwitterViewModel : ExportBaseViewModel

    {

        public ExportOverviewTwitterViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade, SnooperTwitterWeb snooperInfo) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
        }
    }
}
