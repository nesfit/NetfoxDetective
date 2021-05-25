using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Framework.Models.Snoopers;


namespace Netfox.Framework.ApplicationProtocolExport.Snoopers
{
    public abstract class SnooperWindsorInstaller : IWindsorInstaller
    {
        #region Implementation of IWindsorInstaller

        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyInDirectory(new AssemblyFilter(".")).BasedOn<ISnooper>()
                .WithServiceSelf());
        }

        #endregion
    }
}