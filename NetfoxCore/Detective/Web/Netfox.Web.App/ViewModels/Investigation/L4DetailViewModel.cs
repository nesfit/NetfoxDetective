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
    public class L4DetailViewModel : BlankLayoutViewModel
    {
        private CaptureFacade CaptureFacade { get; set; }

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        [FromRoute("ConversationId")]
        public Guid ConvessationId { get; set; }

        public L4ConversationDetailDTO Conversation { get; set; }

        public L3ConversationDTO L3Conversation { get; set; }

        public GridViewDataSet<L7ConversationDTO> L7Conversations { get; set; } = new GridViewDataSet<L7ConversationDTO>();

        public GridViewDataSet<PmFrameBaseDTO> Frames { get; set; } = new GridViewDataSet<PmFrameBaseDTO>();

        public L4DetailViewModel(CaptureFacade captureFacade) { this.CaptureFacade = captureFacade; }

        public override Task PreRender()
        {

            if (!Context.IsPostBack)
            {
                this.L7Conversations.PagingOptions.PageSize = 5;
                this.L7Conversations.SortingOptions.SortDescending = false;
                this.L7Conversations.SortingOptions.SortExpression = nameof(L7ConversationDTO.FirstSeen);
                this.Frames.PagingOptions.PageSize = 5;
                this.Frames.SortingOptions.SortDescending = false;
                this.Frames.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.FirstSeen);
            }
            
            this.CaptureFacade.FillL7ConversationDataSet(this.L7Conversations, this.ConvessationId, this.InvestigationId, null, ConversationType.L4);
            this.CaptureFacade.FillPmFrameDataSet(this.Frames, this.ConvessationId, this.InvestigationId, null, ConversationType.L4);

            this.Conversation = this.CaptureFacade.GetL4Conversation(this.InvestigationId, this.ConvessationId);
            this.L3Conversation = this.CaptureFacade.GetL3Conversation(this.InvestigationId, this.ConvessationId);


            return base.PreRender();
        }
    }
}

