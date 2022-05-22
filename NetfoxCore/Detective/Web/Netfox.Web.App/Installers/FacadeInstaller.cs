using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Web.BL;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades.Base;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.App.Installers
{
    public class FacadeInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<FacadeBase>()
                    .LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>()
                    .BasedOn<IFilterDTO>()
                    .LifestyleTransient(),
                Classes.FromAssemblyInDirectory(new AssemblyFilter(Path.GetDirectoryName(typeof(FacadeInstaller).Assembly.Location)))
                    .BasedOn<NetfoxFacadeBase>()
                    .LifestyleTransient()
            );     
        }
    }
}