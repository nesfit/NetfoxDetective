using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigations
{
    [Authorize]
    public class InvestigationsViewModel : MasterpageViewModel
    {
        protected readonly InvestigationFacade investigationFacade;

        protected readonly UserFacade userFacade;

        public override string ColumnName => "Investigations";

        public override string ColumnCSSClass => "wrapper-settings lastInvestigation";

        public override bool ShowToolbar => true;

        public GridViewDataSet<InvestigationDTO> LastInvestigationDataset { get; set; }

        public IList<InvestigationDTO> LastInvetigations => this.LastInvestigationDataset.Items;

        public InvestigationsViewModel(InvestigationFacade investigationFacade, UserFacade userFacade)
        {
            this.investigationFacade = investigationFacade;
            this.userFacade = userFacade;
        }
        public override Task Init()
        {
            LastInvestigationDataset = new GridViewDataSet<InvestigationDTO>()
            {
                PagingOptions =
                {
                    PageSize = 5
                },
                SortingOptions = 
                {
                    SortDescending = true,
                    SortExpression = nameof(InvestigationDTO.LastAccess)
                }
            };
            return base.Init();
        }

        public override Task PreRender()
        {
            var user = this.userFacade.GetUser(this.Username);

            this.investigationFacade.FillLastInvestigationDataSet(this.LastInvestigationDataset, user);

            return base.PreRender();
        }
    }
}

