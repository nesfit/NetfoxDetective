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

using System.ComponentModel.DataAnnotations.Schema;

namespace Netfox.Snoopers.SnooperDNS.Models.Message
{
    [ComplexType]
    public class DNSBase
    {
        public string Name { get; set; }
        public DNSQType QType { get; set; }
        public DNSQClass QClass { get; set; }

        public enum DNSQType : int 
        {
            A = 1,
            NS,
            MD,
            MF,
            CNAME,
            SOA,
            MB,
            MG,
            MR,
            NULL,
            WKS,
            PTR,
            HINFO,
            MINFO,
            MX,
            TXT,
            RP,
            AFSDB,
            X25,
            ISDN,
            RT,
            NSAP,
            NSAPPTR,
            SIG,
            KEY,
            PX,
            GPOS,
            AAAA,
            LOC,
            NXT,
            EID,
            NIMLOC,
            SRV,
            ATMA,
            NAPTR,
            KX,
            CERT,
            A6,
            DNAME,
            SINK,
            OPT,
            APL,
            DS,
            SSHFP,
            IPSECKEY,
            RRSIG,
            NSEC,
            DNSKEY,
            DHCID,
            NSEC3,
            NSEC3PARAM,
            TLSA,
            SMIMEA,
            HIP = 55,
            NINFO,
            RKEY,
            TALINK,
            CDS,
            CDNSKEY,
            OPENPGPKEY,
            CSYNC,
            SPF = 99,
            UINFO,
            UID,
            GID,
            UNSPEC,
            NID,
            L32,
            L64,
            LP,
            EUI48,
            EUI64,
            TKEY = 249,
            TSIG,
            IXFR,
            AXFR,
            MAILB,
            MAILA,
            ANY,
            URI,
            CAA,
            AVC,
            TA = 32768,
            DLV,
            UNASSIGNED  
        }

        public enum DNSQClass : int
        {
            IN = 1,
            CH = 3,
            HS = 4,
            QCLASSNONE = 254,
            QCLASSANY = 255,
            UNASSIGNED = 0
        }

        public override string ToString() { return $"{nameof(this.Name)}: {this.Name}, {nameof(this.QType)}: {this.QType}, {nameof(this.QClass)}: {this.QClass}"; }
    }
}