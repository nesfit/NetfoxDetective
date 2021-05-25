using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.Facades;

namespace Netfox.Snoopers.SnooperMAFF.WEB.ViewModels
{
    public class ExportOverviewMAFFViewModel : ExportBaseViewModel
    {

        public ExportOverviewMAFFViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade, SnooperMAFFWeb snooperInfo) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
        }
    }
}
