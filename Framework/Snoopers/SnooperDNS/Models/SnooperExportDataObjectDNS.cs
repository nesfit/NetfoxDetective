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
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperDNS.Models.Message;

namespace Netfox.SnooperDNS.Models
{
   public class SnooperExportedDataObjectDNS : SnooperExportedObjectBase
    {
        public enum DNSQueryType
        {
            QUERY = 0,
            IQUERY = 1,
            STATUS = 2,
            NOTIFY = 4,
            UPDATE = 5,
            UNASSIGNED = 3
        }

        public enum DNSResponseCode
        {
            NOERROR = 0,
            FORMERR = 1,
            SERVFAIL = 2,
            NXDOMAIN = 3,
            NOTIMP = 4,
            REFUSED = 5,
            YXDOMAIN = 6,
            YXRRSET = 7,
            NXRRSET = 8,
            NOTAUTHZONE = 9,
            NOTAUTH = 10,
            UNASSIGNED = 11
        }

        public ushort MessageId { get; set; }
        public ushort Flags { get; set; }
        public bool IsAuthoritativeAnswer => (this.Flags & 32) == 32;
        public bool IsTrunCation => (this.Flags & 64) == 64;
        public bool IsRecursionDesired => (this.Flags & 128) == 128;
        public bool IsRecursionAvailable => (this.Flags & 256) == 256;
        public DNSQueryType TypeQuery => Enum.IsDefined(typeof(DNSQueryType), (this.Flags & 30)) ? (DNSQueryType) (this.Flags & 30) : DNSQueryType.UNASSIGNED;
        public DNSResponseCode ResponseCode
        {
            get
            {
                var code = this.Flags >> 12;
                return Enum.IsDefined(typeof(DNSResponseCode), code) ? (DNSResponseCode) code : DNSResponseCode.UNASSIGNED;
            }
        } 
        public PersistableJsonSerializable<DNSBase> Queries { get; set; }
        public PersistableJsonSerializable<DnsResponse> Answer { get; set; }
        public PersistableJsonSerializable<DnsResponse> Authority { get; set; }
        public PersistableJsonSerializable<DnsResponse> Additional { get; set; }

        private SnooperExportedDataObjectDNS() : base() { } //EF

        public SnooperExportedDataObjectDNS(SnooperExportBase exportBase) : base(exportBase)
        {
            
        }
    }
}
