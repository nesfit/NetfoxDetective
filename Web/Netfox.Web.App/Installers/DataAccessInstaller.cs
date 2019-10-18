using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Web.DAL;
using Netfox.Web.BL;
using Netfox.Web.BL.Queries;
using Netfox.Web.BL.Queries.FirstLevel;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.Services.Facades;
using Riganti.Utils.Infrastructure.AutoMapper;
using Riganti.Utils.Infrastructure.EntityFramework;

namespace Netfox.Web.App.Installers
{
    public class DataAccessInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

                Component.For<Func<NetfoxWebDbContext>>()
                    .Instance(() => new NetfoxWebDbContext())
                    .LifestyleSingleton(),
              
                Component.For<IUnitOfWorkProvider>() 
                    .ImplementedBy<EntityFrameworkUnitOfWorkProvider<NetfoxWebDbContext>>()
                    .LifestyleSingleton(),

                Component.For<IUnitOfWorkRegistry>()
                    .UsingFactoryMethod(p => new AsyncLocalUnitOfWorkRegistry())
                    .LifestyleSingleton(),

                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn(typeof(AppQueryBase<>))
                    .LifestyleTransient(),

                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn(typeof(AppFirstLevelQueryBase<>))
                    .LifestyleTransient(),

                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn(typeof(IRepository<,>))
                    .WithServiceAllInterfaces()
                    .WithServiceSelf()
                    .LifestyleTransient(),

                Component.For(typeof(IRepository<,>))
                    .ImplementedBy(typeof(EntityFrameworkRepository<,>))
                    .IsFallback()
                    .LifestyleTransient(),

                Component.For<IDateTimeProvider>()
                    .Instance(new UtcDateTimeProvider()),

                Component.For(typeof(IEntityDTOMapper<,>))
                    .ImplementedBy(typeof(EntityDTOMapper<,>))
                    .LifestyleSingleton()
            );
        }
    }
}
     