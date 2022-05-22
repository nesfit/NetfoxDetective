using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Web.DAL;
using Netfox.Web.BL;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Queries;
using Netfox.Web.BL.Queries.FirstLevel;
using Netfox.Web.DAL.Entities;
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

            using var db = container.Resolve<Func<NetfoxWebDbContext>>()();
            Seed(db);
        }

        private static void Seed(NetfoxWebDbContext ctx)
        {
            if (ctx.Users.Any()) 
                return;
            
            Guid? roleId = ctx.Roles.SingleOrDefault(i => i.Name == "Administrator")?.Id;
            if (roleId == null)
            {
                Role r = new Role {Name = "Administrator"};
                ctx.Roles.Add(r);
                ctx.SaveChanges();

                roleId = r.Id;
            }

            // user: admin
            // password: admin
            ctx.Users.Add(new User
            {
                Username = "admin",
                Password = LoginFacade.SHA256Hash("admin"),
                Firstname = "Administrator",
                Surname = "A", 
                IsEnable = true, 
                RoleId = roleId.Value
            });
            ctx.SaveChanges();
        }
    }
}