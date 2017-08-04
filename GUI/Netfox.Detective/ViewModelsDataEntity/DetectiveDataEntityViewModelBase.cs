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
using System.Threading.Tasks;
using Castle.Core;
using Castle.Windsor;
using Netfox.Core.Interfaces.ViewModels;
using Netfox.Detective.ViewModels;

namespace Netfox.Detective.ViewModelsDataEntity
{
    public abstract class DetectiveDataEntityViewModelBase : DetectiveViewModelBase, IDataEntityVm
    {
        protected DetectiveDataEntityViewModelBase(IWindsorContainer applicationOrInvestigationWindsorContainer) : base(applicationOrInvestigationWindsorContainer) { }

        protected DetectiveDataEntityViewModelBase(IWindsorContainer applicationOrInvestigationWindsorContainer, object model) : this(applicationOrInvestigationWindsorContainer)
        {
            this.EncapsulatedModel = model;
            if(model is IWindsorContainerChanger) { this.ApplicationOrInvestigationWindsorContainer = (model as IWindsorContainerChanger).InvestigationWindsorContainer; }
        }

        public Type EncapsulatedDataType => this.EncapsulatedModel?.GetType();

        [DoNotWire]
        public object EncapsulatedModel { get; protected set; } //obsolete only for private use
    
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task Init() { }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}