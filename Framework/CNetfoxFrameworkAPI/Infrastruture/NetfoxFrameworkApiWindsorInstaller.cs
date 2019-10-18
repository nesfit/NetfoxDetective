// Copyright (c) 2017 Jan Pluskal
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

using System.Linq;
using Castle.Core.Logging;
using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Netfox.Core;
using Netfox.Core.Infrastructure;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.CaptureProcessor.Infrastructure;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.Models.Interfaces;
using Netfox.Nbar;
using Netfox.NBARDatabase;
using Netfox.NetfoxFrameworkAPI.Interfaces;

namespace Netfox.NetfoxFrameworkAPI.Infrastruture
{
    public class NetfoxFrameworkApiWindsorInstaller: IDetectiveIvestigationWindsorInstaller
    {
        #region Implementation of IWindsorInstaller
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            this.OtherTypesRegister(container);
            this.FactoriesRegister(container);
        }
        #endregion
        internal void OtherTypesRegister(IWindsorContainer container)
        {
            //container.Register(Component.For<InvestigationInfo>().OnlyNewServices().LifestyleSingleton());
            container.Register(Component.For<ILogger, NullLogger>().OnlyNewServices().LifestyleSingleton().Start());
            container.Register(Component.For<IFrameworkController, FrameworkController>().OnlyNewServices());
            container.Register(Component.For<NBARProtocolPortDatabase>().LifestyleSingleton().OnlyNewServices().Start());
            //container.Register(Component.For<ApplicationRecognizerNBAR>().OnlyNewServices().LifestyleSingleton());
            //container.Register(Component.For<ApplicationRecognizerRTP>().OnlyNewServices().LifestyleSingleton());
            container.Register(Component.For<IApplicationRecognizer>().ImplementedBy<ApplicationRecognizerNBAR>().LifestyleSingleton().OnlyNewServices());
            container.Register(Component.For<SnooperLoader>().LifestyleTransient().OnlyNewServices());
        }

        internal void AddFacilities(IWindsorContainer container)
        {
            if (!container.Kernel.GetFacilities().Any(f => f is StartableFacility)) { container.AddFacility<StartableFacility>(); }
            if (!container.Kernel.GetFacilities().Any(f => f is TypedFactoryFacility)) { container.AddFacility<TypedFactoryFacility>(); }
        }

        internal void FactoriesRegister(IWindsorContainer container)
        {
            container.Register(Component.For<IControllerCaptureProcessorFactory, ControllerCaptureProcessorFactory>().OnlyNewServices());
            container.Register(Component.For<ISnooperFactory, SnooperFactory>().LifestyleTransient().OnlyNewServices());
            container.Register(Component.For<IFrameworkControllerFactory>().AsFactory());
            //container.Register(Component.For<IControllerCaptureProcessorFactoryInternal>().AsFactory());
        }

        public void SetUpInMemory(IWindsorContainer container)
        {
            /*
            var investigationInfo = container.Resolve<InvestigationInfo>();
            investigationInfo.SqlConnectionStringBuilder = new SqlConnectionStringBuilder(NetfoxSettings.Default.DefaultInMemoryConnectionString);
            container.Register(Component.For<SqlConnectionStringBuilder>().Instance(investigationInfo.SqlConnectionStringBuilder));
            container.Register(Component.For<IObservableNetfoxDBContext, NetfoxDbContext, NetfoxDbContextInMemory>().LifestyleSingleton());
            var observableNetfoxDBContext = container.Resolve<IObservableNetfoxDBContext>();
            observableNetfoxDBContext.RegisterVirtualizingObservableDBSetPagedCollections();
            */
        }
    }
}
