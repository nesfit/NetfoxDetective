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
    ///     Point to Point Protocol
    ///     See http://tools.ietf.org/html/rfc2516
    /// </summary>
    public class PPPoEPacket : Packet
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

        private byte VersionType
        {
            get { return this.header.Bytes[this.header.Offset + PPPoEFields.VersionTypePosition]; }

            set { this.header.Bytes[this.header.Offset + PPPoEFields.VersionTypePosition] = value; }
        }

        /// <summary>
        ///     PPPoe version, must be 0x1 according to RFC
        /// </summary>
        /// FIXME: This currently outputs the wrong version number
        public byte Version
        {
            get { return (byte) ((this.VersionType >> 4)&0xF0); }

            set
            {
                var versionType = this.VersionType;

                // mask the new value in
                versionType = (byte) ((versionType&0x0F)|((value << 4)&0xF0));

                this.VersionType = versionType;
            }
        }

        /// <summary>
        ///     Type, must be 0x1 according to RFC
        /// </summary>
        public byte Type
        {
            get { return (byte) ((this.VersionType)&0x0F); }

            set
            {
                var versionType = this.VersionType;

                // mask the new value in
                versionType = (byte) ((versionType&0xF0)|(value&0xF0));

                this.VersionType = versionType;
            }
        }

        /// <summary>
        /// </summary>
        /// FIXME: This currently outputs the wrong code
        public PPPoECode Code
        {
            get { return (PPPoECode) EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + PPPoEFields.CodePosition); }

            set
            {
                var val = (UInt16) value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + PPPoEFields.CodePosition);
            }
        }

        /// <summary>
        ///     Session identifier for this PPPoe packet
        /// </summary>
        public UInt16 SessionId
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + PPPoEFields.SessionIdPosition); }

            set
            {
                var val = value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + PPPoEFields.SessionIdPosition);
            }
        }

        /// <summary>
        ///     Length of the PPPoe payload, not including the PPPoe header
        /// </summary>
        public UInt16 Length
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + PPPoEFields.LengthPosition); }

            set
            {
                var val = value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + PPPoEFields.LengthPosition);
            }
        }

        /// <summary>
        ///     Construct a new PPPoEPacket from source and destination mac addresses
        /// </summary>
        public PPPoEPacket(PPPoECode Code, UInt16 SessionId)
        {
            log.Debug("");

            // allocate memory for this packet
            var offset = 0;
            var length = PPPoEFields.HeaderLength;
            var headerBytes = new byte[length];
            this.header = new ByteArraySegment(headerBytes, offset, length);

            // set the instance values
            this.Code = Code;
            this.SessionId = SessionId;

            // setup some typical values and default values
            this.Version = 1;
            this.Type = 1;
            this.Length = 0;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public PPPoEPacket(ByteArraySegment bas)
        {
            log.Debug("");

            // slice off the header portion
            this.header = new ByteArraySegment(bas);
            this.header.Length = PPPoEFields.HeaderLength;

            // parse the encapsulated bytes
            this.payloadPacketOrData = ParseEncapsulatedBytes(this.header);
        }

        internal static PacketOrByteArraySegment ParseEncapsulatedBytes(ByteArraySegment Header)
        {
            // slice off the payload
            var payload = Header.EncapsulatedBytes();
            log.DebugFormat("payload {0}", payload);

            var payloadPacketOrData = new PacketOrByteArraySegment();

            // we assume that we have a PPPPacket as the payload
            payloadPacketOrData.ThePacket = new PPPPacket(payload);

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
                buffer.AppendFormat("{0}[PPPoEPacket: Version={2}, Type={3}, Code={4}, SessionId={5}, Length={6}]{1}", color, colorEscape, this.Version, this.Type, this.Code,
                    this.SessionId, this.Length);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                // FIXME: The version output is incorrect
                properties.Add("", Convert.ToString(this.Version, 2).PadLeft(4, '0') + " .... = version: " + this.Version);
                properties.Add(" ", ".... " + Convert.ToString(this.Type, 2).PadLeft(4, '0') + " = type: " + this.Type);
                // FIXME: The Code output is incorrect
                properties.Add("code", this.Code + " (0x" + this.Code.ToString("x") + ")");
                properties.Add("session id", "0x" + this.SessionId.ToString("x"));
                // TODO: Implement a PayloadLength property for PPPoE
                //properties.Add("payload length", PayloadLength.ToString());

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("PPPoE:  ******* PPPoE - \"Point-to-Point Protocol over Ethernet\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("PPPoE:");
                foreach(var property in properties)
                {
                    if(property.Key.Trim() != "") {
                        buffer.AppendLine("PPPoE: " + property.Key.PadLeft(padLength) + " = " + property.Value);
                    }
                    else
                    {
                        buffer.AppendLine("PPPoE: " + property.Key.PadLeft(padLength) + "   " + property.Value);
                    }
                }
                buffer.AppendLine("PPPoE:");
            }

            // append the base output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the encapsulated PPPoE of the Packet p or null if
        ///     there is no encapsulated packet
        /// </summary>
        /// <param name="p">
        ///     A <see cref="Packet" />
        /// </param>
        /// <returns>
        ///     A <see cref="ARPPacket" />
        /// </returns>
        [Obsolete("Use Packet.Extract() instead")]
        public static PPPoEPacket GetEncapsulated(Packet p)
        {
            if(p is EthernetPacket) { if(p.PayloadPacket is PPPoEPacket) { return (PPPoEPacket) p.PayloadPacket; } }

            return null;
        }

        /// <summary>
        ///     Generate a random PPPoEPacket
        /// </summary>
        /// <returns>
        ///     A <see cref="PPPoEPacket" />
        /// </returns>
        public static PPPoEPacket RandomPacket()
        {
            throw new NotImplementedException();
        }
    }
}