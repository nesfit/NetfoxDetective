using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Interfaces.Models.Base;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModelsDataEntity.Investigations;

namespace Netfox.Detective.Infrastructure
{
    public class DetectiveIvestigationWindsorInstaller : DetectiveIvestigationWindsorInstallerBase
    {
        #region Implementation of IWindsorInstaller

        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container, store);
            this.RegisterViewModelModelResolver(container);
        }

        #endregion

        private void RegisterViewModelModelResolver(IWindsorContainer container)
        {
            var parentContainer = container.Parent;
            ICrossContainerHierarchyResolver parentCrossContainerHierarchyResolver = null;
            if (parentContainer?.Kernel.HasComponent(typeof(CrossContainerHierarchyResolver)) ?? false)
            {
                parentCrossContainerHierarchyResolver = parentContainer.Resolve<ICrossContainerHierarchyResolver>();
            }

            container.Register(Component.For<ICrossContainerHierarchyResolver, CrossContainerHierarchyResolver>());
            if (parentCrossContainerHierarchyResolver != null)
            {
                var childSubResolver = container.Resolve<ICrossContainerHierarchyResolver>();
                parentCrossContainerHierarchyResolver.SubResolver = childSubResolver;
            }
        }

        protected internal override void RegisterOtherTypes(IWindsorContainer container)
        {
            base.RegisterOtherTypes(container);
            container.Register(Component.For<IInvestigation, Investigation>().OnlyNewServices());
            container.Register(Component.For<InvestigationVm>().OnlyNewServices());
        }

        public DetectiveIvestigationWindsorInstaller() : base(Classes
            .FromAssemblyContaining<DetectiveIvestigationWindsorInstaller>())
        {
        }
    }
}