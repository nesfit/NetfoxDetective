using System;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Serialization;
using Castle.Core.Logging;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Core.Infrastructure;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Core.Interfaces.Views.Exports;
using Netfox.Core.Models;
using Netfox.Core.Windsor;
using Netfox.Detective.Commands.Investigations;
using Netfox.Detective.Commands.Workspaces;
using Netfox.Detective.Core.BaseTypes.Views;
using Netfox.Detective.Interfaces;
using Netfox.Detective.Messages;
using Netfox.Detective.Models.Base;
using Netfox.Detective.Models.WorkspacesAndSessions;
using Netfox.Detective.Services;
using Netfox.Detective.ViewModels;
using Netfox.Detective.ViewModelsDataEntity.BkTasks;
using Netfox.Detective.Views;
using Netfox.Logger;
using DirectoryWrapper = Netfox.Detective.Wrappers.DirectoryWrapper;
using IDirectoryInfoFactory = Netfox.Detective.Interfaces.IDirectoryInfoFactory;
using ISerializerFactory = System.Windows.Documents.Serialization.ISerializerFactory;

namespace Netfox.Detective.Infrastructure
{
    public class DetectiveApplicationWindsorInstaller : IDetectiveApplicationWindsorInstaller
    {
        #region Implementation of IWindsorInstaller

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            this.AddFacilities(container);

            this.RegisterFactories(container);

            this.RegisterOtherTypes(container);

            this.ServicesRegister(container);
            this.DetectiveApplicationPaneViewModelBaseRegister(container);
            this.DetectiveWindowViewModelBaseRegister(container);
            this.AutoRegisterDetectivePaneViewsRegister(container);
            this.AnalyzersRegistration(container);
        }

        protected internal void AnalyzersRegistration(IWindsorContainer container)
        {
            container.Register(this.FromThisAssembly.BasedOn<IAnalyzerApplication>().WithServiceDefaultInterfaces()
                .LifestyleSingleton());
        }

        protected internal void RegisterFactories(IWindsorContainer container)
        {
            container.Register(Component.For<IInvestigationFactory, InvestigationFactory>());
            container.Register(Component.For<ICrossContainerHierarchyResolver, CrossContainerHierarchyResolver>());
            container.Register(Component.For<IBgTaskFactory>().AsFactory());
            container.Register(Component.For<ISerializerFactory>().AsFactory());
        }

        protected internal void AddFacilities(IWindsorContainer container)
        {
            if (!container.Kernel.GetFacilities().Any(f => f is StartableFacility))
            {
                container.AddFacility<StartableFacility>();
            }

            if (!container.Kernel.GetFacilities().Any(f => f is TypedFactoryFacility))
            {
                container.AddFacility<TypedFactoryFacility>();
            }
        }

        protected internal void RegisterOtherTypes(IWindsorContainer container)
        {
            container.Register(Component.For<NetfoxJsonSettings, INetfoxSettings, ILoggerSettings, IAppSettings>()
                .LifestyleSingleton());
            container.Register(Component.For<IApplicationShell, ApplicationShell>()
                .LifestyleSingleton()); //TODO Obsolete
            //container.Register(Component.For<SnooperLoader>()); //TODO register in Framework 
            container.Register(Component.For<NetfoxFileAppender>().LifestyleSingleton());
            container.Register(Component.For<NetfoxOutputAppender>().LifestyleSingleton());
            container.Register(Component.For<ILogger, NetfoxLogger>().LifestyleSingleton().Start());
            container.Register(Component.For<IInvestigationInfo, InvestigationInfo>().LifestyleTransient());
            container.Register(Component.For<IInvestigationInfoLoader, InvestigationInfoLoader>().LifestyleSingleton());
            container.Register(Component.For<Workspace>().LifestyleTransient());
            container.Register(Component.For<BgTaskVm>().LifestyleTransient());
            container.Register(Component.For<IDirectoryWrapper, DirectoryWrapper>().LifestyleTransient());
            container.Register(Component.For<CreateWorkspaceCommand>().LifestyleTransient());
            container.Register(Component.For<CreateInvestigationCommand>().LifestyleTransient());

            container.Register(Component.For<IFileSystem, FileSystem>());
            container.Register(Component.For<XmlObjectSerializer, DataContractSerializer>().LifestyleTransient());
            container.Register(Component.For<IDirectoryInfoFactory, DirectoryInfoFactory>());
            container.Register(Component.For<IDetectiveMessenger, DetectiveMvvmLightMessenger>());
            container.Register(Component.For<IWorkspaceFactory, WorkspaceFactory>().LifestyleTransient());
            container.Register(Component.For<ISerializationPersistor<Workspace>, WorkspaceSerializationPersistor>());
            container.Register(Component
                .For<ISerializationPersistor<Investigation>, InvestigationSerializationPersistor>());
            container.Register(
                Component.For<IWorkspacePathSerializationPersistor, WorkspacePathSerializationPersistor>());
            container.Register(Component.For<WorkspacePathMapper>());
        }

        protected internal void ServicesRegister(IWindsorContainer container)
        {
            container.Register(
                this.FromThisAssembly.BasedOn<IStartableDetectiveService>()
                    .WithServiceDefaultInterfaces()
                    .WithServiceSelf()
                    .LifestyleSingleton()
                    .Configure(component => component.Start()));
            container.Register(this.FromThisAssembly.BasedOn<IDetectiveService>().WithServiceDefaultInterfaces()
                .WithServiceSelf().LifestyleSingleton());
        }

        // Classes.FromThisAssembly(); is optimalized out... needs to be Classes.FromAssemblyContaining
        protected internal FromAssemblyDescriptor FromThisAssembly { get; set; } =
            Classes.FromAssemblyContaining<DetectiveApplicationWindsorInstaller>();

        private bool IsContainingMultiparameterConstructor(Type type)
        {
            return type.GetConstructors().Any(c => c.GetParameters().Count() > 1);
        }

        protected internal void DetectiveApplicationPaneViewModelBaseRegister(IWindsorContainer container)
        {
            container.Register(
                this.FromThisAssembly.BasedOn<DetectiveApplicationPaneViewModelBase>()
                    .If(this.IsContainingMultiparameterConstructor)
                    .WithServiceDefaultInterfaces()
                    .LifestyleCustom<PerEntityLifestyleManager>());
            container.Register(this.FromThisAssembly.BasedOn<DetectiveApplicationPaneViewModelBase>()
                .WithServiceDefaultInterfaces().LifestyleSingleton());
        }

        protected internal void DetectiveWindowViewModelBaseRegister(IWindsorContainer container)
        {
            container.Register(
                this.FromThisAssembly.BasedOn<DetectiveWindowViewModelBase>()
                    .If(this.IsContainingMultiparameterConstructor)
                    .WithServiceDefaultInterfaces()
                    .LifestyleCustom<PerEntityLifestyleManager>());
            container.Register(this.FromThisAssembly.BasedOn<DetectiveWindowViewModelBase>()
                .WithServiceDefaultInterfaces().LifestyleSingleton());
        }

        protected internal void AutoRegisterDetectivePaneViewsRegister(IWindsorContainer container)
        {
            container.Register(
                this.FromThisAssembly.BasedOn<IAutoRegisterView>()
                    .If(type => typeof(DetectiveApplicationPaneViewBase).IsAssignableFrom(type))
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton());
            container.Register(
                this.FromThisAssembly.BasedOn<IAutoRegisterView>()
                    .If(type => typeof(DetectiveDataEntityPaneViewBase).IsAssignableFrom(type))
                    .WithServiceDefaultInterfaces()
                    .LifestyleTransient());
            container.Register(
                this.FromThisAssembly.BasedOn<IAutoRegisterView>()
                    .If(type => typeof(DetectiveWindowBase).IsAssignableFrom(type))
                    .WithServiceDefaultInterfaces()
                    .LifestyleSingleton());
        }

        #endregion
    }
}