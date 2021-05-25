using System;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels
{
    [Authorize]
    public class DefaultViewModel : BaseLayoutViewModel
    {
        private readonly InvestigationFacade investigationFacade;
    
        private readonly UserFacade userFacade;

        public override string Title => "Dashboard";

        public GridViewDataSet<InvestigationDTO> LastInvestigation { get; set; }

        public DefaultViewModel(InvestigationFacade investigationFacade, UserFacade userFacade)
		{
		    this.investigationFacade = investigationFacade;
		    this.userFacade = userFacade;
		}

        public void RedirectToInvestigation(Guid investigationId)
        {
            this.Context.RedirectToRoute("Investigation", new {InvestigationId = investigationId});
        }

        public override Task Init()
        {
            LastInvestigation = new GridViewDataSet<InvestigationDTO>()
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
            this.investigationFacade.FillLastInvestigationDataSet(LastInvestigation, user);

            return base.PreRender();
        }

    }
}
