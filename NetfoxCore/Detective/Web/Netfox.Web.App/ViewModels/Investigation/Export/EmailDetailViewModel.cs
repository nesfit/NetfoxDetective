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
    public class EmailDetailViewModel : BlankLayoutViewModel
    {
        private ExportFacade ExportFacade { get; set; }

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        [FromQuery("email_id")]
        public Guid EmailId { get; set; }

        public ExportEmailDTO Email { get; set; }

        public bool ShowFiles => Attachments.PagingOptions.TotalItemsCount > 0;

        public GridViewDataSet<EmailAttachmentDTO> Attachments { get; set; } = new GridViewDataSet<EmailAttachmentDTO>();

        public EmailDetailViewModel(ExportFacade exportFacade) { this.ExportFacade = exportFacade; }

        public override Task PreRender()
        {

            if (!Context.IsPostBack)
            {
                this.Attachments.PagingOptions.PageSize = 5;
                this.Attachments.SortingOptions.SortDescending = false;
                this.Attachments.SortingOptions.SortExpression = nameof(ExportEmailDTO.Date);
            }

            this.Email = this.ExportFacade.GetEmail(this.InvestigationId, this.EmailId);
            this.ExportFacade.FillAttachmentsOfEmail(this.Attachments, this.InvestigationId, this.EmailId);
            this.ExportFacade.GetAddressOfEmail(this.Email, this.EmailId, this.InvestigationId);

            return base.PreRender();
        }
    }
}

