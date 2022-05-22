using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ViewModel.Serialization;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Netfox.Core.Infrastructure;
using Netfox.Web.App.Installers;
using Netfox.Web.App.Settings;
using Netfox.Web.BL.Installers;
using Netfox.Web.BL.Settings;

namespace Netfox.Web.App
{
    public class AspStartup
    {
        private readonly IConfiguration _config;
        private readonly IWindsorContainer _container;

        public AspStartup(IConfiguration config)
        {
            _config = config;
            _container = new WindsorContainer();
            _container.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(_container));
            _container.AddFacility<TypedFactoryFacility>();
            _container.Install(FromAssembly.Containing<AspStartup>());
            
            AutoMapperInstaller.InitMapper(_container);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.LoginPath = new PathString("/Login"); 
                    opt.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToReturnUrl = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri),
                        OnRedirectToAccessDenied = c => DotvvmAuthenticationHelper.ApplyStatusCodeResponse(c.HttpContext, 403),
                        OnRedirectToLogin = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri),
                        OnRedirectToLogout = c => DotvvmAuthenticationHelper.ApplyRedirectResponse(c.HttpContext, c.RedirectUri)
                    };
                });

            services.AddDotVVM(new DotvvmConfig(_container));

            services.AddHangfire(opt =>
                opt.UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(_config.GetConnectionString("NetfoxWebDbContext"), new SqlServerStorageOptions
                    {
                        UseRecommendedIsolationLevel = true
                    }));

            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            
            app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);

            app.UseStaticFiles(new StaticFileOptions {FileProvider = new PhysicalFileProvider(env.ContentRootPath)});

            app.UseHangfireDashboard(options: new DashboardOptions {Authorization = new[] {new NoAuthFilter()}});
            app.UseHangfireServer();
        }

        private sealed class DotvvmConfig : IDotvvmServiceConfigurator
        {
            private readonly IWindsorContainer _container;

            public DotvvmConfig(IWindsorContainer container)
            {
                _container = container;
            }

            public void ConfigureServices(IDotvvmServiceCollection opt)
            {
                opt.Services.AddSingleton<IViewModelLoader>(_ => new WindsorViewModelLoader(_container));
                
                opt.AddBusinessPack();
                opt.AddDefaultTempStorages("Temp");
            }
        }

        private sealed class NoAuthFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context) => true;
        }
    }
}