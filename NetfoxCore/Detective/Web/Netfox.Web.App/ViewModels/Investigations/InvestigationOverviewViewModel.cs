using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Netfox.Web.App.Helpers;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Investigations
{
    public class InvestigationOverviewViewModel : InvestigationsViewModel
    {

        public override string Title => "Investigations";

        public FilteredListPageHelper<InvestigationDTO, Guid, InvestigationFilterDTO> Helper { get; set; }

        public List<Guid> InvestigationIDs { get; set; } = new List<Guid>();

        public bool IsAllSelected { get; set; }

        public InvestigationOverviewViewModel( InvestigationFacade investigationFacade, UserFacade userFacade) : base(investigationFacade, userFacade)
        {
            this.Helper = new FilteredListPageHelper<InvestigationDTO, Guid, InvestigationFilterDTO>(this.investigationFacade)
            {
                DefaultSortOptions = new SortingOptions()
                {
                    SortExpression = nameof(InvestigationDTO.Id)
                }
            };
        }

        public void SelectAll()
        {
            if (this.IsAllSelected)
            {
                foreach (var i in this.Helper.Items.Items)
                {
                    this.InvestigationIDs.Add(i.Id);
                }
            }
            else { this.InvestigationIDs.Clear(); }
        }

        public void RemoveSelectedInvestigations()
        {
            foreach (var investigationID in this.InvestigationIDs)
            {
                this.RemoveInvestigation(investigationID);
            }
        }
        public void RemoveInvestigation(Guid id)
        {
            this.investigationFacade.RemoveInvestigation(id, this.AppPath);
        }

        public void EditInvestigation(Guid investigationId)
        {
            this.Context.Parameters.Add("InvestigationId", investigationId.ToString());
            this.Context.RedirectToRoute("Investigation_Edit");
        }

        public bool IsOwenerInvestiogation(string ownerUsername, string username) { return ownerUsername == username; }

        public bool IsActionVisable(string ownerUsername)
        {
            return this.Context.GetAuthentication().Context.User.IsInRole("Administrator") || this.IsOwenerInvestiogation(ownerUsername, this.Username);
        }

        public override Task Init()
        {
            this.Helper.Init();
            return base.Init();
        }

        public override Task PreRender()
        {
            if (!Context.IsPostBack || this.Helper.Items.IsRefreshRequired)
            {
                var currentUser = this.userFacade.GetUser(this.Username);
                this.investigationFacade.FillDataSet(this.Helper.Items, this.Helper.Filter, currentUser);
            }

            foreach (var i in this.Helper.Items.Items)
            {
                //i.CanEditRemove = this.IsActionVisable(i.Owner.Username);
            }
            
            return base.PreRender();
        }
    }
}

