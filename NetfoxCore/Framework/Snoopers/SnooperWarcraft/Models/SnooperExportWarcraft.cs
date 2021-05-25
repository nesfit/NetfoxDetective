// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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

using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperWarcraft.Models
{
    public class SnooperExportWarcraft : SnooperExportBase
    {
        public SnooperExportWarcraft() : base() { } //EF
        #region Overrides of SnooperExportBase
        public override string ToString()
        {
            // not sure what this is supposed to do, so for now return empty string
            return string.Empty;
        }
        #endregion
    }
}