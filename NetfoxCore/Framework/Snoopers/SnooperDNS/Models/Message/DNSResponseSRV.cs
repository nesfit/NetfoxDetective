﻿// Copyright (c) 2017 Jan Pluskal, Pavol Vican
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

namespace Netfox.Snoopers.SnooperDNS.Models.Message
{
    public class DnsResponseSRV: DnsResponse
    {
        public string Service { get; set; }
        public string Protocol { get; set; }
        public ushort Priority { get; set; }
        public ushort Weight { get; set; }
        public ushort Port { get; set; }
        public string Target { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(this.Service)}: {this.Service}, {nameof(this.Protocol)}: {this.Protocol}, {nameof(this.Priority)}: {this.Priority}, {nameof(this.Weight)}: {this.Weight}, {nameof(this.Port)}: {this.Port}, {nameof(this.Target)}: {this.Target}";
        }
    }
}