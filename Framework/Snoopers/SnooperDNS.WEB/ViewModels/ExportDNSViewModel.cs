using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using Netfox.SnooperDNS.WEB.DTO;
using Netfox.SnooperDNS.WEB.Facade;
using Netfox.Web.App.ViewModels.Investigation;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.SnooperDNS.WEB.ViewModels
{
    public class ExportDNSViewModel : ExportBaseViewModel
    {
        public ExportFilterDTO Filters { get; set; } = new ExportFilterDTO();

        private ExportDNSFacade ExportDNSFacade { get; set; }

        public GridViewDataSet<SnooperDNSListDTO> ExportObjects { get; set; } = new GridViewDataSet<SnooperDNSListDTO>();

        public ExportDNSViewModel(
            InvestigationFacade investigationFacade,
            HangfireFacade hangfireFacade,
            CaptureFacade captureFacade,
            ExportFacade exportFacade,
            SnooperDNSWeb snooperInfo,
            ExportDNSFacade exportFTPFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade)
        {
            this.SnooperInfo = snooperInfo;
            this.ExportDNSFacade = exportFTPFacade;
        }

        #region Overrides of ExportBaseViewModel
        public override Task PreRender()
        {
            if(!this.Context.IsPostBack)
            {
                this.ExportObjects.PagingOptions.PageSize = 15;
                this.ExportObjects.SortingOptions.SortDescending = false;
                this.ExportObjects.SortingOptions.SortExpression = nameof(SnooperDNSListDTO.FirstSeen);
                this.ExportDNSFacade.InitFilter(this.Filters, this.InvestigationId);
                this.Clear();
            }

            if(this.ExportObjects.IsRefreshRequired)
            {
                this.ExportDNSFacade.FillMessages(this.InvestigationId, this.ExportObjects, this.Filters);
            }
            return base.PreRender();
        }
        #endregion

        public void Filter()
        {
           
            this.ExportObjects.IsRefreshRequired = true;
            this.ExportObjects.PagingOptions.PageIndex = 0;
            
        }

        public void Clear()
        {
            this.Filters.SearchText = "";
            this.Filters.DurationTo = this.Filters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
            this.Filters.DurationFrom = Filters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
            this.Filter();
        }

        public void UpdateInvestigationMenu()
        {
            this.ExportObjects.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }

    }
}

