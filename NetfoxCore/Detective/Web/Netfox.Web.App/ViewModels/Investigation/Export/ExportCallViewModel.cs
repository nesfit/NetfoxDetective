using System;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation.Export
{
    public class ExportCallViewModel : ExportBaseViewModel
    {
        public GridViewDataSet<ExportCallDTO> Calls { get; set; } = new GridViewDataSet<ExportCallDTO>();

        public GridViewDataSet<ExportCallStreamDTO> Streams { get; set; } = new GridViewDataSet<ExportCallStreamDTO>();

        public ExportFilterDTO CallFilters { get; set; } = new ExportFilterDTO();

        public ExportFilterDTO StreamFilters { get; set; } = new ExportFilterDTO();

        public ExportCallViewModel(InvestigationFacade investigationFacade, HangfireFacade hangfireFacade, CaptureFacade captureFacade, ExportFacade exportFacade) : base(investigationFacade, hangfireFacade, captureFacade, exportFacade) {}

        public override Task PreRender()
        {
            if (!this.Context.IsPostBack)
            {
                this.Calls.PagingOptions.PageSize = 15;
                this.Calls.SortingOptions.SortDescending = false;
                this.Calls.SortingOptions.SortExpression = nameof(ExportCallDTO.FirstSeen);
                this.ExportFacade.InitCallFilter(this.CallFilters, this.InvestigationId);
                this.Streams.PagingOptions.PageSize = 15;
                this.Streams.SortingOptions.SortDescending = false;
                this.Streams.SortingOptions.SortExpression = nameof(ExportCallStreamDTO.FirstSeen);
                this.ExportFacade.InitCallStreamFilter(this.StreamFilters, this.InvestigationId);
                this.Clear();

            }

            if(this.Calls.IsRefreshRequired)
            {
                this.ExportFacade.FillCallDataSet(this.Calls, this.InvestigationId, this.CallFilters);
            }

            if (this.Streams.IsRefreshRequired)
            { 
                this.ExportFacade.FillCallStreamDataSet(this.Streams, this.InvestigationId, this.StreamFilters);
            }

            return base.PreRender();
        }

        public void Filter(string type = "")
        {
            this.Calls.IsRefreshRequired = false;
            this.Streams.IsRefreshRequired = false;
           
            switch (type)
            {
                case "Call":
                    this.Calls.IsRefreshRequired = true;
                    this.Calls.PagingOptions.PageIndex = 0;
                    break;
                case "Stream":
                    this.Streams.IsRefreshRequired = true;
                    this.Streams.PagingOptions.PageIndex = 0;
                    break;
            }
        }

        public void Clear(string type = "")
        {
            switch(type)
            {
                case "Call":
                    this.CallFilters.SearchText = String.Empty;
                    this.CallFilters.DurationTo = this.CallFilters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
                    this.CallFilters.DurationFrom = this.CallFilters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
                    break;
                case "Stream":
                    this.StreamFilters.SearchText = String.Empty;
                    this.StreamFilters.DurationTo = this.CallFilters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
                    this.StreamFilters.DurationFrom = this.CallFilters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
                    break;
                default: 
                    this.CallFilters.SearchText = String.Empty;
                    this.CallFilters.DurationTo = this.CallFilters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
                    this.CallFilters.DurationFrom = this.CallFilters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
                    this.StreamFilters.SearchText = String.Empty;
                    this.StreamFilters.DurationTo = this.CallFilters.DurationMax.ToString("dd.MM.yyyy HH:mm:ss");
                    this.StreamFilters.DurationFrom = this.CallFilters.DurationMin.ToString("dd.MM.yyyy HH:mm:ss");
                    break;
                
            }
        }
        public void UpdateInvestigationMenu()
        {
            this.Calls.IsRefreshRequired = false;
            this.Streams.IsRefreshRequired = false;
            base.UpdateInvestigationMenu();
        }
    }
}

