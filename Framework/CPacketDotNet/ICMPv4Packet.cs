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
    ///     An ICMP packet
    ///     See http://en.wikipedia.org/wiki/Internet_Control_Message_Protocol
    /// </summary>
    [Serializable]
    public class ICMPv4Packet : InternetPacket
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

        /// <value>
        ///     The Type/Code enum value
        /// </value>
        public virtual ICMPv4TypeCodes TypeCode
        {
            get
            {
                var val = EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + ICMPv4Fields.TypeCodePosition);

                //TODO: how to handle a mismatch in the mapping? maybe throw here?
                if(Enum.IsDefined(typeof(ICMPv4TypeCodes), val)) { return (ICMPv4TypeCodes) val; }
                throw new NotImplementedException("TypeCode of " + val + " is not defined in ICMPv4TypeCode");
            }

            set
            {
                var theValue = (UInt16) value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + ICMPv4Fields.TypeCodePosition);
            }
        }

        /// <value>
        ///     Checksum value
        /// </value>
        public ushort Checksum
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + ICMPv4Fields.ChecksumPosition); }

            set
            {
                var theValue = value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + ICMPv4Fields.ChecksumPosition);
            }
        }

        /// <summary>
        ///     ID field
        /// </summary>
        public ushort ID
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + ICMPv4Fields.IDPosition); }

            set
            {
                var theValue = value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + ICMPv4Fields.IDPosition);
            }
        }

        /// <summary>
        ///     Sequence field
        /// </summary>
        public ushort Sequence
        {
            get { return EndianBitConverter.Big.ToUInt16(this.header.Bytes, this.header.Offset + ICMPv4Fields.SequencePosition); }

            set { EndianBitConverter.Big.CopyBytes(value, this.header.Bytes, this.header.Offset + ICMPv4Fields.SequencePosition); }
        }

        /// <summary>
        ///     Contents of the ICMP packet
        /// </summary>
        public byte[] Data
        {
            get { return this.payloadPacketOrData.TheByteArraySegment.ActualBytes(); }

            set { this.payloadPacketOrData.TheByteArraySegment = new ByteArraySegment(value, 0, value.Length); }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public ICMPv4Packet(ByteArraySegment bas)
        {
            log.Debug("");

            this.header = new ByteArraySegment(bas);
            this.header.Length = ICMPv4Fields.HeaderLength;

            // store the payload bytes
            this.payloadPacketOrData = new PacketOrByteArraySegment();
            this.payloadPacketOrData.TheByteArraySegment = this.header.EncapsulatedBytes();
        }

        /// <summary>
        ///     Construct with parent packet
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        /// <param name="ParentPacket">
        ///     A <see cref="Packet" />
        /// </param>
        public ICMPv4Packet(ByteArraySegment bas, Packet ParentPacket) : this(bas) { this.ParentPacket = ParentPacket; }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color
        {
            get { return AnsiEscapeSequences.LightBlue; }
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
                buffer.AppendFormat("{0}[ICMPPacket: TypeCode={2}]{1}", color, colorEscape, this.TypeCode);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                properties.Add("type/code", this.TypeCode + " (0x" + this.TypeCode.ToString("x") + ")");
                // TODO: Implement checksum verification for ICMPv4
                properties.Add("checksum", this.Checksum.ToString("x"));
                properties.Add("identifier", "0x" + this.ID.ToString("x"));
                properties.Add("sequence number", this.Sequence + " (0x" + this.Sequence.ToString("x") + ")");

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("ICMP:  ******* ICMPv4 - \"Internet Control Message Protocol (Version 4)\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("ICMP:");
                foreach(var property in properties) { buffer.AppendLine("ICMP: " + property.Key.PadLeft(padLength) + " = " + property.Value); }
                buffer.AppendLine("ICMP:");
            }

            // append the base string output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the ICMPv4Packet inside of Packet p or null if
        ///     there is no encapsulated ICMPv4Packet
        /// </summary>
        /// <param name="p">
        ///     A <see cref="Packet" />
        /// </param>
        /// <returns>
        ///     A <see cref="ICMPv4Packet" />
        /// </returns>
        [Obsolete("Use Packet.Extract() instead")]
        public static ICMPv4Packet GetEncapsulated(Packet p)
        {
            log.Debug("");

            if(p is InternetLinkLayerPacket)
            {
                var payload = InternetLinkLayerPacket.GetInnerPayload((InternetLinkLayerPacket) p);
                if(payload is IpPacket)
                {
                    var payload2 = payload.PayloadPacket;
                    if(payload2 is ICMPv4Packet) { return (ICMPv4Packet) payload2; }
                }
            }

            return null;
        }
    }
}