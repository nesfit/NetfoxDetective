using System;
using System.Collections.Generic;
using DotVVM.BusinessPack.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.App.ViewModels
{
    public abstract class LayoutViewModel : DotvvmViewModelBase
    {
        public string Username => this.Context.GetAuthentication().Context.User.Identity.Name;
        public bool IsAdmin => this.Context.GetAuthentication().Context.User.IsInRole("Administrator");
        public bool IsAuthenticated => this.Context.GetAuthentication().Context.User.Identity.IsAuthenticated;
        public string PageCSSClass => this.Context.Route.RouteName.ToString().ToLower();
        public virtual string ActivePage => this.Context.Route.RouteName;
        public string AppPath => this.Context.Configuration.ApplicationPhysicalPath;
        public abstract string Title { get; }
        public string ErrorMessage { get; set; }

        public void LogOut()
        {
            this.Context.GetAuthentication().SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
            this.Context.RedirectToRoute("login");
        }
    }
}

