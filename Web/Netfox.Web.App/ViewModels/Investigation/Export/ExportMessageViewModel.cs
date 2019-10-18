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
    public class ExportMessageViewModel : ExportBaseViewModel
    {
        public GridViewDataSet<ExportChatMessageDTO> ExportObjects { get; set; } = new GridViewDataSet<ExportChatMessageDTO>();

        public ExportFilterDTO Filters { get; set; } = new ExportFilterDTO();

        public ExportMessageViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) { }

        public override Task PreRender()
        {
            if (!this.Context.IsPostBack)
            {
                this.ExportObjects.PagingOptions.PageSize = 15;
                this.ExportObjects.SortingOptions.SortDescending = false;
                this.ExportObjects.SortingOptions.SortExpression = nameof(ExportChatMessageDTO.FirstSeen);
                this.ExportFacade.InitChatMessageFilter(this.Filters, this.InvestigationId);
                this.Clear();

            }

            if(this.ExportObjects.IsRefreshRequired)
            {
                this.ExportFacade.FillChatMessageDataSet(this.ExportObjects, this.InvestigationId, this.Filters);
            }

            return base.PreRender();
        }

        public void Filter() { }

        public void Clear()
        {
            this.Filters.SearchText = String.Empty;
            this.Filters.DurationTo = this.Filters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
            this.Filters.DurationFrom = this.Filters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public void UpdateInvestigationMenu()
        {
            this.ExportObjects.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }
    }
}

