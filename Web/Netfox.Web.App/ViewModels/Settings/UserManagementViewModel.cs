using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using Netfox.Web.App.Helpers;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Settings
{
    [Authorize(Roles = new[] { "Administrator"})]
    public class UserManagementViewModel : SettingsViewModel
    {
        private readonly UserFacade userFacade;

        public override string Title => "User Management";

        public FilteredListPageHelper<UserDTO, Guid, UserFilterDTO> Helper { get; set; }
        
        public List<Guid> UserIDs { get; set; } = new List<Guid>();

        public bool IsAllSelected { get; set; }

        public override bool ShowToolbar => true;

        public UserManagementViewModel(UserFacade facase)
        {
            this.userFacade = facase;
            this.Helper = new FilteredListPageHelper<UserDTO, Guid, UserFilterDTO>(this.userFacade)
            {
                DefaultSortOptions = new SortingOptions()
                {
                    SortExpression = nameof(UserDTO.Id)
                }
            };
        }

        public void SelectAll()
        {
            if (this.IsAllSelected)
            {
                foreach (var i in this.Helper.Items.Items)
                {
                    this.UserIDs.Add(i.Id);
                }
            }
            else { this.UserIDs.Clear(); }
        }

        public void RemoveSelectedUsers()
        {
            foreach(var userID in this.UserIDs)
            {
                this.RemoveUser(userID);
            }
        }
        public void RemoveUser(Guid id)
        {
            this.userFacade.Delete(id);
        }

        public void EditUser(Guid UserId)
        {
           this.Context.Parameters.Add("UserId", UserId.ToString());
           this.Context.RedirectToRoute("Settings_User");
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
                this.Helper.LoadData();
            }
            return base.PreRender();
        }
    }
}

