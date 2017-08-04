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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Core.Windsor;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModels;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Detective.Views;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Detective.Infrastructure
{
    public abstract class DetectiveIvestigationWindsorInstallerBase : IWindsorInstaller, IDetectiveIvestigationWindsorInstaller
    {
        protected DetectiveIvestigationWindsorInstallerBase(FromAssemblyDescriptor fromAssemblyDescriptorDescriptor) { this.FromAssemblyDescriptor = fromAssemblyDescriptorDescriptor; }
        #region Implementation of IWindsorInstaller
        public virtual void Install(IWindsorContainer container, IConfigurationStore store)
        {
            this.AddFacilities(container);
            this.RegisterFactories(container);
            this.RegisterOtherTypes(container);
            this.AnalyzersRegistration(container);

            this.ServicesRegister(container);
            this.DetectiveInvestigationPaneViewModelBaseRegister(container);
            this.DetectiveDataEntityPaneViewModelsRegister(container);
            this.DetectiveIvestigationDataEntityPaneViewModelBaseRegister(container);
            this.AutoRegisterDetectivePaneViewsRegister(container);
            this.SnooperRegister(container);
        }
        #endregion


        protected internal virtual void RegisterFactories(IWindsorContainer container)
        {
            container.Register(Component.For<IInvestigationFactoryInternal>().AsFactory().OnlyNewServices());
            container.Register(Component.For<ISnooperFactory, SnooperFactory>().LifestyleTransient().OnlyNewServices());
        }

        protected internal virtual void AddFacilities(IWindsorContainer container)
        {
            if (!container.Kernel.GetFacilities().Any(f => f is StartableFacility)) { container.AddFacility<StartableFacility>(); }
            if (!container.Kernel.GetFacilities().Any(f => f is TypedFactoryFacility)) { container.AddFacility<TypedFactoryFacility>(); }
        }

        protected internal virtual void RegisterOtherTypes(IWindsorContainer container){}

        protected internal virtual void AnalyzersRegistration(IWindsorContainer container)
        {
            container.Register(this.FromAssemblyDescriptor.BasedOn<IAnalyzerInvestigation>().WithServiceFromInterface(typeof(IAnalyzerInvestigation)).WithServiceSelf().LifestyleSingleton());
        }

        protected internal virtual void SnooperRegister(IWindsorContainer container)
        {
            container.Register(this.FromAssemblyDescriptor.BasedOn<ISnooper>().WithServiceFromInterface(typeof(ISnooper)).WithServiceSelf().LifestyleTransient());
        }

        protected internal virtual void ServicesRegister(IWindsorContainer container)
        {
            container.Register(this.FromAssemblyDescriptor.BasedOn<IStartableInvestigationService>().WithServiceDefaultInterfaces().WithServiceSelf().LifestyleSingleton().Configure(component => component.Start()));
            container.Register(this.FromAssemblyDescriptor.BasedOn<IInvestigationService>().WithServiceDefaultInterfaces().WithServiceSelf().LifestyleSingleton());
        }

        protected internal FromAssemblyDescriptor FromAssemblyDescriptor { get; set; }
        protected internal virtual void DetectiveInvestigationPaneViewModelBaseRegister(IWindsorContainer container)
        {
            container.Register(this.FromAssemblyDescriptor.BasedOn<DetectiveInvestigationPaneViewModelBase>().If(IsContainingMultiparameterConstructor).WithServiceDefaultInterfaces().LifestyleCustom<PerEntityLifestyleManager>());
            container.Register(this.FromAssemblyDescriptor.BasedOn<DetectiveInvestigationPaneViewModelBase>().WithServiceDefaultInterfaces().LifestyleSingleton());
        }


        protected internal virtual void DetectiveDataEntityPaneViewModelsRegister(IWindsorContainer container)
        {
            container.Register(this.FromAssemblyDescriptor.BasedOn<DetectiveDataEntityViewModelBase>().If(IsContainingMultiparameterConstructor).WithServiceDefaultInterfaces().LifestyleCustom<PerEntityLifestyleManager>());
            container.Register(this.FromAssemblyDescriptor.BasedOn<DetectiveDataEntityViewModelBase>().WithServiceDefaultInterfaces().LifestyleTransient());
        }

        private static bool IsContainingMultiparameterConstructor(Type type) { return type.GetConstructors().Any(c => c.GetParameters().Count() > 1); }

        protected internal virtual void DetectiveIvestigationDataEntityPaneViewModelBaseRegister(IWindsorContainer container)
        {
            container.Register(this.FromAssemblyDescriptor.BasedOn<DetectiveIvestigationDataEntityPaneViewModelBase>().If(IsContainingMultiparameterConstructor).WithServiceDefaultInterfaces().LifestyleCustom<PerEntityLifestyleManager>());
            container.Register(this.FromAssemblyDescriptor.BasedOn<DetectiveIvestigationDataEntityPaneViewModelBase>().WithServiceDefaultInterfaces().LifestyleTransient());
        }

        protected internal virtual void AutoRegisterDetectivePaneViewsRegister(IWindsorContainer container)
        {
            container.Register(
                this.FromAssemblyDescriptor
                    .BasedOn<IAutoRegisterView>()
                    .If(type => typeof(DetectiveApplicationPaneViewBase).IsAssignableFrom(type))
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton());
            container.Register(
                this.FromAssemblyDescriptor
                    .BasedOn<IAutoRegisterView>()
                    .If(type => typeof(DetectiveDataEntityPaneViewBase).IsAssignableFrom(type))
                    .WithServiceDefaultInterfaces()
                    .LifestyleTransient());
            container.Register(
                this.FromAssemblyDescriptor
                    .BasedOn<IAutoRegisterView>()
                    .If(type => typeof(DetectiveWindowBase).IsAssignableFrom(type))
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton());
        }

        protected static FromAssemblyDescriptor GetFromAssemblyDescriptorForAssemblisDeclaringTypes(IEnumerable<Type> types)
        {
            return Classes.FromAssemblyInDirectory(new AssemblyFilter(".", "*Snooper*").FilterByAssembly(assembly => assembly.ExportedTypes.Intersect(types).Any()));
        }

    }
}