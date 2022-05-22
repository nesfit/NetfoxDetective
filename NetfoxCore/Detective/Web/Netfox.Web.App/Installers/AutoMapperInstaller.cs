using System.IO;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Web.BL.Mappings;

namespace Netfox.Web.App.Installers
{
    public class AutoMapperInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(Path.GetDirectoryName(typeof(AutoMapperInstaller).Assembly.Location)))
                    .BasedOn<IMapping>()
                    .WithServiceAllInterfaces()
                    .LifestyleSingleton()
            );
        }

        internal static void InitMapper(IWindsorContainer container)
        {
            // configure all mappings now
            Mapper.Initialize(mapper =>
            {
                foreach (var mapping in container.ResolveAll<IMapping>())
                {
                    mapping.Configure(mapper);
                }
            });
            Mapper.AssertConfigurationIsValid();
        } 
    }
}