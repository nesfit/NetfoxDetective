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
    public class L3DetailViewModel : BlankLayoutViewModel
    {
        private CaptureFacade CaptureFacade { get; set; }

        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        [FromRoute("ConversationId")]
        public Guid ConvessationId { get; set; }

        public L3ConversationDTO Conversation { get; set; }

        public GridViewDataSet<L4ConversationDTO> L4Conversations { get; set; } = new GridViewDataSet<L4ConversationDTO>();

        public GridViewDataSet<L7ConversationDTO> L7Conversations { get; set; } = new GridViewDataSet<L7ConversationDTO>();

        public GridViewDataSet<PmFrameBaseDTO> Frames { get; set; } = new GridViewDataSet<PmFrameBaseDTO>();

        public L3DetailViewModel(CaptureFacade captureFacade) { this.CaptureFacade = captureFacade; }

        public override Task PreRender()
        {

            if(!Context.IsPostBack)
            {
                this.L4Conversations.PagingOptions.PageSize = 5;
                this.L4Conversations.SortingOptions.SortDescending = false;
                this.L4Conversations.SortingOptions.SortExpression = nameof(L4ConversationDTO.FirstSeen);
                this.L7Conversations.PagingOptions.PageSize = 5;
                this.L7Conversations.SortingOptions.SortDescending = false;
                this.L7Conversations.SortingOptions.SortExpression = nameof(L7ConversationDTO.FirstSeen);
                this.Frames.PagingOptions.PageSize = 5;
                this.Frames.SortingOptions.SortDescending = false;
                this.Frames.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.FirstSeen);
            }
            this.CaptureFacade.FillL4ConversationDataSet(this.L4Conversations, this.ConvessationId, this.InvestigationId, null, ConversationType.L3);
            this.CaptureFacade.FillL7ConversationDataSet(this.L7Conversations, this.ConvessationId, this.InvestigationId, null, ConversationType.L3);
            this.CaptureFacade.FillPmFrameDataSet(this.Frames, this.ConvessationId, this.InvestigationId, null, ConversationType.L3);

            this.Conversation = this.CaptureFacade.GetL3Conversation(this.InvestigationId, this.ConvessationId);


            return base.PreRender();
        }
    }
}

