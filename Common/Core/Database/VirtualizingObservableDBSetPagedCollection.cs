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
using System.Linq.Expressions;
using AlphaChiTech.Virtualization;
using AlphaChiTech.VirtualizingCollection.Interfaces;
using Castle.Windsor;

namespace Netfox.Core.Database
{
    public class VirtualizingObservableDBSetPagedCollection<T> : VirtualizingObservableCollection<T>, INotifyImmediately where T : class, IEntity
    {

        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer, IObservableNetfoxDBContext dbContext)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer, dbContext)))
        { }
        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer, IObservableNetfoxDBContext dbContext, IEnumerable<string> eagerLoadProperties)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer, dbContext, null, eagerLoadProperties)))
        { }
        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer, IObservableNetfoxDBContext dbContext,  Expression<Func<T, Boolean>> filter)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer, dbContext, filter, null))) { }
        public VirtualizingObservableDBSetPagedCollection(IWindsorContainer investigationOrAppWindsorContainer, IObservableNetfoxDBContext dbContext,  Expression<Func<T, Boolean>> filter, IEnumerable<string> eagerLoadProperties)
            : base(new PageDbSetSourceProvider<T>(new ObservableDbSetRepository<T>(investigationOrAppWindsorContainer, dbContext,  filter, eagerLoadProperties)))
        { }

        public bool IsNotifyImmidiately
        {
            get =>  this.Provider is INotifyImmediately iNotifyImmediatelyProvider && iNotifyImmediatelyProvider.IsNotifyImmidiately;
            set
            {
                if (this.Provider is INotifyImmediately iNotifyImmediatelyProvider) { iNotifyImmediatelyProvider.IsNotifyImmidiately = value; }
            }
        }
    }
}
