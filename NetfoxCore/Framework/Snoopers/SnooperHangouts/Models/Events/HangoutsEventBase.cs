// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperHangouts.Models.Events
{
    public abstract class HangoutsEventBase : SnooperExportedObjectBase
    {
        private PersistableJsonSerializableGuid _framesGuids;
        public string DeviceOsVersion { get; private set; }
        public string DeviceHardware { get; private set; }
        public string ClientId { get; private set; }
        protected HangoutsEventBase():base() { } //EF
        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FrameGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }

        protected HangoutsEventBase(SnooperExportBase exportBase) : base(exportBase) {}

        protected void ParseRequestHeader(RequestHeader requestHeader)
        {
            this.DeviceOsVersion = requestHeader.ClientVersion.DeviceOsVersion;
            this.DeviceHardware = requestHeader.ClientVersion.DeviceHardware;
            this.ClientId = requestHeader.ClientIdentifier.Resource;
        }
    }
}