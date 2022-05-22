using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using Netfox.Web.App.Helpers;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.DAL.Entities;

namespace Netfox.Web.App.ViewModels.Settings
{
    [Authorize(Roles = "Administrator")]
    public class UserViewModel : SettingsViewModel
    {
        private readonly UserFacade userFacade;
        [FromRoute("UserId")]
        public Guid UserId { get; set; }
        public UserDTO User { get; set; }
        public string Password { get; set; }
        public string ListPageRouteName => "Settings_UserManagement";
        public bool IsNewUser => this.UserId == Guid.Empty;
        public override string Title => IsNewUser ? "New User" : "Edit user";
        public IEnumerable<Role> Roles { get; set; }
        public Guid SelectedRoleID { get; set; }

        public override bool ShowToolbar => true;

        public UserViewModel(UserFacade facade)
        {
            this.userFacade = facade;
        }

        public override Task PreRender()
        {
            this.Roles = this.userFacade.GetRoles();
            if (!this.Context.IsPostBack)
            {
                if(this.IsNewUser)
                {
                    User = userFacade.InitializeNew();
                    this.SelectedRoleID = this.Roles.First().Id;
                }
                else
                {
                    User = this.userFacade.GetDetail(UserId);
                    this.SelectedRoleID = User.RoleId;
                }
            }
            return base.PreRender();
        }

        public void Save()
        {
            this.User.RoleId = this.SelectedRoleID;

            if (this.IsNewUser)
            {
                this.userFacade.Save(this.User, this.Password);
            }
            else
            {
                this.userFacade.Save(this.User);
            }

            this.Context.RedirectToRoute("Settings_UserManagement");
        }

        public void Cancel()
        {
            this.Context.RedirectToRoute(ListPageRouteName);
        }
    }
}

