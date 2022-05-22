using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.Facades;

namespace Netfox.Snoopers.SnooperWebmails.WEB.ViewModels
{
    public class ExportOverviewWebmailsViewModel : ExportBaseViewModel
    {
      
        public ExportOverviewWebmailsViewModel(
            InvestigationFacade investigationFacade,
            HangfireFacade hangfireFacade,
            CaptureFacade captureFacade,
            ExportFacade exportFacade,
            SnooperWebmailsWeb snooperInfo) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
        }
    }
}
