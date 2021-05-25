using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using Netfox.Web.App.Helpers;
using Netfox.Web.BL.Infrastructure;

namespace Netfox.Web.App.Installers
{
    public class PresentationInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(

                Classes.FromAssemblyContaining<AspStartup>()
                    .BasedOn<IDotvvmPresenter>()
                    .LifestyleTransient(),

                /*Classes.FromAssemblyContaining<Startup>()
                    .BasedOn<DotvvmViewModelBase>()
                    .LifestyleTransient(),*/
                Classes.FromAssemblyInDirectory(new AssemblyFilter(Path.GetDirectoryName(typeof(PresentationInstaller).Assembly.Location)))
                    .BasedOn<DotvvmViewModelBase>()
                    .LifestyleTransient(),

                Classes.FromAssemblyContaining<AspStartup>()
                    .BasedOn<IHelper>()
                    .LifestyleTransient()
            );
        }
    }
}