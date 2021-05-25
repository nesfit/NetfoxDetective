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

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperDNS.Models
{
   public class SnooperExportedDataObjectDNS : SnooperExportedObjectBase
    {
        public enum DNSQueryType : int
        {
            QUERY = 0,
            IQUERY = 1,
            STATUS = 2,
            NOTIFY = 4,
            UPDATE = 5,
            UNASSIGNED = 3
        }

        public enum DNSResponseCode : int
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

        public int __MessageId { get; set; }
        public int __Flags { get; set; }

        [NotMapped]
        public ushort MessageId
        {
            get
            {
                unchecked
                {
                    return (ushort)__MessageId;
                }
            }
            set
            {
                unchecked
                {
                    __MessageId = (int)value;
                }
            }
        }

        [NotMapped]
        public ushort Flags
        {
            get
            {
                unchecked
                {
                    return (ushort)__Flags;
                }
            }
            set
            {
                unchecked
                {
                    __Flags = (int)value;
                }
            }
        }
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

        public PersistableJsonSerializableDNSBase Queries { get; set; }
        public PersistableJsonSerializableDNSResponse Answer { get; set; }
        public PersistableJsonSerializableDNSResponse Authority { get; set; }
        public PersistableJsonSerializableDNSResponse Additional { get; set; }

        private SnooperExportedDataObjectDNS() : base() { } //EF

        public SnooperExportedDataObjectDNS(SnooperExportBase exportBase) : base(exportBase)
        {
            
        }
    }
}
