using System;
using System.Collections.Generic;
using System.Text;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     802.1Q vlan packet
    ///     http://en.wikipedia.org/wiki/IEEE_802.1Q
    /// </summary>
    [Serializable]
    public class Ieee8021QPacket : InternetPacket
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public Ieee8021QPacket(ByteArraySegment bas)
        {
            // set the header field, header field values are retrieved from this byte array
            this.header = new ByteArraySegment(bas);
            this.header.Length = Ieee8021QFields.HeaderLength;

            // parse the payload via an EthernetPacket method
            this.payloadPacketOrData = EthernetPacket.ParseEncapsulatedBytes(this.header, this.Type);
        }

        /// <summary> Fetch ascii escape sequence of the color associated with this packet type.</summary>
        public override String Color
        {
            get { return AnsiEscapeSequences.LightCyan; }
        }

        /// <value>
        ///     Type of packet that this vlan packet encapsulates
        /// </value>
        public virtual EthernetPacketType Type
        {
            get { return (EthernetPacketType) EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + Ieee8021QFields.TypePosition); }

            set
            {
                var val = (Int16) value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + Ieee8021QFields.TypePosition);
            }
        }

        /// <summary>
        ///     Gets or sets the priority control point.
        /// </summary>
        /// <value>
        ///     The priority control point.
        /// </value>
        public IeeeP8021PPriorities PriorityControlPoint
        {
            get
            {
                var tci = this.TagControlInformation;
                tci >>= (16 - 3); // priority is the upper 3 bits
                return (IeeeP8021PPriorities) tci;
            }

            set
            {
                var tci = this.TagControlInformation;

                // mask the existing Priority off and then back in from value
                var val = (ushort) value;
                tci = (ushort) ((tci&0x1FFF)|((val&0x7) << (16 - 3)));
                this.TagControlInformation = tci;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance canonical format indicator.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the mac address is in non-canonical format <c>false</c> if otherwise.
        /// </value>
        public bool CanonicalFormatIndicator
        {
            get
            {
                var tci = this.TagControlInformation;
                tci >>= 12;
                return ((tci&0x1) == 1)? true : false;
            }

            set
            {
                var tci = this.TagControlInformation;

                // mask the existing CFI off and then back in from value
                var val = (value? 1 : 0);
                tci = (ushort) ((tci&0xEFFF)|(val << 12));
                this.TagControlInformation = tci;
            }
        }

        /// <summary>
        ///     Gets or sets the VLAN identifier.
        /// </summary>
        /// <value>
        ///     The VLAN identifier.
        /// </value>
        public ushort VLANIdentifier
        {
            get
            {
                var tci = this.TagControlInformation;
                return (ushort) (tci&0xFFF);
            }

            set
            {
                var tci = this.TagControlInformation;

                // mask the existing vlan id off
                tci = (ushort) ((tci&0xF000)|(value&0xFFF));
                this.TagControlInformation = tci;
            }
        }

        private ushort TagControlInformation
        {
            get { return (ushort) EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + Ieee8021QFields.TagControlInformationPosition); }

            set
            {
                var val = (Int16) value;
                EndianBitConverter.Big.CopyBytes(val, this.header.Bytes, this.header.Offset + Ieee8021QFields.TagControlInformationPosition);
            }
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
                buffer.AppendFormat("{0}[Ieee8021QPacket: PriorityControlPoint={2}, CanonicalFormatIndicator={3}, Type={4}]{1}", color, colorEscape, this.PriorityControlPoint,
                    this.CanonicalFormatIndicator, this.Type);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                properties.Add("priority", this.PriorityControlPoint + " (0x" + this.PriorityControlPoint.ToString("x") + ")");
                properties.Add("canonical format indicator", this.CanonicalFormatIndicator.ToString());
                properties.Add("type", this.Type + " (0x" + this.Type.ToString("x") + ")");
                properties.Add("VLANIdentifier", this.VLANIdentifier + " (0x" + this.VLANIdentifier.ToString("x") + ")");

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("Ieee802.1Q:  ******* Ieee802.1Q - \"VLan tag\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("Ieee802.1Q:");
                foreach(var property in properties) { buffer.AppendLine("Ieee802.1Q: " + property.Key.PadLeft(padLength) + " = " + property.Value); }
                buffer.AppendLine("Ieee802.1Q:");
            }

            // append the base string output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }
    }
}