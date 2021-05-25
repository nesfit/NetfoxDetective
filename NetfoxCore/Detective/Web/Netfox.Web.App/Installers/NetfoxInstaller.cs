using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DotVVM.Framework.ViewModel;
using Netfox.Core.Database;
using Netfox.Core.Infrastructure;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Infrastructure;
using Netfox.Web.App.Settings;
using Netfox.Web.BL;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Infrastructure;
using Netfox.Web.BL.Installers;
using Netfox.Web.BL.Providers;
using Netfox.Web.BL.Queries;
using Netfox.Web.BL.Settings;
using Netfox.Web.DAL;
using UnitOfWork;
using UnitOfWork.BaseDataEntity;
using UnitOfWork.EF6Repository;
using NetfoxDbContext = Netfox.Persistence.NetfoxDbContext;

namespace Netfox.Web.App.Installers
{
    public class NetfoxInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var path = Path.GetDirectoryName(typeof(NetfoxInstaller).Assembly.Location);
            var filter = new AssemblyFilter(path);
            var settings = new JsonWebSettings();
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(settings.ConnectionString)
            {
                InitialCatalog = settings.InvestigationPrefix
            };
            container.Register(
                Component.For<INetfoxWebSettings>()
                    .Instance(settings)
                    .LifestyleSingleton(),
                
                Component.For<INetfoxSettings>()
                    .Instance(new NetfoxJsonSettings())
                    .LifestyleSingleton(),
            
                Component.For<Func<IWindsorContainer, SqlConnectionStringBuilder, NetfoxDbContext>>()
                    .Instance((windsorContainer, sqlBuilder) => new NetfoxDbContext(windsorContainer, sqlBuilder))
                    .LifestyleSingleton(),

                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<INetfoxProvider>()
                    .LifestyleSingleton(),

                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<HangfireFacade>()
                    .LifestyleTransient(),

                Component.For<SqlConnectionStringBuilder>()
                    .Instance(sqlConnectionStringBuilder).IsDefault(),

                Classes.FromAssemblyInDirectory(filter)
                    .BasedOn(typeof(NetfoxQueryBase<>))
                    .LifestyleTransient(),

                Component.For<IObservableNetfoxDBContext>()
                    .ImplementedBy<NetfoxDbContext>()
                    .Named(nameof(IObservableNetfoxDBContext))
                    .IsDefault()
                    .LifestyleSingleton()
   
                // , Classes.FromAssemblyInDirectory(filter)
                //     .BasedOn<ISnooperWeb>()
                //     .WithServiceFromInterface(typeof(ISnooperWeb))
                //     .WithServiceSelf().LifestyleTransient(),
                //
                // Classes.FromAssemblyInDirectory(filter)
                //     .BasedOn<ISnooper>()
                //     .WithServiceFromInterface(typeof(ISnooper))
                //     .WithServiceSelf()
                //     .LifestyleTransient()
            );
            
            new NetfoxFrameworkApiWindsorInstaller().Install(container, null);

            new SnooperInstaller().Install(container, null);
            
            var ctx = container.Resolve<IObservableNetfoxDBContext>();
            ctx.RegisterVirtualizingObservableDBSetPagedCollections();
        }
    }
}