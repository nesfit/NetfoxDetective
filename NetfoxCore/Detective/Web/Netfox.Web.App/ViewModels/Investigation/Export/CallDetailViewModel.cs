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
    public class CallDetailViewModel : Netfox.Web.App.ViewModels.BlankLayoutViewModel
    {
        private ExportFacade ExportFacade { get; set; }

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        [FromQuery("call_id")]
        public Guid CallId { get; set; }

        public ExportCallDetailDTO Call { get; set; }

        public GridViewDataSet<ExportCallStreamDTO> Streams { get; set; } = new GridViewDataSet<ExportCallStreamDTO>();

        public CallDetailViewModel(ExportFacade exportFacade) { this.ExportFacade = exportFacade; }

        public override Task PreRender()
        {

            if (!Context.IsPostBack)
            {
                this.Streams.PagingOptions.PageSize = 5;
                this.Streams.SortingOptions.SortDescending = false;
                this.Streams.SortingOptions.SortExpression = nameof(ExportCallStreamDTO.FirstSeen);
            }

            this.Call = this.ExportFacade.GetCall(this.InvestigationId, this.CallId);
            this.ExportFacade.FillCallStreamOfCall(this.Streams, this.InvestigationId, this.Call.RTPAddress);

            return base.PreRender();
        }
    }
}

