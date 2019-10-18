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
    ///     PPP packet
    ///     See http://en.wikipedia.org/wiki/Point-to-Point_Protocol
    /// </summary>
    public class PPPPacket : Packet
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

        /// <summary>
        ///     See http://www.iana.org/assignments/ppp-numbers
        /// </summary>
        public PPPProtocol Protocol
        {
            get { return (PPPProtocol) EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + PPPFields.ProtocolPosition); }

            set
            {
                var val = (UInt16) value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + PPPFields.ProtocolPosition);
            }
        }

        /// <summary>
        ///     Construct a new PPPPacket from source and destination mac addresses
        /// </summary>
        public PPPPacket(PPPoECode Code, UInt16 SessionId)
        {
            log.Debug("");

            // allocate memory for this packet
            var offset = 0;
            var length = PPPFields.HeaderLength;
            var headerBytes = new byte[length];
            this.header = new ByteArraySegment(headerBytes, offset, length);

            // setup some typical values and default values
            this.Protocol = PPPProtocol.Padding;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public PPPPacket(ByteArraySegment bas)
        {
            log.Debug("");

            // slice off the header portion as our header
            this.header = new ByteArraySegment(bas);
            this.header.Length = PPPFields.HeaderLength;

            // parse the encapsulated bytes
            this.payloadPacketOrData = ParseEncapsulatedBytes(this.header, this.Protocol);
        }

        internal static PacketOrByteArraySegment ParseEncapsulatedBytes(ByteArraySegment Header, PPPProtocol Protocol)
        {
            // slice off the payload
            var payload = Header.EncapsulatedBytes();

            log.DebugFormat("payload: {0}", payload);

            var payloadPacketOrData = new PacketOrByteArraySegment();

            switch(Protocol)
            {
                case PPPProtocol.IPv4:
                    payloadPacketOrData.ThePacket = new IPv4Packet(payload);
                    break;
                case PPPProtocol.IPv6:
                    payloadPacketOrData.ThePacket = new IPv6Packet(payload);
                    break;
                default:
                    throw new NotImplementedException("Protocol of " + Protocol + " is not implemented");
            }

            return payloadPacketOrData;
        }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color
        {
            get { return AnsiEscapeSequences.DarkGray; }
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
                buffer.AppendFormat("{0}[PPPPacket: Protocol={2}]{1}", color, colorEscape, this.Protocol);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                properties.Add("protocol", this.Protocol + " (0x" + this.Protocol.ToString("x") + ")");

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("PPP:  ******* PPP - \"Point-to-Point Protocol\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("PPP:");
                foreach(var property in properties) { buffer.AppendLine("PPP: " + property.Key.PadLeft(padLength) + " = " + property.Value); }
                buffer.AppendLine("PPP:");
            }

            // append the base output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the encapsulated PPPPacket of the Packet p or null if
        ///     there is no encapsulated packet
        /// </summary>
        /// <param name="p">
        ///     A <see cref="Packet" />
        /// </param>
        /// <returns>
        ///     A <see cref="PPPPacket" />
        /// </returns>
        [Obsolete("Use Packet.Extract() instead")]
        public static PPPPacket GetEncapsulated(Packet p)
        {
            if(p is EthernetPacket)
            {
                var payload = p.PayloadPacket;
                if(payload is PPPoEPacket) { return (PPPPacket) payload.PayloadPacket; }
            }

            return null;
        }

        /// <summary>
        ///     Generate a random PPPoEPacket
        /// </summary>
        /// <returns>
        ///     A <see cref="PPPoEPacket" />
        /// </returns>
        public static PPPoEPacket RandomPacket() { throw new NotImplementedException(); }
    }
}