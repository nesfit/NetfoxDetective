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
using System.Linq;

namespace Netfox.NBARDatabase
{
    public partial class NBAR2TaxonomyProtocolPorts
    {
        public UInt32[] tcpArr
        {
            get
            {
                return !String.IsNullOrEmpty(this.tcp)
                    ? this.tcp.Split(',').Select(p =>
                    {
                        UInt32 port;
                        UInt32.TryParse(p, out port);
                        return port;
                    }).ToArray()
                    : new UInt32[]
                        {};
            }
        }

        public UInt32[] udpArr
        {
            get
            {
                return !String.IsNullOrEmpty(this.udp)
                    ? this.udp.Split(',').Select(p =>
                    {
                        UInt32 port;
                        UInt32.TryParse(p, out port);
                        return port;
                    }).ToArray()
                    : new UInt32[]
                        {};
            }
        }
    }
}