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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using NUnit.Framework;

namespace Netfox.Detective.Tests
{
    [TestFixture,Apartment(ApartmentState.STA)]
       public abstract class WindsorInstallerTestsBase {
        protected WindsorContainer WindsorContainer { get; set; }

        [TearDown]
        public virtual void Cleanup()
        {
            this.WindsorContainer.Dispose();
            this.WindsorContainer = null;
        }

        [SetUp]
        public virtual void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            this.WindsorContainer  = new WindsorContainer("WindsorInvestigationInstallerTestsBase", new DefaultKernel(), new DefaultComponentInstaller());
            this.WindsorContainer.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(this.WindsorContainer));
        }

        protected bool ContainsComponent(Type type, LifestyleType lifestyleType)
        {
            var component = this.GetHandlersFor(type);
            return Enumerable.Any<IHandler>(component, c => c.ComponentModel.LifestyleType == lifestyleType);
        }

        protected bool ContainsComponent(Type interfaceType, Type componentType, LifestyleType lifestyleType)
        {
            var component = this.GetHandlersFor(interfaceType) as IEnumerable<IHandler>;
            component = component.Where(c => c.ComponentModel.Implementation.IsAssignableFrom(componentType));

            return component.Any(c => c.ComponentModel.LifestyleType == lifestyleType);
        }

        protected IHandler[] GetHandlersFor(Type type) { return this.WindsorContainer.Kernel.GetAssignableHandlers(type); }
        protected IHandler[] GetAllHandlers() { return this.GetHandlersFor(typeof(object)); }

        protected virtual void AssertAllHandlersAtLeastCount(int count)
        {
            Console.WriteLine($@"Windsor registered component: {this.GetAllHandlers().Length}");
            foreach(var handler in this.GetAllHandlers())
            {
                Console.WriteLine($"\t{handler.ComponentModel.ComponentName} : {handler.ComponentModel.LifestyleType}");
            }
            Assert.GreaterOrEqual(this.GetAllHandlers().Length, count, "It is expted to have more registered types.");
        }
    }
}