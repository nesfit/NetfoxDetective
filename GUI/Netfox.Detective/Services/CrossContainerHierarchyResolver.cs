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
using System.Linq;
using Castle.Core;
using Castle.Windsor;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.Interfaces;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.Services {
    public class CrossContainerHierarchyResolver : ICrossContainerHierarchyResolver
    {
        public IWindsorContainer Container { get; }

        public TVM Resolve<TVM>(object model) where TVM:class{ return this.Resolve(typeof(TVM), model) as TVM; }

        [DoNotWire]
        public ICrossContainerHierarchyResolver SubResolver { private get; set; }

        public CrossContainerHierarchyResolver(IWindsorContainer container) { this.Container = container; }
        
        public DetectiveViewModelBase Resolve(Type typeOfVm, object model)
        {
            if(this.Container.Kernel.GetHandler(typeOfVm) != null)
            {
                return this.Container.Resolve(typeOfVm, new
                {
                    model = model
                }) as DetectiveViewModelBase;
            }

            return this.SubResolver?.Resolve(typeOfVm,model);
        }

        public Type[] AvailableAnalyzerTypes => this.Container.Kernel.GetHandlers(typeof(IAnalyzer))
            .Concat(this.Container.Kernel.GetHandlers(typeof(IAnalyzerInvestigation)))
            .Concat(this.Container.Kernel.GetHandlers(typeof(IAnalyzerApplication)))
            .SelectMany(h => h.ComponentModel.Services.Where(t => !t.IsInterface))
            .Concat(this.SubResolver?.AvailableAnalyzerTypes ?? new Type[0])
            .Distinct()
            .ToArray();

    }
}