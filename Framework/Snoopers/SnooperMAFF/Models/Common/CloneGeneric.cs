// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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

using System.Collections.Generic;
using System.Linq;
using Netfox.SnooperMAFF.Models.Objects;

namespace Netfox.SnooperMAFF.Models.Common
{
    /// <summary>
    /// Class implements objects cloning depends on on type of derived object
    /// </summary>
    public static class CloneGeneric
    {
        /// <summary>
        /// Clones the current type of object.
        /// </summary>
        /// <param name="oItemToClone">The item to clone.</param>
        /// <returns>Return cloned object</returns>
        public static BaseObject CloneObject(BaseObject oItemToClone)
        {
            return oItemToClone.Clone();
        }

        /// <summary>
        /// Clones the list of object's.
        /// </summary>
        /// <param name="oListToClone">The list to clone.</param>
        /// <returns></returns>
        public static List<BaseObject> CloneList(List<BaseObject> oListToClone)
        {
            return oListToClone.Select(CloneGeneric.CloneObject).ToList();
        }
    }
}
