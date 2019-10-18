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

using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Netfox.Framework.CaptureProcessor.CoreController;
using Netfox.Framework.CaptureProcessor.Interfaces;

namespace Netfox.Framework.CaptureProcessor.Infrastructure {
    public class ControllerCaptureProcessorFactory : IControllerCaptureProcessorFactory {
        public IWindsorContainer Container { get; }

        public ControllerCaptureProcessorFactory(IWindsorContainer container) { this.Container = container; }
        #region Implementation of IControllerCaptureProcessorFactory
        public ControllerCaptureProcessorLocal Create()
        {
            var chidWc = new WindsorContainer($"ControllerCaptureProcessorLocal-{Guid.NewGuid()}", new DefaultKernel(), new DefaultComponentInstaller());
            this.Container.AddChildContainer(chidWc);
            chidWc.Register(Component.For<IWindsorContainer,WindsorContainer>().Instance(chidWc));
            chidWc.Install(new CaptureProcessorWindsorInstaller());
            return chidWc.Resolve<IControllerCaptureProcessorFactoryInternal>().Create();
        }
        #endregion
    }
}