using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigation
{
    public class L7DetailViewModel : Netfox.Web.App.ViewModels.BlankLayoutViewModel
    {
        private CaptureFacade CaptureFacade { get; set; }

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        [FromRoute("ConversationId")]
        public Guid ConvessationId { get; set; }

        public L7ConversationDetailDTO Conversation { get; set; }

        public GridViewDataSet<PmFrameBaseDTO> Frames { get; set; } = new GridViewDataSet<PmFrameBaseDTO>();

        public L7DetailViewModel(CaptureFacade captureFacade) { this.CaptureFacade = captureFacade; }

        public override Task PreRender()
        {

            if (!Context.IsPostBack)
            {
                this.Frames.PagingOptions.PageSize = 5;
                this.Frames.SortingOptions.SortDescending = false;
                this.Frames.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.FirstSeen);
            }

            this.CaptureFacade.FillPmFrameDataSet(this.Frames, this.ConvessationId, this.InvestigationId, null, ConversationType.L7);
            this.Conversation = this.CaptureFacade.GetL7Conversation(this.InvestigationId, this.ConvessationId);

            return base.PreRender();
        }
    }
}

