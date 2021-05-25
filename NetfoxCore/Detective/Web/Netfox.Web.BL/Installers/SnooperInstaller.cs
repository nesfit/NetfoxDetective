using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Framework.Models.Snoopers;
using Netfox.Web.BL.Infrastructure;

namespace Netfox.Web.BL.Installers
{
    public class SnooperInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var path = Path.GetDirectoryName(typeof(SnooperInstaller).Assembly.Location);
            foreach (string f in Directory.EnumerateFiles(path, "*.dll"))
            {
                if (!Path.GetFileName(f).Contains("Netfox.Snoopers", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                
                Console.WriteLine($">>> Scanning `{f}`...");
                foreach (Type t in Assembly.LoadFile(f).GetTypes().Where(i => i.IsClass && !i.IsAbstract))
                {
                    if (t.IsAssignableTo(typeof(ISnooper)))
                    {
                        Console.WriteLine($">>>   Registering `{t.FullName}` as ISnooper...");
                        container.Register(Component.For<ISnooper>().Forward(t).ImplementedBy(t).LifestyleTransient());
                    }
                    else if (t.IsAssignableTo(typeof(ISnooperWeb)))
                    {
                        Console.WriteLine($">>>   Registering `{t.FullName}` as ISnooperWeb...");
                        container.Register(
                            Component.For<ISnooperWeb>().Forward(t).ImplementedBy(t).LifestyleTransient());
                    }
                }
            }
        }
    }
}