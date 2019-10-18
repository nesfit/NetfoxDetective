/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Net;
using Netfox.Framework.Models.PmLib.SupportedTypes;
using PacketDotNet;

namespace Netfox.Framework.Models.PmLib
{

    /// <summary> Gets a IP flags.</summary>
    public enum PmPacketIPv4FlagsEnum
    {
        /// <summary> An enum constant representing the not i pv 4 option.</summary>
        NotIPv4 = -1,

        /// <summary> An enum constant representing the no flags option.</summary>
        NoFlags = 0,

        /// <summary> An enum constant representing the mf option.</summary>
        Mf = 1, //more fragments

        /// <summary> An enum constant representing the df option.</summary>
        Df = 2, //do not fragment

        /// <summary> An enum constant representing the reserved option.</summary>
        Reserved = 4 //must be zero
    }
    /// <summary>
    ///     Represents a basic structure containing all relevant packet information.
    /// </summary>
    public class PmPacket 
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="linkLayer"></param>
        /// <param name="packetData"></param>
        public PmPacket(PmLinkType linkLayer, Byte[] packetData)
        {
            this.PacketInfo = Packet.ParsePacket(PmSupportedTypes.ConvertLinkTypeToPacketdotNet(linkLayer), packetData);
            this.UpdateCalculatedOffsets();
        }

        /// <summary>
        ///     Gets or sets an application name.
        /// </summary>
        public String ApplicationName { get; set; }

        #region L2
        /// <summary>
        ///     Gets a protocol type carried in the Ethernet frame.
        ///     If current packet is not Ethernet frame it yields to EthernetPacketType.NONE
        /// </summary>
        public EthernetPacketType EthernetType
        {
            get
            {
                var eth = (EthernetPacket) this.PacketInfo.Extract(typeof(EthernetPacket));
                return eth?.Type ?? EthernetPacketType.None;
            }
        }

        public EthernetPacket Ethernet
        {
            get
            {
                return (EthernetPacket)this.PacketInfo.Extract(typeof(EthernetPacket));
            }
        }
        #endregion

        /// <summary>
        ///     Gets or sets the lenght of the frame.
        /// </summary>
        public Int32 FrameLength { get; set; }

        /// <summary>
        ///     Gets or sets the unique frame locator.
        /// </summary>
        public Object FrameLocator { get; set; }

        /// <summary>
        ///     Represents an offset of this frame in the source file.
        ///     Using this offset it is possible to directly access the frame
        ///     within source pcap file.
        /// </summary>
        public Int64 FramePcapOffset { get; set; }

        /// <summary>
        ///     Gets or sets time offset of the frame.
        /// </summary>
        public DateTime FrameTimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets IP protocol number
        /// </summary>
        public IPProtocolType IPProtocol { get; set; }

        /// <summary>
        ///     Gets an underlaying packet info object.
        /// </summary>
        public Packet PacketInfo { get; }

        public DateTime TimeStamp
        {
            get { return this.FrameTimeStamp; }
            set { this.FrameTimeStamp = value; }
        }

        #region Various offsets in the packet
        /// <summary>
        ///     Gets an offset of a network protocol header.
        /// </summary>
        public Int16 PacketHeaderOffset { get; private set; }

        /// <summary>
        ///     Gets an offset of a transport protocol header.
        /// </summary>
        public Int16 SegmentHeaderOffset { get; private set; }

        /// <summary>
        ///     Gets an offset of a application payload data.
        /// </summary>
        public Int16 SegmentPayloadOffset { get; private set; }

        /// <summary>
        ///     Gets a length of application payload data.
        /// </summary>
        public Int32 SegmentPayloadLength { get; private set; }

        /// <summary>
        ///     Method calculates offsets of L2, L3 and L4 headers so that they could be easily accessed and parsed afterwards
        /// </summary>
        private void UpdateCalculatedOffsets()
        {
            var packet = this.PacketInfo;
            while(packet != null)
            {
                if(packet is EthernetPacket)
                {
                    var tpacket = packet as EthernetPacket;
                    this.PacketHeaderOffset = (Int16) tpacket.Header.Length;
                }
                if(packet is IPv4Packet)
                {
                    var tpacket = packet as IPv4Packet;
                    this.SegmentHeaderOffset = (Int16) (this.PacketHeaderOffset + tpacket.Header.Length);
                }
                if(packet is IPv6Packet)
                {
                    var tpacket = packet as IPv6Packet;
                    this.SegmentHeaderOffset = (Int16) (this.PacketHeaderOffset + (tpacket.TotalLength - tpacket.PayloadLength));
                }
                if(packet is TcpPacket)
                {
                    var tpacket = packet as TcpPacket;
                    this.SegmentPayloadOffset = (Int16) (this.SegmentHeaderOffset + (tpacket.DataOffset*4));
                    this.SegmentPayloadLength = tpacket.PayloadData?.Length ?? 0;
                }
                if(packet is UdpPacket)
                {
                    var tpacket = packet as UdpPacket;
                    this.SegmentPayloadOffset = (Int16) (this.SegmentHeaderOffset + tpacket.Header.Length);
                    this.SegmentPayloadLength = tpacket.PayloadData?.Length ?? 0;
                }
                packet = packet.PayloadPacket;
            }
        }
        #endregion

        #region L3

        #region IPv4/IPv6 common informations
        /// <summary>
        ///     Gets the IP packet enbcapulated in the current CapPacket object.
        /// </summary>
        /// <value>The IPPacket instance if current CapPacket contains this pacekt; otherwise null.</value>
        public IpPacket IP
        {
            get
            {
                var p = (IpPacket) this.PacketInfo.Extract(typeof(IpPacket));
                return p;
            }
        }

        /// <summary>
        ///     Gets an IP version, i.e. IPv4 or IPv6.
        /// </summary>
        public IpVersion IpVersion
        {
            get
            {
                var ip = (IpPacket) this.PacketInfo.Extract(typeof(IpPacket));
                return ip?.Version ?? IpVersion.IPv4;
            }
        }

        /// <summary>
        ///     Gets a protocol type carried in the IP packet.
        /// </summary>
        public IPProtocolType ProtocolIdentifier
        {
            get
            {
                var ip = (IpPacket) this.PacketInfo.Extract(typeof(IpPacket));
                return ip?.Protocol ?? IPProtocolType.NONE;
            }
        }

        /// <summary>
        ///     Gets a source address.
        /// </summary>
        public IPAddress SourceAddress
        {
            get
            {
                var ip = (IpPacket) this.PacketInfo.Extract(typeof(IpPacket));
                return  ip?.SourceAddress ?? IPAddress.None;
            }
        }

        /// <summary>
        ///     Gets a destination address.
        /// </summary>
        public IPAddress DestinationAddress
        {
            get
            {
                var ip = (IpPacket) this.PacketInfo.Extract(typeof(IpPacket));
                return  ip?.DestinationAddress ?? IPAddress.None;
            }
        }

        /// <summary>
        ///     Gets the source end point of the encapsulated pacekt.
        /// </summary>
        /// <value>The source end point.</value>
        public IPEndPoint SourceEndPoint => new IPEndPoint(this.IP.SourceAddress, this.SourceTransportPort);

        /// <summary>
        ///     Gets the target end point of the encapsulated packet.
        /// </summary>
        /// <value>The target end point.</value>
        public IPEndPoint DestinationEndPoint => new IPEndPoint(this.IP.DestinationAddress, this.DestinationTransportPort);

        public PmPacketIPv4FlagsEnum PmPacketIPv4Flags
        {
            get
            {
                var ipv4 = this.PacketInfo.Extract(typeof(IpPacket)) as IPv4Packet;
                return (ipv4 != null)? (PmPacketIPv4FlagsEnum) ipv4.FragmentFlags : PmPacketIPv4FlagsEnum.NotIPv4;
            }
        }

        public Int32 Pv4FragmentOffset
        {
            get
            {
                var ipv4 = this.PacketInfo.Extract(typeof(IpPacket)) as IPv4Packet;
                return ipv4?.FragmentOffset ?? -1;
            }
        }
        #endregion

        #region IPv4 related information
        /// <summary>
        ///     Gets the IPv4 packet enbcapulated in the current CapPacket object.
        /// </summary>
        /// <value>The IPV4Packet instance if current CapPacket contains this pacekt; otherwise null.</value>
        public IPv4Packet Pv4 => (IPv4Packet) this.PacketInfo.Extract(typeof(IPv4Packet));

        /// <summary>
        ///     In case of IPv4 packet it returns FragmentOffset field from IPv4 header
        ///     FragmentOffset is here measured in 8 byte increments. For example Wireshark shows 'Fragment offset' in bytes, therefore its shows 8 times bigger value.
        /// </summary>
        public Int32 Ipv4FragmentOffset
        {
            get
            {
                var ipv4 = this.PacketInfo.Extract(typeof(IpPacket)) as IPv4Packet;
                return ipv4?.FragmentOffset ?? -1;
            }
        }

        /// <summary>
        ///     Retrieves IPv4 identification
        /// </summary>
        public Int32 Ipv4Identification
        {
            get
            {
                var ipv4 = this.PacketInfo.Extract(typeof(IpPacket)) as IPv4Packet;
                return ipv4?.Id ?? -1;
            }
        }

        //TODO: Vesely - Return also unspecified value
        /// <summary>
        ///     Retrieves IPv4 fragmentation DF bit
        /// </summary>
        public Boolean Ipv4DFbit => this.Ipv4FragmentFlags != -1 && (this.Pv4FragmentOffset&1) != 0;

        //TODO: Vesely - Return also unspecified value
        /// <summary>
        ///     Retrieves IPv4 fragmentation MF bit
        /// </summary>
        public Boolean Ipv4MFbit => this.Ipv4FragmentFlags != -1 && (this.Ipv4FragmentFlags&1) != 0;

        /// <summary>
        ///     In case of IPv4 packet it returns FragmentFlags field from IPv4 header
        /// </summary>
        public Int16 Ipv4FragmentFlags
        {
            get
            {
                //It is non-IPv4 for packet, hence fragmentation information are obsolete
                var tpacket = this.PacketInfo.Extract(typeof(IpPacket)) as IPv4Packet;
                return tpacket?.FragmentFlags ?? -1;
            }
        }
        #endregion

        #region IPv6 related information
        /// <summary>
        ///     Gets the IPv6 packet enbcapulated in the current CapPacket object.
        /// </summary>
        /// <value>The IPV6 Packet instance if current CapPacket contains this pacekt; otherwise null.</value>
        public IPv6Packet Pv6 => (IPv6Packet) this.PacketInfo.Extract(typeof(IPv6Packet));
        #endregion

        #endregion

        #region L4

        #region TCP/UDP common informations
        /// <summary>
        ///     Gets a source transport port if applicable.
        /// </summary>
        public UInt16 SourceTransportPort
        {
            get
            {
                switch(this.ProtocolIdentifier)
                {
                    case IPProtocolType.TCP:
                        var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                        return tcp?.SourcePort ?? 0;
                    case IPProtocolType.UDP:
                        var udp = (UdpPacket) this.PacketInfo.Extract(typeof(UdpPacket));
                        return udp?.SourcePort ?? 0;
                }
                return 0;
            }
        }

        /// <summary>
        ///     Gets a destination transport port if applicable.
        /// </summary>
        public UInt16 DestinationTransportPort
        {
            get
            {
                switch(this.ProtocolIdentifier)
                {
                    case IPProtocolType.TCP:
                        var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                        return tcp?.DestinationPort ?? 0;
                    case IPProtocolType.UDP:
                        var udp = (UdpPacket) this.PacketInfo.Extract(typeof(UdpPacket));
                        return udp?.DestinationPort ?? 0;
                    default:
                        return 0;
                }
            }
        }
        #endregion

        #region TCP related information
        /// <summary>
        ///     Gets the TCP packet enbcapulated in the current CapPacket object.
        /// </summary>
        /// <value>The TCPPacket instance if current CapPacket contains this pacekt; otherwise null.</value>
        public TcpPacket TCP => (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));

        #region TCPFlags
        /// <summary>
        ///     Gets a TCP flags.
        /// </summary>
        public Byte TcpFlags
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return tcp?.AllFlags ?? 0;
            }
        }

        //TODO: Vesely - For all TCP flags return also unspecified value
        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFAck
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.Ack;
            }
        }

        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFCwr
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.CWR;
            }
        }

        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFEce
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.ECN;
            }
        }

        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFFin
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.Fin;
            }
        }

        /// <summary>
        ///     Gets TCP NS flag
        /// </summary>
        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFPsh
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.Psh;
            }
        }

        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFRst
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.Rst;
            }
        }

        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFSyn
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.Syn;
            }
        }

        /// <summary>
        ///     Gets TCP ACK flag
        /// </summary>
        public Boolean TcpFUrg
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.Urg;
            }
        }
        #endregion

        /// <summary>
        ///     Gets a TCP sequence number field.
        /// </summary>
        public UInt32 TcpSequenceNumber
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return tcp?.SequenceNumber ?? 0;
            }
        }

        /// <summary>
        ///     Gets a TCP acknowledgment number field.
        /// </summary>
        public UInt32 TcpAcknowledgmentNumber
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return tcp?.AcknowledgmentNumber ?? 0;
            }
        }

        /// <summary>
        ///     Gets a TCP sequence number field.
        /// </summary>
        public UInt32 SequenceNumber
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return tcp?.SequenceNumber ?? 0;
            }
        }

        /// <summary>
        ///     Gets a TCP acknowledgment number field.
        /// </summary>
        public UInt32 AcknowledgmentNumber
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return tcp?.AcknowledgmentNumber ?? 0;
            }
        }

        /// <summary>
        ///     Gets a TCP checksum
        /// </summary>
        public Boolean IsValidChecksum
        {
            get
            {
                var tcp = (TcpPacket) this.PacketInfo.Extract(typeof(TcpPacket));
                return (tcp != null) && tcp.CalculateTCPChecksum() == tcp.Checksum;
            }
        }
        #endregion

        #region UDP related information
        /// <summary>
        ///     Gets the UDP packet enbcapulated in the current CapPacket object.
        /// </summary>
        /// <value>The UDPPacket instance if current CapPacket contains this pacekt; otherwise null.</value>
        public UdpPacket Udp => (UdpPacket) this.PacketInfo.Extract(typeof(UdpPacket));
        #endregion

        #endregion
    }
}