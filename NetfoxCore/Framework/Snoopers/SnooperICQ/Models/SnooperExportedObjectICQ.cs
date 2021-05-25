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

using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperICQ.Models
{
    public class SnooperExportedObjectICQ : SnooperExportedObjectBase, IChatMessage
    {
        private SnooperExportedObjectICQ() : base() { } //EF
        public SnooperExportedObjectICQ(SnooperExportBase exportBase) : base(exportBase) { }

        #region Implementation of IChatMessage
        public string Message { get; internal set; }
        public string Sender { get; internal set; }
        public string Receiver { get; internal set; }
        #endregion
    }
}