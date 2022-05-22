using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using Netfox.Web.App.Helpers;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigations
{
    public class InvestigationViewModel : InvestigationsViewModel
    {
        [FromRoute("InvestigationId")]
        public Guid InvestigationId { get; set; }

        public InvestigationDTO Investigation { get; set; }

        public string ListPageRouteName => "Investigations_overview";

        public override string Title => IsNewInvestigation? "New Investigation" : "Edit Investigation";

        public FilteredListPageHelper<UserDTO, Guid, UserFilterDTO> InvestigatorsHelper { get; set; }

        public List<Guid> InvestigatorIDs { get; set; } = new List<Guid>();

        public bool IsNewInvestigation => this.InvestigationId == Guid.Empty;

        public bool IsAllSelected { get; set; }

        public IEnumerable<UserDTO> Users { get; set; }

        public Guid OwnerID { get; set; }

        public InvestigationViewModel(InvestigationFacade investigationFacade, UserFacade userFacade) : base(investigationFacade, userFacade)
        {
            this.InvestigatorsHelper = new FilteredListPageHelper<UserDTO, Guid, UserFilterDTO>(this.userFacade)
            {
                DefaultSortOptions = new SortingOptions()
                {
                    SortExpression = nameof(UserDTO.Id)
                }
            };
        }
        //public void SelectAllInvestigators() { this.SelectAll(this.IsAllSelected, this.InvestigatorIDs, this.Investigators); }

        // TODO
        public void Save()
        {
            if (this.IsNewInvestigation)
            {
                this.investigationFacade.AddInvestigation(this.Investigation, this.InvestigatorIDs, this.AppPath);
            }
            else
            {
               this.investigationFacade.Save(this.Investigation, this.InvestigatorIDs);
            }

            this.Context.RedirectToRoute(ListPageRouteName);
        }

        public void Cancel()
        {
            this.Context.RedirectToRoute(ListPageRouteName);
        }

        public override Task Init()
        {
            this.InvestigatorsHelper.Init();
            return base.Init();
        }

        // TODO investigator
        public override Task PreRender()
        {
            if (!this.Context.IsPostBack || this.InvestigatorsHelper.Items.IsRefreshRequired)
            {
                this.InvestigatorsHelper.LoadData();
            }

            if (!this.Context.IsPostBack)
            {
                this.Users = this.userFacade.GetUserList();

                if (this.IsNewInvestigation)
                {
                    this.Investigation = this.investigationFacade.InitializeNew();
                    this.Investigation.OwnerID = this.userFacade.GetUser(this.Username).Id;
                }
                else
                {
                    
                    this.Investigation = this.investigationFacade.GetDetail(this.InvestigationId);
                    //this.OwnerID = this.Investigation.;
                    foreach (var i in this.Investigation.UserInvestigations)
                    {
                        this.InvestigatorIDs.Add(i.UserId);
                    }
                }
            }
            return base.PreRender();
        }
    }
}
