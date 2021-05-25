﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Netfox.Core.Infrastructure;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Infrastructure;
using Netfox.Web.App.Settings;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Infrastructure;
using Netfox.Web.BL.Installers;
using Netfox.Web.BL.Queries;
using Netfox.Web.BL.Queries.FirstLevel;
using Netfox.Web.BL.Settings;
using Netfox.Web.DAL;
using Riganti.Utils.Infrastructure.AutoMapper;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.BL.Facades
{
    public class HangfireFacade
    {
        public IWindsorContainer Container { get; set; }

        protected Func<IWindsorContainer, Guid, string, NetfoxAPIFacade> NetfoxAPIFacadeFactory;

        public HangfireFacade()
        {
            this.Container = new WindsorContainer("WebNetfoxHangfireWC", new DefaultKernel(), new DefaultComponentInstaller());
            this.Container.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(this.Container));
            this.Container.Register(Component.For<INetfoxSettings>().Instance(new NetfoxJsonSettings()).LifestyleSingleton());
            this.Container.Register(Component.For<INetfoxWebSettings>().Instance(new JsonWebSettings()).LifestyleSingleton());
        }

        public void ProcessCapture(Guid investigationId, string appPath, string filePath)
        {
            this.InitializeWindsor();
            var nfxAPIFacade = NetfoxAPIFacadeFactory(this.Container, investigationId, appPath);
            nfxAPIFacade.ProcessCapture(filePath);
        }

        public void ExportData(Guid investigationId, List<string> availableSnoopers, string appPath)
        {
            this.InitializeWindsor();
            var nfxAPIFacade = NetfoxAPIFacadeFactory(this.Container, investigationId, appPath);
            nfxAPIFacade.ExportData(availableSnoopers);
        }

        public void ExportData(Guid investigationId, List<string> availableSnoopers, string appPath, Guid captureId)
        {
            this.InitializeWindsor();
            var nfxAPIFacade = NetfoxAPIFacadeFactory(this.Container, investigationId, appPath);
            nfxAPIFacade.ExportData(availableSnoopers, captureId);
        }

        private void InitializeWindsor()
        {
            this.Container.Register(
                Component.For<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>()
                    .Instance((container, id, path) => new NetfoxAPIFacade(container.Resolve<INetfoxWebSettings>(), container, id, path))
                    .LifestyleSingleton(),

                /* DAL */
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
                    .LifestyleSingleton(),
                 /* // DAL */

                 /* BL */
                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<FacadeBase>()
                    .LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<IFilterDTO>()
                    .LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<NetfoxFacadeBase>()
                    .LifestyleTransient()

            );

            this.Container.Install(new SnooperInstaller());

            this.NetfoxAPIFacadeFactory = this.Container.Resolve<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>();
            var netfoxFrameworkApiWindsorInstaller = new NetfoxFrameworkApiWindsorInstaller();
            netfoxFrameworkApiWindsorInstaller.Install(this.Container, null);
        }
    }
}
