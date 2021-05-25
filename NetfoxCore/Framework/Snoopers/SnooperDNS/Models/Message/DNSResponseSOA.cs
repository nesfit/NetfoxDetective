// Copyright (c) 2017 Jan Pluskal, Pavol Vican
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

namespace Netfox.Snoopers.SnooperDNS.Models.Message
{
    public class DnsResponseSOA : DnsResponse
    {
        public String MName { get; set; }
        public String RName { get; set; }
        public UInt32 Serial { get; set; }

        public UInt32 Refresh { get; set; }
        public UInt32 Retry { get; set; }
        public UInt32 Expire { get; set; }
        public UInt32 Minimum { get; set; }

        public override String ToString()
        {
            return
                $"{base.ToString()}, {nameof(this.MName)}: {this.MName}, {nameof(this.RName)}: {this.RName}, {nameof(this.Serial)}: {this.Serial}, {nameof(this.Refresh)}: {this.Refresh}, {nameof(this.Retry)}: {this.Retry}, {nameof(this.Expire)}: {this.Expire}, {nameof(this.Minimum)}: {this.Minimum}";
        }
    }
}