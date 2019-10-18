using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     An IGMP packet.
    /// </summary>
    [Serializable]
    public class IGMPv2Packet : InternetPacket
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public IGMPv2Packet(ByteArraySegment bas)
        {
            // set the header field, header field values are retrieved from this byte array
            this.header = new ByteArraySegment(bas);
            this.header.Length = UdpFields.HeaderLength;

            // store the payload bytes
            this.payloadPacketOrData = new PacketOrByteArraySegment();
            this.payloadPacketOrData.TheByteArraySegment = this.header.EncapsulatedBytes();
        }

        /// <summary>
        ///     Constructor with parent
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        /// <param name="ParentPacket">
        ///     A <see cref="Packet" />
        /// </param>
        public IGMPv2Packet(ByteArraySegment bas, Packet ParentPacket) : this(bas)
        {
            this.ParentPacket = ParentPacket;
        }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color
        {
            get { return AnsiEscapeSequences.Brown; }
        }

        /// <value>
        ///     The type of IGMP message
        /// </value>
        public virtual IGMPMessageType Type
        {
            get { return (IGMPMessageType) this.header.Bytes[this.header.Offset + IGMPv2Fields.TypePosition]; }

            set { this.header.Bytes[this.header.Offset + IGMPv2Fields.TypePosition] = (byte) value; }
        }

        /// <summary> Fetch the IGMP max response time.</summary>
        public virtual byte MaxResponseTime
        {
            get { return this.header.Bytes[this.header.Offset + IGMPv2Fields.MaxResponseTimePosition]; }

            set { this.header.Bytes[this.header.Offset + IGMPv2Fields.MaxResponseTimePosition] = value; }
        }

        /// <summary> Fetch the IGMP header checksum.</summary>
        public virtual short Checksum
        {
            get { return BitConverter.ToInt16(this.header.Bytes, this.header.Offset + IGMPv2Fields.ChecksumPosition); }

            set
            {
                var theValue = BitConverter.GetBytes(value);
                Array.Copy(theValue, 0, this.header.Bytes, (this.header.Offset + IGMPv2Fields.ChecksumPosition), 2);
            }
        }

        /// <summary> Fetch the IGMP group address.</summary>
        public virtual IPAddress GroupAddress
        {
            get { return IpPacket.GetIPAddress(AddressFamily.InterNetwork, this.header.Offset + IGMPv2Fields.GroupAddressPosition, this.header.Bytes); }
        }

        /// <summary>
        ///     Returns the encapsulated IGMPv2Packet of the Packet p or null if
        ///     there is no encapsulated packet
        /// </summary>
        /// <param name="p">
        ///     A <see cref="Packet" />
        /// </param>
        /// <returns>
        ///     A <see cref="IGMPv2Packet" />
        /// </returns>
        [Obsolete("Use Packet.Extract() instead")]
        public static IGMPv2Packet GetEncapsulated(Packet p)
        {
            if(p is InternetLinkLayerPacket)
            {
                var payload = InternetLinkLayerPacket.GetInnerPayload((InternetLinkLayerPacket) p);
                if(payload is IpPacket)
                {
                    var innerPayload = payload.PayloadPacket;
                    if(innerPayload is IGMPv2Packet) { return (IGMPv2Packet) innerPayload; }
                }
            }

            return null;
        }

        /// <summary cref="Packet.ToString(StringOutputType)" />
        public override string ToString(StringOutputType outputFormat)
        {
            var buffer = new StringBuilder();
            var color = "";
            var colorEscape = "";

            if(outputFormat == StringOutputType.Colored || outputFormat == StringOutputType.VerboseColored)
            {
                color = this.Color;
                colorEscape = AnsiEscapeSequences.Reset;
            }

            if(outputFormat == StringOutputType.Normal || outputFormat == StringOutputType.Colored)
            {
                // build the output string
                buffer.AppendFormat("{0}[IGMPv2Packet: Type={2}, MaxResponseTime={3}, GroupAddress={4}]{1}", color, colorEscape, this.Type,
                    String.Format("{0:0.0}", (this.MaxResponseTime/10)), this.GroupAddress);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                properties.Add("type", this.Type + " (0x" + this.Type.ToString("x") + ")");
                properties.Add("max response time", String.Format("{0:0.0}", this.MaxResponseTime/10) + " sec (0x" + this.MaxResponseTime.ToString("x") + ")");
                // TODO: Implement checksum validation for IGMPv2
                properties.Add("header checksum", "0x" + this.Checksum.ToString("x"));
                properties.Add("group address", this.GroupAddress.ToString());

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("IGMP:  ******* IGMPv2 - \"Internet Group Management Protocol (Version 2)\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("IGMP:");
                foreach(var property in properties) { buffer.AppendLine("IGMP: " + property.Key.PadLeft(padLength) + " = " + property.Value); }
                buffer.AppendLine("IGMP:");
            }

            // append the base string output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }
    }
}