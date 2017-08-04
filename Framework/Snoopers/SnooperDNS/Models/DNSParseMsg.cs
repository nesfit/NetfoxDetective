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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.Core.Database.Wrappers;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.SnooperDNS.Models.Message;
using PacketDotNet;

namespace Netfox.SnooperDNS.Models
{
    public class DNSParseMsg
    {
        private readonly PDUStreamReader _reader;
        public IEnumerable<PmFrameBase> Frames;
        public string InvalidReason;
        public DateTime Timestamp;
        public bool Valid;
        public List<IExportSource> ExportSources;
        public ushort Flags;
        public ushort MessageId;
        public PersistableJsonSerializable<DNSBase> Queries;
        public PersistableJsonSerializable<DnsResponse> Answer, Authority, Additional;
        private ushort _countQuestion, _countAnswer, _countAuthority, _countAdditional;
        private long _beginMessage;

        public enum DNSResponseType
        {
            ANSWER,
            AUTHORITY,
            ADDITIONAL
        }

        public DNSParseMsg(PDUStreamReader reader)
        {
            // fill default values and store things we'll need later
            this._reader = reader;
            this.Valid = true;
            this.InvalidReason = string.Empty;
            this.ExportSources = new List<IExportSource>();
            this._beginMessage = 0;
            this.Queries = new PersistableJsonSerializable<DNSBase>();
            this.Answer = new PersistableJsonSerializable<DnsResponse>();
            this.Authority = new PersistableJsonSerializable<DnsResponse>();
            this.Additional = new PersistableJsonSerializable<DnsResponse>();
            // do the parsing itself
            this.Parse();
        }

        private void ReadHeader()
        {
            // store begin of message - it is important for parsing pointer
            if (this._reader.PDUStreamBasedProvider.Conversation.L4Conversation.L4ProtocolType == IPProtocolType.TCP)
            {
                this._beginMessage = 2;
                this._reader.ReadUInt16();
            }
            this._reader.ReadBigEndian = true;
            this.MessageId = this._reader.ReadUInt16();
            this.Flags = this._reader.ReadUInt16();
            this._countQuestion = this._reader.ReadUInt16();
            this._countAnswer = this._reader.ReadUInt16();
            this._countAuthority = this._reader.ReadUInt16();
            this._countAdditional = this._reader.ReadUInt16();
        }

        private long ChangePositionOfStream(byte beginPointer, long position)
        {
            var pointer = (beginPointer << 8) + this._reader.ReadByte();
            pointer -= (192 << 8);
            if (position == -1) position = this._reader.PDUStreamBasedProvider.Position;
            this._reader.PDUStreamBasedProvider.Seek(pointer + this._beginMessage, SeekOrigin.Begin);
            return position;
        }

        private byte ReadLengthOfLabel(ref long position)
        {
            var count = this._reader.ReadByte();
            // if the first two bits are ones, this byte is part of pointer 
            if(count >= 192)
            {
                position = this.ChangePositionOfStream(count, position);
                count = this._reader.ReadByte();
            }
            return count;
        }


        private static string ToStringDomainName(List<char[]> domainName)
        {
            var stringBuild = new StringBuilder();
            foreach(var label in domainName)
            {
                stringBuild.Append(label);
            }
            return stringBuild.ToString();
        }

        private List<char[]> ReadDomainName()
        {
            long position = -1;
            var domainName = new List<char[]>();
            var addDelimiter = false;
            byte count;
            var dot = new[]
            {
                '.'
            };

            while ((count = this.ReadLengthOfLabel(ref position)) != 0)
            {
                if(addDelimiter) { domainName.Add(dot); }
                else
                {
                    addDelimiter = true;
                }
                domainName.Add((this._reader.ReadChars(count)));
            }

            // change position in the stream, if pointer was read
            if (position != -1)
            {
                this._reader.PDUStreamBasedProvider.Seek(position, SeekOrigin.Begin);
            }
            return domainName;
        }

        private void ReadQuery()
        {
            for (short i = 0; i < this._countQuestion; i++)
            {
                var query = new DNSBase
                {
                    Name = ToStringDomainName(this.ReadDomainName()),
                };
                Int32 qType = this._reader.ReadUInt16();
                query.QType = Enum.IsDefined(typeof(DNSBase.DNSQType), qType) ? (DNSBase.DNSQType)qType : DNSBase.DNSQType.UNASSIGNED;
                Int32 qClass = this._reader.ReadUInt16();
                query.QClass = Enum.IsDefined(typeof(DNSBase.DNSQClass), qClass) ? (DNSBase.DNSQClass)qClass : DNSBase.DNSQClass.UNASSIGNED;
                this.Queries.Add(query);
            }
        }

        private void ReadResponse(ushort countMessages, DNSResponseType responseType)
        {
            for(ushort i = 0; i < countMessages; i++)
            { 
                var name = this.ReadDomainName();

                Int32 tmpNumber = this._reader.ReadUInt16();
                var qType = Enum.IsDefined(typeof(DNSBase.DNSQType), tmpNumber) ? (DNSBase.DNSQType)tmpNumber : DNSBase.DNSQType.UNASSIGNED;

                DnsResponse response;
                switch (qType)
                {
                    case DNSBase.DNSQType.A:
                    case DNSBase.DNSQType.AAAA:
                        response = new DnsResponseA();
                        break;
                    case DNSBase.DNSQType.CNAME:
                    case DNSBase.DNSQType.NS:
                    case DNSBase.DNSQType.PTR:
                    case DNSBase.DNSQType.TXT:
                        response = new DnsResponseName();
                        break;
                    case DNSBase.DNSQType.MX:
                        response = new DnsResponseMX();
                        break;
                    case DNSBase.DNSQType.SOA:
                        response = new DnsResponseSOA();
                        break;
                    case DNSBase.DNSQType.SRV:
                        response = new DnsResponseSRV();
                        break;
                    case DNSBase.DNSQType.NAPTR:
                        response = new DnsResponseNAPTR();
                        break;
                    default:
                        response = new DnsResponseOther();
                        break;
                }
 
                response.QType = qType;
                tmpNumber = this._reader.ReadUInt16();
                response.QClass = Enum.IsDefined(typeof(DNSBase.DNSQClass), tmpNumber) ? (DNSBase.DNSQClass)tmpNumber : DNSBase.DNSQClass.UNASSIGNED;
                response.TTL = this._reader.ReadUInt32();

                this.ReadResponseRData(response, name);
                switch (responseType)
                {
                    case DNSResponseType.ANSWER:
                        this.Answer.Add(response);
                        break;
                    case DNSResponseType.AUTHORITY:
                        this.Authority.Add(response);
                        break;
                    case DNSResponseType.ADDITIONAL:
                        this.Additional.Add(response);
                        break;
                }
                
            }
        }

        private void ReadDNSResponseName(DnsResponseName response)
        {
            response.RDataName = ToStringDomainName(this.ReadDomainName());
        }

        private void ReadDNSResponseTXT(DnsResponseName response)
        {
            var count = this._reader.ReadByte();
            response.RDataName = string.Concat(this._reader.ReadChars(count));
        }

        private void ReadDNSResponseA(DnsResponseA response, int countBytes)
        {
            var IP = new byte[countBytes];
            this._reader.Read(IP, 0, countBytes);
            response.IPAddress = new IPAddressEF(IP);
        }

        private void ReadDNSResponseMX(DnsResponseMX response)
        {
            response.Preference = this._reader.ReadUInt16();
            response.Exchange = ToStringDomainName(this.ReadDomainName());
        }

        private void ReadDNSResponseSOA(DnsResponseSOA response)
        {
            response.MName = ToStringDomainName(this.ReadDomainName());
            response.RName = ToStringDomainName(this.ReadDomainName());
            response.Serial = this._reader.ReadUInt32();
            response.Refresh = this._reader.ReadUInt32();
            response.Retry = this._reader.ReadUInt32();
            response.Expire = this._reader.ReadUInt32();
            response.Minimum = this._reader.ReadUInt32();
        }

        private void ReadDNSResponseSRV(DnsResponseSRV response, List<char[]> domainName)
        {
            response.Service = string.Concat(domainName[0]);
            response.Protocol = string.Concat(domainName[2]);
            response.Priority = this._reader.ReadUInt16();
            response.Weight = this._reader.ReadUInt16();
            response.Port = this._reader.ReadUInt16();
            response.Target = ToStringDomainName(this.ReadDomainName());
        }

        private void ReadDNSResponseOther(DnsResponseOther response, int qDataLength)
        {
            response.DataBytes = new byte[qDataLength];
            this._reader.Read(response.DataBytes, 0, qDataLength);
        }

        private void ReadDNSResponseNAPTR(DnsResponseNAPTR response)
        {
            response.Order = this._reader.ReadUInt16();
            response.Preference = this._reader.ReadUInt16();
            var countFlags = this._reader.ReadByte();
            response.Flags = this._reader.ReadChars(countFlags);
            countFlags = this._reader.ReadByte();
            response.Services = string.Concat(this._reader.ReadChars(countFlags));
            countFlags = this._reader.ReadByte();
            response.Regexp = string.Concat(this._reader.ReadChars(countFlags));
            response.Replacement = ToStringDomainName(this.ReadDomainName());
        }

        private void ReadResponseRData(DnsResponse response, List<char[]> domainName)
        {
            var qDataLength = this._reader.ReadUInt16();
            switch(response.QType)
            {
                case DNSBase.DNSQType.A:
                    this.ReadDNSResponseA((DnsResponseA)response, 4);
                    break;
                case DNSBase.DNSQType.AAAA:
                    this.ReadDNSResponseA((DnsResponseA)response, 16);
                    break;
                case DNSBase.DNSQType.CNAME:
                case DNSBase.DNSQType.NS:
                case DNSBase.DNSQType.PTR:
                    this.ReadDNSResponseName((DnsResponseName) response);
                    break;
                case DNSBase.DNSQType.MX:
                    this.ReadDNSResponseMX((DnsResponseMX) response);
                    break;
                case DNSBase.DNSQType.TXT:
                    this.ReadDNSResponseTXT((DnsResponseName) response);
                    break;
                case DNSBase.DNSQType.SOA:
                    this.ReadDNSResponseSOA((DnsResponseSOA) response);
                    break;
                case DNSBase.DNSQType.SRV:
                    this.ReadDNSResponseSRV((DnsResponseSRV) response, domainName);
                    domainName.RemoveRange(0, 4);
                    break;
               case DNSBase.DNSQType.NAPTR:
                    this.ReadDNSResponseNAPTR((DnsResponseNAPTR) response);
                    break;
                default:
                    this.ReadDNSResponseOther((DnsResponseOther) response, qDataLength);
                    break;
            }
            response.Name = ToStringDomainName(domainName);
        }

        private void Parse()
        {
            // transform reader to stream provider to get timestamp and frame numbers values
            var streamProvider = this._reader.PDUStreamBasedProvider;
            this.Frames = streamProvider.ProcessedFrames;
            if (streamProvider.GetCurrentPDU() != null)
            {
                this.Timestamp = streamProvider.GetCurrentPDU().FirstSeen;
            }
            else
            {
                this.InvalidReason = "could not retrieve PDU";
                this.ExportSources.Add(streamProvider.Conversation);
                this.Valid = false;
                return;
            }

            //Console.WriteLine("DNSMsg created, frame numbers: " + string.Join(",", Frames.ToArray()));
            if (!streamProvider.GetCurrentPDU().L7Conversation.ApplicationTags.Any())
            {
                this.Valid = false;
                this.InvalidReason = "no application tag";
                this.ExportSources.Add(streamProvider.GetCurrentPDU());
                return;
            }

            this.ExportSources.Add(streamProvider.GetCurrentPDU());

            try
            {
                this.ReadHeader();
                this.ReadQuery();
                this.ReadResponse(this._countAnswer, DNSResponseType.ANSWER);
                this.ReadResponse(this._countAuthority, DNSResponseType.AUTHORITY);
                this.ReadResponse(this._countAdditional, DNSResponseType.ADDITIONAL);
            }

            catch(Exception ex) {
                this.Valid = false;
                this.InvalidReason = $"Message parsing failed. {ex}";
            }
        }
    }
}