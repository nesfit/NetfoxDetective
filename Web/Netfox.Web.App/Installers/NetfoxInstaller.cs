using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DotVVM.Framework.ViewModel;
using Netfox.Core.Database;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Infrastruture;
using Netfox.Web.BL;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Infrastructure;
using Netfox.Web.BL.Providers;
using Netfox.Web.BL.Queries;
using Netfox.Web.DAL;
using Netfox.Web.DAL.Properties;
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
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(NetfoxWebSettings.Default.ConnectionString)
            {
                InitialCatalog = NetfoxWebSettings.Default.InvestigationPrefix
            };
            container.Register(
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

                Classes.FromAssemblyInDirectory(new AssemblyFilter(new FileInfo(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.ToString()))
                    .BasedOn(typeof(NetfoxQueryBase<>))
                    .LifestyleTransient(),

                Component.For<IObservableNetfoxDBContext>()
                    .ImplementedBy<NetfoxDbContext>()
                    .Named(nameof(IObservableNetfoxDBContext))
                    .IsDefault()
                    .LifestyleSingleton(),
   
                Classes.FromAssemblyInDirectory(new AssemblyFilter(new FileInfo(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.ToString()))
                    .BasedOn<ISnooperWeb>()
                    .WithServiceFromInterface(typeof(ISnooperWeb))
                    .WithServiceSelf().LifestyleTransient(),

                Classes.FromAssemblyInDirectory(new AssemblyFilter(new FileInfo(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.ToString()))
                    .BasedOn<ISnooper>()
                    .WithServiceFromInterface(typeof(ISnooper))
                    .WithServiceSelf()
                    .LifestyleTransient()
            );

            var netfoxFrameworkApiWindsorInstaller = new NetfoxFrameworkApiWindsorInstaller();
            netfoxFrameworkApiWindsorInstaller.Install(container, null);

            var observableNetfoxDBContext = container.Resolve<IObservableNetfoxDBContext>();
            observableNetfoxDBContext.RegisterVirtualizingObservableDBSetPagedCollections();
        }
    }
}