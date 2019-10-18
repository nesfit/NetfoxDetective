using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     User datagram protocol
    ///     See http://en.wikipedia.org/wiki/Udp
    /// </summary>
    public class UdpPacket : TransportPacket
    {
#if DEBUG
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
#else
    // NOTE: No need to warn about lack of use, the compiler won't
    //       put any calls to 'log' here but we need 'log' to exist to compile
#pragma warning disable 0169, 0649
        private static readonly ILogInactive log;
#pragma warning restore 0169, 0649
#endif

        /// <summary> Fetch the port number on the source host.</summary>
        public virtual ushort SourcePort
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + UdpFields.SourcePortPosition); }

            set
            {
                var val = value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + UdpFields.SourcePortPosition);
            }
        }

        /// <summary> Fetch the port number on the target host.</summary>
        public virtual ushort DestinationPort
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + UdpFields.DestinationPortPosition); }

            set
            {
                var val = value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + UdpFields.DestinationPortPosition);
            }
        }

        /// <value>
        ///     Length in bytes of the header and payload, minimum size of 8,
        ///     the size of the Udp header
        /// </value>
        public virtual int Length
        {
            get { return EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + UdpFields.HeaderLengthPosition); }

            // Internal because it is updated based on the payload when
            // its bytes are retrieved
            internal set
            {
                var val = (Int16) value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + UdpFields.HeaderLengthPosition);
            }
        }

        /// <summary> Fetch the header checksum.</summary>
        public override ushort Checksum
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + UdpFields.ChecksumPosition); }

            set
            {
                var val = value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + UdpFields.ChecksumPosition);
            }
        }

        /// <summary> Check if the UDP packet is valid, checksum-wise.</summary>
        public bool ValidChecksum
        {
            get
            {
                // IPv6 has no checksum so only the TCP checksum needs evaluation
                if(this.ParentPacket.GetType() == typeof(IPv6Packet)) {
                    return this.ValidUDPChecksum;
                }
                    // For IPv4 both the IP layer and the TCP layer contain checksums
                return ((IPv4Packet) this.ParentPacket).ValidIPChecksum && this.ValidUDPChecksum;
            }
        }

        /// <value>
        ///     True if the udp checksum is valid
        /// </value>
        public virtual bool ValidUDPChecksum
        {
            get
            {
                log.Debug("ValidUDPChecksum");
                var retval = this.IsValidChecksum(TransportChecksumOption.AttachPseudoIPHeader);
                log.DebugFormat("ValidUDPChecksum {0}", retval);
                return retval;
            }
        }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color
        {
            get { return AnsiEscapeSequences.LightGreen; }
        }

        /// <summary>
        ///     Update the Udp length
        /// </summary>
        public override void UpdateCalculatedValues()
        {
            // update the length field based on the length of this packet header
            // plus the length of all of the packets it contains
            this.Length = this.TotalPacketLength;
        }

        /// <summary>
        ///     Create from values
        /// </summary>
        /// <param name="SourcePort">
        ///     A <see cref="System.UInt16" />
        /// </param>
        /// <param name="DestinationPort">
        ///     A <see cref="System.UInt16" />
        /// </param>
        public UdpPacket(ushort SourcePort, ushort DestinationPort)
        {
            log.Debug("");

            // allocate memory for this packet
            var offset = 0;
            var length = UdpFields.HeaderLength;
            var headerBytes = new byte[length];
            this.header = new ByteArraySegment(headerBytes, offset, length);

            // set instance values
            this.SourcePort = SourcePort;
            this.DestinationPort = DestinationPort;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public UdpPacket(ByteArraySegment bas)
        {
            log.DebugFormat("bas {0}", bas);

            // set the header field, header field values are retrieved from this byte array
            this.header = new ByteArraySegment(bas);
            this.header.Length = UdpFields.HeaderLength;

            this.payloadPacketOrData = new PacketOrByteArraySegment();

            // is this packet going to port 7 or 9? if so it might be a WakeOnLan packet
            const int wakeOnLanPort0 = 7;
            const int wakeOnLanPort1 = 9;
            if(this.DestinationPort.Equals(wakeOnLanPort0) || this.DestinationPort.Equals(wakeOnLanPort1)) {
                this.payloadPacketOrData.ThePacket = new WakeOnLanPacket(this.header.EncapsulatedBytes());
            }
            else
            {
                // store the payload bytes
                this.payloadPacketOrData.TheByteArraySegment = this.header.EncapsulatedBytes();
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        /// <param name="ParentPacket">
        ///     A <see cref="Packet" />
        /// </param>
        public UdpPacket(ByteArraySegment bas, Packet ParentPacket) : this(bas)
        {
            this.ParentPacket = ParentPacket;
        }

        /// <summary>
        ///     Calculates the UDP checksum, optionally updating the UDP checksum header.
        /// </summary>
        /// <returns>The calculated UDP checksum.</returns>
        public int CalculateUDPChecksum()
        {
            var newChecksum = this.CalculateChecksum(TransportChecksumOption.AttachPseudoIPHeader);
            return newChecksum;
        }

        /// <summary>
        ///     Update the checksum value.
        /// </summary>
        public void UpdateUDPChecksum()
        {
            this.Checksum = (ushort) this.CalculateUDPChecksum();
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

            if(outputFormat == StringOutputType.Normal || outputFormat == StringOutputType.Colored) {
                buffer.AppendFormat("{0}[UDPPacket: SourcePort={2}, DestinationPort={3}]{1}", color, colorEscape, this.SourcePort, this.DestinationPort);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                properties.Add("source", this.SourcePort.ToString());
                properties.Add("destination", this.DestinationPort.ToString());
                properties.Add("length", this.Length.ToString());
                properties.Add("checksum", "0x" + this.Checksum.ToString("x") + " [" + (this.ValidUDPChecksum? "valid" : "invalid") + "]");

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("UDP:  ******* UDP - \"User Datagram Protocol\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("UDP:");
                foreach(var property in properties) { buffer.AppendLine("UDP: " + property.Key.PadLeft(padLength) + " = " + property.Value); }
                buffer.AppendLine("UDP:");
            }

            // append the base string output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the UdpPacket inside of the Packet p or null if
        ///     there is no encapsulated packet
        /// </summary>
        /// <param name="p">
        ///     A <see cref="Packet" />
        /// </param>
        /// <returns>
        ///     A <see cref="UdpPacket" />
        /// </returns>
        [Obsolete("Use Packet.Extract() instead")]
        public static UdpPacket GetEncapsulated(Packet p)
        {
            if(p is InternetLinkLayerPacket)
            {
                var payload = InternetLinkLayerPacket.GetInnerPayload((InternetLinkLayerPacket) p);
                if(payload is IpPacket)
                {
                    var innerPayload = payload.PayloadPacket;
                    if(innerPayload is UdpPacket) { return (UdpPacket) innerPayload; }
                }
            }

            return null;
        }

        /// <summary>
        ///     Generate a random packet
        /// </summary>
        /// <returns>
        ///     A <see cref="UdpPacket" />
        /// </returns>
        public static UdpPacket RandomPacket()
        {
            var rnd = new Random();
            var SourcePort = (ushort) rnd.Next(ushort.MinValue, ushort.MaxValue);
            var DestinationPort = (ushort) rnd.Next(ushort.MinValue, ushort.MaxValue);

            return new UdpPacket(SourcePort, DestinationPort);
        }
    }
}