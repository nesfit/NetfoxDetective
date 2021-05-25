using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Filters;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;

namespace Netfox.Web.App.ViewModels.Settings
{
    [Authorize]
    public class ProfileViewModel : SettingsViewModel
    {
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        public string NewPassword { get; set; }

        public bool IsPasswordDialogDisplayed { get; set; } = false;

        public UserDTO User { get; set; }

        public string ErrorMessage { get; set; }

        public override string Title => "User Profile";

        public override bool ShowToolbar => true;

        private UserFacade Facade { get; set; }

        public ProfileViewModel(UserFacade facade)
        {
            this.Facade = facade;
        }

        public override Task PreRender()
        {
            this.User = this.Facade.GetUser(this.Username);
            return base.PreRender();
        }

        public void Cancel()
        {
            this.IsPasswordDialogDisplayed = false;
            this.ErrorMessage = null;
        }

        public void ChangePassword()
        {
            
            if(this.Facade.ChangePassword(this.User.Id, LoginFacade.SHA256Hash(this.Password), LoginFacade.SHA256Hash(this.NewPassword)))
            {
                this.IsPasswordDialogDisplayed = false;
                return;
            }

            this.ErrorMessage = "Old password is not valid!";
        }

    }
}

