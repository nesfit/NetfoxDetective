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
using Castle.Windsor.Installer;

namespace Netfox.Core.Infrastructure {
    public class InstallerFactoryFilter : InstallerFactory
    {
        public Type ImplementingInterface { get; }

        public InstallerFactoryFilter(Type implementingInterface) { this.ImplementingInterface = implementingInterface; }
        #region Overrides of InstallerFactory
        public override IEnumerable<Type> Select(IEnumerable<Type> installerTypes)
        {
            foreach(var installerType in installerTypes)
            {
                if(installerType.Assembly.IsDynamic)
                    continue; //skipping dynamic assembly because otherwise it emmits  "System.NotSupportedException : The invoked member is not supported in a dynamic assembly." when running test on VS TS.
                //if(this.ImplementingInterface.IsInstanceOfType(installerType)) { yield return installerType; }
                //if(installerType.IsInstanceOfType(this.ImplementingInterface)) { yield return installerType; }
                if (this.ImplementingInterface.IsAssignableFrom(installerType))
                {
                    yield return installerType;
                }

            }
        }
        #endregion
    }
}