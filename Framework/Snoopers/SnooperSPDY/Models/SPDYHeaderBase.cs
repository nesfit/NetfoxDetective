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

using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.SnooperSPDY.Models.Frames;

namespace Netfox.SnooperSPDY.Models
{
    [ComplexType]
    public class SPDYHeaderBase
    {
        //[NotMapped]
        //public Dictionary<string, string> Fields { get; private set; }
        public PersistableJsonSerializableDictionaryStringString Fields { get; set; } = new PersistableJsonSerializableDictionaryStringString();
        public string Version { get; private set; }
        public bool IsPresent => this.Version != null;

        public SPDYHeaderBase() { } //EF
        protected SPDYHeaderBase(SPDYStreamFrame streamFrame)
        {
            this.Version = streamFrame.Fields[":version"];
            this.Fields = new PersistableJsonSerializableDictionaryStringString(streamFrame.Fields.Where(f => !f.Key.StartsWith(":")).ToDictionary(f => f.Key, f => f.Value));
        }
    }
}