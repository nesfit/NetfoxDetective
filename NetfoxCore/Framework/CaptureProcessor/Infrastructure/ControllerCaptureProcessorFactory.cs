using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Netfox.Framework.CaptureProcessor.CoreController;
using Netfox.Framework.CaptureProcessor.Interfaces;

namespace Netfox.Framework.CaptureProcessor.Infrastructure
{
    internal class ControllerCaptureProcessorFactory : IControllerCaptureProcessorFactory
    {
        public IWindsorContainer Container { get; }

        public ControllerCaptureProcessorFactory(IWindsorContainer container)
        {
            this.Container = container;
        }

        #region Implementation of IControllerCaptureProcessorFactory

        public ControllerCaptureProcessorLocal Create()
        {
            var chidWc = new WindsorContainer($"ControllerCaptureProcessorLocal-{Guid.NewGuid()}", new DefaultKernel(),
                new DefaultComponentInstaller());
            this.Container.AddChildContainer(chidWc);
            chidWc.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(chidWc));
            chidWc.Install(new CaptureProcessorWindsorInstaller());
            return chidWc.Resolve<IControllerCaptureProcessorFactoryInternal>().Create();
        }

        #endregion
    }
}