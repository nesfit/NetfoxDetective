using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation.Export
{
    public class ExportEmailViewModel : ExportBaseViewModel
    {
        public GridViewDataSet<ExportEmailDTO> ExportObjects { get; set; } = new GridViewDataSet<ExportEmailDTO>();

        public ExportFilterDTO Filters { get; set; } = new ExportFilterDTO();

        public ExportEmailViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) { }

        public override Task PreRender()
        {
            if (!this.Context.IsPostBack)
            {
                this.ExportObjects.PagingOptions.PageSize = 15;
                this.ExportObjects.SortingOptions.SortDescending = false;
                this.ExportObjects.SortingOptions.SortExpression = nameof(ExportEmailDTO.From);
                this.ExportFacade.InitEmailFilter(this.Filters);
                this.Clear();

            }

            if (this.ExportObjects.IsRefreshRequired)
            {
                this.ExportFacade.FillEmailDataSet(this.ExportObjects, this.InvestigationId, this.Filters);
            }

            return base.PreRender();
        }

        public void Filter()
        {
            this.ExportObjects.PagingOptions.PageIndex = 0;
        }

        public void Clear()
        {
            this.Filters.SearchText = String.Empty;
        }

        public void UpdateInvestigationMenu()
        {
            this.ExportObjects.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }
    }
}

