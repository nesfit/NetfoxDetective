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

using System.Collections.Generic;
using System.Text;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Snoopers.SnooperDNS.Models.Message;

namespace Netfox.Snoopers.SnooperDNS.Models
{
    public class PersistableJsonSerializableDNSResponse : PersistableJsonSerializable<DnsResponse> {
        public PersistableJsonSerializableDNSResponse() {}
        public PersistableJsonSerializableDNSResponse(IEnumerable<DnsResponse> collection) : base(collection) {}

        public override string ToString()
        {
            var sb = new StringBuilder("");

            var e = this.GetEnumerator();
            while (e.MoveNext())
            {
                var obj = e.Current;
                var properties = obj.GetType().GetProperties();

                foreach (var p in properties)
                {
                    sb.Append(p.Name + ": " + (p.GetValue(obj)?.ToString() ?? ""));
                    sb.Append(", ");
                }
                sb.Append("\n");

            }
            return sb.ToString();
        }
    }
}