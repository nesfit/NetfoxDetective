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

using System.Collections.Concurrent;
using Castle.MicroKernel.Lifestyle;
using Netfox.Core.Interfaces.ViewModels;

namespace Netfox.Core.Windsor
{
    public class PerEntityLifestyleManager : AbstractLifestyleManager
    {
        private readonly ConcurrentDictionary<object, IDataEntityVm> _instances = new ConcurrentDictionary<object, IDataEntityVm>();

        public override object Resolve(Castle.MicroKernel.Context.CreationContext context, Castle.MicroKernel.IReleasePolicy releasePolicy)
        {
            var param = context.AdditionalArguments["model"];
            if(param != null && this._instances.TryGetValue(param, out var retrievedInstance)) { return retrievedInstance; }

            var instanceVm = base.Resolve(context, releasePolicy);
            if(instanceVm is IDataEntityVm) // only IDataEntityVm VMs are bound per entity... other constructors are not
            {
                var instance = instanceVm as IDataEntityVm;
                this._instances.TryAdd(param, instance); //no need for concurrency check
            }
            return instanceVm;
        }

        /// <summary>
        /// Invoked when the container gets disposed. The container will not call it multiple times in multithreaded environments.
        ///               However it may be called at the same time when some out of band release mechanism is in progress. Resolving those potential
        ///               issues is the task of implementors
        /// </summary>
        public override void Dispose() { }

        public override bool Release(object instance)
        {
            var dataentityVm = instance as IDataEntityVm;
            var weakModel = dataentityVm.EncapsulatedModel;
            if (this._instances.Keys.Contains(weakModel))
            {
                this._instances.TryRemove(weakModel,out dataentityVm);
                return true;
            }
            return false;
        }
    }
}
