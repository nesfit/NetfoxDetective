using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.Facades;

namespace Netfox.SnooperMessenger.WEB.ViewModels
{
    public class ExportOverviewMessengerViewModel : ExportBaseViewModel
    {

        public ExportOverviewMessengerViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade, SnooperMessengerWeb snooperInfo) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
        }
    }
}
