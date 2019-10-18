using System;
using System.Web.Hosting;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;
using Netfox.Web.App.Installers;
using Netfox.Web.BL.Infrastructure;

[assembly: OwinStartup(typeof(Netfox.Web.App.Startup))]
namespace Netfox.Web.App
{
    public class Startup
    {
        private static IWindsorContainer container;

        private static IWindsorContainer InitializeWindsor()
        {
            container = new WindsorContainer();
            container.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(container));
            container.AddFacility<TypedFactoryFacility>();
            container.Install(FromAssembly.Containing<Startup>());

            AutoMapperInstaller.InitAutoMapper(container);

            return container;
        }

        public void Configuration(IAppBuilder app)
        {
            var applicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            InitializeWindsor();
            ConfigureAuth(app);

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(applicationPhysicalPath, debug: IsInDebugMode(), options: options =>
            {
                options.Services.AddSingleton<IViewModelLoader>(serviceProvider => new WindsorViewModelLoader(container));
                options.AddDefaultTempStorages("Temp");
            });

            // use static files
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(applicationPhysicalPath)
            });

            // hangfire
           GlobalConfiguration.Configuration
                .UseSqlServerStorage("NetfoxWebDbContext");
               
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangAuthorizationFilter() }
            });
            app.UseHangfireServer();
        }
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login"),
                Provider = new CookieAuthenticationProvider()
                {
                    OnApplyRedirect = context =>
                    {
                        DotvvmAuthenticationHelper.ApplyRedirectResponse(context.OwinContext, context.RedirectUri);
                    }
                }
            });
        }

		private bool IsInDebugMode()
        {
            #if !DEBUG
			return false;
            #endif
            return false;
        }
    }

    public class HangAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
           
            return true;
        }
    }
}
