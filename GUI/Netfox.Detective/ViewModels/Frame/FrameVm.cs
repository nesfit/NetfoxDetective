// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Castle.Windsor;
using Netfox.Detective.ViewModelsDataEntity;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;

namespace Netfox.Detective.ViewModels.Frame
{
    /// <summary>
    ///     Representation of frame
    /// </summary>
    public class FrameVm : DetectiveDataEntityViewModelBase
    {
        public enum FrameDirection
        {
            Unknow,
            Up,
            Down
        }

        private uint _selectedLength;
        private uint _selectedOffset;
        
        public FrameVm(WindsorContainer applicationWindsorContainer, PmFrameBase model) : base(applicationWindsorContainer, model)
        {
            this.FwFrame = model;
            this._initialized = true;
        }
        
        public FrameDirection Direction { get; set; }

        public object ModelData => this.FwFrame;
        
        public PmFrameBase FwFrame { get; }

        public PmFrameBase Frame => this.FwFrame;
        
        public Int32 SrcPort => this.FwFrame.SrcPort;

        public Int32 DstPort => this.FwFrame.DstPort;

        public Int64 FrameIndex => this.FwFrame?.FrameIndex ?? 0;

        public Guid Id => this.FwFrame.Id;

        public string PacketType => this.FwFrame.IpProtocol.ToString();

        public DateTime TimeStamp => this.FwFrame.TimeStamp;

        public string L2SourceAddress { get; }

        public string L2TargetAddress { get; }

        public string IPSourceAddress => this.FwFrame.SourceEndPoint.Address.ToString();

        public string IPDestinationAddress => this.FwFrame.DestinationEndPoint.Address.ToString();

        public IPEndPoint Source => this.FwFrame.SourceEndPoint;

        public IPEndPoint Destination => this.FwFrame.DestinationEndPoint;

        public Int64 FrameSize => this.FwFrame?.OriginalLength ?? 0;

        public List<PmFrameBase> DecapsulatedFromFrames => this.FwFrame?.DecapsulatedFromFrames;

        public List<PmFrameBase> EncapsulatesFrames => this.FwFrame?.EncapsulatedFrames;

        public uint SelectedOffset
        {
            get { return this._selectedOffset; }
            set
            {
                this._selectedOffset = value;
                this.OnPropertyChanged();
            }
        }

        public uint SelectedLength
        {
            get { return this._selectedLength; }
            set
            {
                this._selectedLength = value;
                this.OnPropertyChanged();
            }
        }

        public int FrameFlowSize
        {
            get
            {
                if(this.FwFrame == null) { return 0; }

                if(this.Direction == FrameDirection.Unknow || this.Direction == FrameDirection.Up) { return (int) this.FwFrame.OriginalLength; }
                return -((int) this.FwFrame.OriginalLength);
            }
        }

        public IEnumerable<FrameHexLine> FrameHexContent
        {
            get
            {
                if(this.FwFrame == null) { yield break; }

                var bytes = this.FwFrame.PmPacket.PacketInfo.Bytes;
                var len = bytes.Length;
                for(ushort i = 0; i < len;)
                {
                    yield return new FrameHexLine
                    {
                        Offset = i.ToString("X4"),
                        //Ascii = ASCIIEncoding.ASCII.GetString(bytes, i, Math.Min(7, len - i)),
                        Ascii = RawAsciiBytes(bytes, i, Math.Min(8, len - i)),
                        Hexa0 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa1 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa2 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa3 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa4 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa5 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa6 = i < len? bytes[i++].ToString("X2") : String.Empty,
                        Hexa7 = i < len? bytes[i++].ToString("X2") : String.Empty
                    };
                }
            }
        }

        /// <summary>
        ///     Returns an enumeration of Name Value pairs. Note that
        ///     this enumeration can be nested, ie, Value may represent another
        ///     enumeration.
        /// </summary>
        public IEnumerable AllPacketHeaders
        {
            get
            {
                if(this.FwFrame == null) { yield break; }

                uint currentOffset = 0;
                var p = this.FwFrame.PmPacket.PacketInfo;
                while(p != null)
                {
                    var length = (uint) p.Bytes.Length;

                    if(p is EthernetPacket)
                    {
                        var q = p as EthernetPacket;
                        yield return new GenericFiledVm
                        {
                            Offset = 0,
                            Length = length,
                            Id = "MediaView",
                            Name = "Ethernet",
                            Value = $"smac: {q.SourceHwAddress}, dmac: {q.DestinationHwAddress}",
                            Content = new object[]
                            {
                                new
                                {
                                    Name = "Destination Address",
                                    Value = q.DestinationHwAddress
                                },
                                new
                                {
                                    Name = "Source Address",
                                    Value = q.SourceHwAddress
                                },
                                new
                                {
                                    Name = "Type/Length",
                                    Value = q.Type
                                }
                            }
                        };

                        currentOffset += (uint) q.Header.Length;
                    }
                    if(p is IPv4Packet)
                    {
                        var q = p as IPv4Packet;
                        yield return new GenericFiledVm
                        {
                            Offset = currentOffset,
                            Length = length - currentOffset,
                            Id = "IpView",
                            Name = "Internet Protocol",
                            Value = $"sa: {q.SourceAddress}, da: {q.DestinationAddress}",
                            Content = new object[]
                            {
                                new
                                {
                                    Name = "Version",
                                    Value = q.Version,
                                    Position = IPv4Fields.VersionAndHeaderLengthPosition,
                                    Length = IPv4Fields.VersionAndHeaderLengthLength
                                },
                                new
                                {
                                    Name = "IHL",
                                    Value = q.HeaderLength
                                },
                                new
                                {
                                    Name = "Type-of-service",
                                    Value = q.TypeOfService
                                },
                                new
                                {
                                    Name = "Total Length",
                                    Value = q.TotalLength
                                },
                                new
                                {
                                    Name = "Identification",
                                    Value = q.Id
                                },
                                new
                                {
                                    Name = "Flags",
                                    Value = q.FragmentFlags
                                },
                                new
                                {
                                    Name = "Fragment Offset",
                                    Value = q.FragmentOffset
                                },
                                new
                                {
                                    Name = "Time-to-live",
                                    Value = q.TimeToLive
                                },
                                new
                                {
                                    Name = "Protocol",
                                    Value = q.Protocol
                                },
                                new
                                {
                                    Name = "Header Checksum",
                                    Value = q.Checksum
                                },
                                new
                                {
                                    Name = "Source Address",
                                    Value = q.SourceAddress
                                },
                                new
                                {
                                    Name = "Destination Address",
                                    Value = q.DestinationAddress
                                }
                            }
                        };

                        currentOffset += (uint) q.Header.Length;
                    }
                    if(p is ICMPv4Packet)
                    {
                        var q = p as ICMPv4Packet;
                        yield return new GenericFiledVm
                        {
                            Offset = currentOffset,
                            Length = length - currentOffset,
                            Id = "IpView",
                            Name = "Internet Control Message Protocol",
                            Value = $"{q.TypeCode} id=0x{q.ID:x4} seq={q.Sequence}",
                            Content = new object[]
                            {
                                new
                                {
                                    Name = "Type",
                                    Value = (ushort)q.TypeCode >> 8
                                },
                                new
                                {
                                    Name = "Code",
                                    Value = (ushort)q.TypeCode & 0x00FF
                                },
                                new
                                {
                                    Name = "Checksum",
                                    Value = "0x"+q.Checksum.ToString("x4")
                                },
                                new
                                {
                                    Name = "Identifier",
                                    Value = "0x"+q.ID.ToString("x4")
                                },
                                new
                                {
                                    Name = "Sequence number",
                                    Value = q.Sequence
                                },
                                new
                                {
                                    Name = "Data",
                                    Value = BitConverter.ToString(q.Data).Replace("-","").ToLower()
                                }
                            }
                        };

                        currentOffset += (uint) q.Header.Length;
                    }
                    if(p is IPv6Packet)
                    {
                        var q = p as IPv6Packet;
                        yield return new GenericFiledVm
                        {
                            Offset = currentOffset,
                            Length = length - currentOffset,
                            Id = "IpView",
                            Name = "Internet Protocol",
                            Value = $"sa: {q.SourceAddress}, da: {q.DestinationAddress}",
                            Content = new object[]
                            {
                                new
                                {
                                    Name = "Version",
                                    Value = q.Version
                                },
                                new
                                {
                                    Name = "TrafficClass",
                                    Value = q.TrafficClass
                                },
                                new
                                {
                                    Name = "Flow Label",
                                    Value = q.FlowLabel
                                },
                                new
                                {
                                    Name = "Payload Length",
                                    Value = q.PayloadLength
                                },
                                new
                                {
                                    Name = "Next Header",
                                    Value = q.NextHeader
                                },
                                new
                                {
                                    Name = "Hop Limit",
                                    Value = q.HopLimit
                                },
                                new
                                {
                                    Name = "Source Address",
                                    Value = q.SourceAddress
                                },
                                new
                                {
                                    Name = "Destination Address",
                                    Value = q.DestinationAddress
                                }
                            }
                        };

                        currentOffset += (uint) q.Header.Length;
                    }
                    if(p is TcpPacket)
                    {
                        var q = p as TcpPacket;
                        yield return new GenericFiledVm
                        {
                            Offset = currentOffset,
                            Length = length - currentOffset,
                            Id = "TransportView",
                            Name = "Transmission Control Protocol",
                            Value = $"sp: {q.SourcePort}, dp: {q.DestinationPort}, sn: {q.SequenceNumber}, ack: {q.AcknowledgmentNumber}",
                            Content = new object[]
                            {
                                new
                                {
                                    Name = "Source Port",
                                    Value = q.SourcePort
                                },
                                new
                                {
                                    Name = "Destination Port",
                                    Value = q.DestinationPort
                                },
                                new
                                {
                                    Name = "Sequence Number",
                                    Value = q.SequenceNumber
                                },
                                new
                                {
                                    Name = "Acknowledgment Port",
                                    Value = q.AcknowledgmentNumber
                                },
                                new
                                {
                                    Name = "Offset",
                                    Value = q.DataOffset
                                },
                                // new { Name = "TCP Flags", Value = String.Format("0x{0:X2} {1}", q.AllFlags, q.GetFlagsString()),
                                new GenericFiledVm
                                {
                                    Name = "TCP Flags",
                                    Value = $"0x{q.AllFlags:X2}",
                                    Id = "TcpFlagsView",
                                    Content = new object[]
                                    {
                                        new
                                        {
                                            Name = "Reduced",
                                            Value = q.CWR
                                        },
                                        new
                                        {
                                            Name = "Echo",
                                            Value = q.ECN
                                        },
                                        new
                                        {
                                            Name = "Urgent",
                                            Value = q.Urg
                                        },
                                        new
                                        {
                                            Name = "Ack",
                                            Value = q.Ack
                                        },
                                        new
                                        {
                                            Name = "Push",
                                            Value = q.Psh
                                        },
                                        new
                                        {
                                            Name = "Reset",
                                            Value = q.Rst
                                        },
                                        new
                                        {
                                            Name = "Syn",
                                            Value = q.Syn
                                        },
                                        new
                                        {
                                            Name = "Fin",
                                            Value = q.Fin
                                        }
                                    }
                                },
                                new
                                {
                                    Name = "Window",
                                    Value = q.WindowSize
                                },
                                new
                                {
                                    Name = "Checksum",
                                    Value = q.Checksum
                                },
                                new
                                {
                                    Name = "Urgent Pointer",
                                    Value = q.UrgentPointer
                                },
                                new
                                {
                                    Name = "TCP Options",
                                    // TODO: Someone haven't implemented TCP Options
                                    Value = "...TODO..." 
                                }
                            }
                        };

                        currentOffset += (uint) q.Header.Length;
                    }
                    if(p is UdpPacket)
                    {
                        var q = p as UdpPacket;
                        yield return new GenericFiledVm
                        {
                            Offset = currentOffset,
                            Length = length - currentOffset,
                            Id = "TransportView",
                            Name = "User Datagram Protocol",
                            Value = $"sp: {q.SourcePort}, dp: {q.DestinationPort}, len: {q.Length}",
                            Content = new object[]
                            {
                                new
                                {
                                    Name = "Source Port",
                                    Value = q.SourcePort
                                },
                                new
                                {
                                    Name = "Destination Port",
                                    Value = q.DestinationPort
                                },
                                new
                                {
                                    Name = "Length",
                                    Value = q.Length
                                },
                                new
                                {
                                    Name = "Checksum",
                                    Value = q.Checksum
                                }
                            }
                        };

                        currentOffset += (uint) q.Header.Length;
                    }

                    if(p is TcpPacket)
                    {
                        var q = p as TcpPacket;

                        if(q.PayloadData.Length > 0)
                        {
                            yield return new GenericFiledVm
                            {
                                Offset = currentOffset,
                                Length = (uint) q.PayloadData.Length,
                                Id = "ApplicationView",
                                Name = "TCP Application data",
                                Value = $"Length : {q.PayloadData.Length}B",
                                Content = new object[]
                                {
                                    new
                                    {
                                        Name = "ASCII Data",
                                        Value = RawAsciiBytes(q.PayloadData)
                                    }
                                }
                            };
                        }
                    }

                    if(p is UdpPacket)
                    {
                        var q = p as UdpPacket;

                        if(q.PayloadData.Length > 0)
                        {
                            yield return new GenericFiledVm
                            {
                                Offset = currentOffset,
                                Length = (uint) q.PayloadData.Length,
                                Id = "ApplicationView",
                                Name = "UDP Application data",
                                Value = $"Length : {q.PayloadData.Length}B",
                                Content = new object[]
                                {
                                    new
                                    {
                                        Name = "ASCII Data",
                                        Value = RawAsciiBytes(q.PayloadData)
                                    }
                                }
                            };
                        }
                    }


                    p = p.PayloadPacket;
                }
            }
        }

        private static string RawAsciiBytes(byte[] bytes, bool allowLineBreak = false)
        {
            return RawAsciiBytes(bytes, 0, bytes.Length, allowLineBreak);
        }

        private static string RawAsciiBytes(byte[] bytes, int index, int count, bool allowLineBreak = false)
        {
            var result = string.Empty;
            var lastIndex = index + count;

            for(var i = index; i < lastIndex; i++)
            {
                if(bytes[i] > 0x20 && bytes[i] < 0x7E || (allowLineBreak && (bytes[i] == 0x0D || bytes[i] == 0x0A))) { result += (char) bytes[i]; }
                else
                { result += '.'; }
            }

            return result;
        }

        public class FrameHexLine
        {
            public string Offset { get; set; }
            public string Ascii { get; set; }
            public string Hexa0 { get; set; }
            public string Hexa1 { get; set; }
            public string Hexa2 { get; set; }
            public string Hexa3 { get; set; }
            public string Hexa4 { get; set; }
            public string Hexa5 { get; set; }
            public string Hexa6 { get; set; }
            public string Hexa7 { get; set; }
        }

        public class GenericFiledVm
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public object Content { get; set; }
            public string Id { get; set; }
            public uint Offset { get; set; }
            public uint Length { get; set; }
        }
    }
}