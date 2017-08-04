// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModelsDataEntity.Investigations;

namespace Netfox.Detective.Infrastructure {
    public class DetectiveIvestigationWindsorInstaller : DetectiveIvestigationWindsorInstallerBase
    {
        #region Implementation of IWindsorInstaller
        public override void Install(IWindsorContainer container, IConfigurationStore store)
        {
            base.Install(container,store);
            this.RegisterViewModelModelResolver(container);
        }
        #endregion
        private void RegisterViewModelModelResolver(IWindsorContainer container)
        {
            var parrentContainer = container.Parent;
            ICrossContainerHierarchyResolver parentCrossContainerHierarchyResolver = null;
            if (parrentContainer?.Kernel.HasComponent(typeof(CrossContainerHierarchyResolver)) ?? false)
            {
                parentCrossContainerHierarchyResolver = parrentContainer.Resolve<ICrossContainerHierarchyResolver>();
            }
            container.Register(Component.For<ICrossContainerHierarchyResolver, CrossContainerHierarchyResolver>());
            if(parentCrossContainerHierarchyResolver != null)
            {
                var childSubResolver = container.Resolve<ICrossContainerHierarchyResolver>();
                parentCrossContainerHierarchyResolver.SubResolver = childSubResolver;
            }
        }

        protected internal override void RegisterOtherTypes(IWindsorContainer container)
        {
            base.RegisterOtherTypes(container);
            container.Register(Component.For<Investigation>().OnlyNewServices());
            container.Register(Component.For<InvestigationVm>().OnlyNewServices());
        }

        public DetectiveIvestigationWindsorInstaller() : base(Classes.FromAssemblyContaining<DetectiveIvestigationWindsorInstaller>()) { }
    }
}