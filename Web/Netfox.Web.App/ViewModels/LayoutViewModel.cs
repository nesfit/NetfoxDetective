using System;
using System.Collections.Generic;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.App.ViewModels
{
    public abstract class LayoutViewModel : DotvvmViewModelBase
    {
        public string Username => this.Context.GetAuthentication().User.Identity.Name;
        public bool IsAdmin => this.Context.GetAuthentication().User.IsInRole("Administrator");
        public bool IsAuthenticated => this.Context.GetAuthentication().User.Identity.IsAuthenticated;
        public string PageCSSClass => this.Context.Route.RouteName.ToString().ToLower();
        public virtual string ActivePage => this.Context.Route.RouteName;
        public string AppPath => this.Context.Configuration.ApplicationPhysicalPath;
        public abstract string Title { get; }
        public string ErrorMessage { get; set; }

        public void LogOut()
        {
            this.Context.GetAuthentication().SignOut();
            this.Context.RedirectToRoute("login");
        }
    }
}

