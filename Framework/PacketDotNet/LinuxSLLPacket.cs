using System;
using System.Collections.Generic;
using System.Text;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     Represents a Linux cooked capture packet, the kinds of packets
    ///     received when capturing on an 'any' device
    ///     See http://github.com/mcr/libpcap/blob/master/pcap/sll.h
    /// </summary>
    public class LinuxSLLPacket : InternetLinkLayerPacket
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="bas">
        ///     A <see cref="ByteArraySegment" />
        /// </param>
        public LinuxSLLPacket(ByteArraySegment bas)
        {
            this.header = new ByteArraySegment(bas);
            this.header.Length = LinuxSLLFields.SLLHeaderLength;

            // parse the payload via an EthernetPacket method
            this.payloadPacketOrData = EthernetPacket.ParseEncapsulatedBytes(this.header, this.EthernetProtocolType);
        }

        /// <value>
        ///     Information about the packet direction
        /// </value>
        public LinuxSLLType Type
        {
            get { return (LinuxSLLType) EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.PacketTypePosition); }

            set
            {
                var theValue = (Int16) value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.PacketTypePosition);
            }
        }

        /// <value>
        ///     The
        /// </value>
        public int LinkLayerAddressType
        {
            get { return EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressTypePosition); }

            set
            {
                var theValue = (Int16) value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressTypePosition);
            }
        }

        /// <value>
        ///     Number of bytes in the link layer address of the sender of the packet
        /// </value>
        public int LinkLayerAddressLength
        {
            get { return EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressLengthPosition); }

            set
            {
                // range check
                if((value < 0) || (value > 8)) { throw new InvalidOperationException("value of " + value + " out of range of 0 to 8"); }

                var theValue = (Int16) value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressLengthPosition);
            }
        }

        /// <value>
        ///     Link layer header bytes, maximum of 8 bytes
        /// </value>
        public byte[] LinkLayerAddress
        {
            get
            {
                var headerLength = this.LinkLayerAddressLength;
                var theHeader = new Byte[headerLength];
                Array.Copy(this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressPosition, theHeader, 0, headerLength);
                return theHeader;
            }

            set
            {
                // update the link layer length
                this.LinkLayerAddressLength = value.Length;

                // copy in the new link layer header bytes
                Array.Copy(value, 0, this.header.Bytes, this.header.Offset + LinuxSLLFields.LinkLayerAddressPosition, value.Length);
            }
        }

        /// <value>
        ///     The encapsulated protocol type
        /// </value>
        public EthernetPacketType EthernetProtocolType
        {
            get { return (EthernetPacketType) EndianBitConverter.Big.ToInt16(this.header.Bytes, this.header.Offset + LinuxSLLFields.EthernetProtocolTypePosition); }

            set
            {
                var theValue = (Int16) value;
                EndianBitConverter.Big.CopyBytes(theValue, this.header.Bytes, this.header.Offset + LinuxSLLFields.EthernetProtocolTypePosition);
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
                buffer.AppendFormat("[{0}LinuxSLLPacket{1}: Type={2}, LinkLayerAddressType={3}, LinkLayerAddressLength={4}, Source={5}, ProtocolType={6}]", color, colorEscape,
                    this.Type, this.LinkLayerAddressType, this.LinkLayerAddressLength, BitConverter.ToString(this.LinkLayerAddress, 0), this.EthernetProtocolType);
            }

            if(outputFormat == StringOutputType.Verbose || outputFormat == StringOutputType.VerboseColored)
            {
                // collect the properties and their value
                var properties = new Dictionary<string, string>();
                properties.Add("type", this.Type + " (" + ((int) this.Type) + ")");
                properties.Add("link layer address type", this.LinkLayerAddressType.ToString());
                properties.Add("link layer address length", this.LinkLayerAddressLength.ToString());
                properties.Add("source", BitConverter.ToString(this.LinkLayerAddress));
                properties.Add("protocol", this.EthernetProtocolType + " (0x" + this.EthernetProtocolType.ToString("x") + ")");

                // calculate the padding needed to right-justify the property names
                var padLength = RandomUtils.LongestStringLength(new List<string>(properties.Keys));

                // build the output string
                buffer.AppendLine("LCC:  ******* LinuxSLL - \"Linux Cooked Capture\" - offset=? length=" + this.TotalPacketLength);
                buffer.AppendLine("LCC:");
                foreach(var property in properties) { buffer.AppendLine("LCC: " + property.Key.PadLeft(padLength) + " = " + property.Value); }
                buffer.AppendLine("LCC:");
            }

            // append the base output
            buffer.Append(base.ToString(outputFormat));

            return buffer.ToString();
        }
    }
}