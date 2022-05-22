using System;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet.Utils;
using PostSharp.Patterns.Contracts;

namespace Netfox.Framework.CaptureProcessor.L7PDUTracking.DVB
{
    /// <summary>
    /// Generic Stream Encapsulation (GSE) packet
    /// <para><b>Current implementation uses copied data instead of offsets in capture file.</b></para>
    /// </summary>
    /// <remarks>
    /// <para>For complete GSE Packet structure, see
    /// <a href="http://www.etsi.org/deliver/etsi_ts/102600_102699/10260601/01.02.01_60/ts_10260601v010201p.pdf">
    /// ETSI TS 102 606-1 V1.2.1.</a></para>
    /// </remarks>
    class GsePacket
        : IFragment
    {
        public readonly int? Crc32;

        public readonly GseHeader Header;

        public readonly byte[] PDU; // NOTE: Current implementation uses copied data instead of offsets in capture file.

        /// <summary>GSE Packet type based on combination of <see cref="GseHeader"/> fields.</summary>
        public readonly PacketType Type;

        /// <summary>Checks whether this GSE packet is a GSE fragment packet.</summary>
        public bool IsGseFragment =>
            this.Type == PacketType.Start || this.Type == PacketType.Intermediate || this.Type == PacketType.End;

        public GsePacket(
            [Required] GseHeader header,
            [Required] byte[] pdu,
            int? crc32,
            PacketType type,
            [Required] PmFrameBase lastFrame)
        {
            this.Header = header;
            this.PDU = pdu;
            this.Crc32 = crc32;
            this.Type = type;
            this.LastFrame = lastFrame;
        }

        /// <summary>
        /// Factory method parsing <see cref="GsePacket"/>.
        /// </summary>
        /// <param name="reader"><see cref="PDUStreamReader"/> for stream from which the <see cref="GsePacket"/>
        /// should be parsed.</param>
        /// <exception cref="InvalidPacketFormatException">If read data does not correspond to valid format.</exception>
        /// <returns>Parsed <see cref="GsePacket"/> object.</returns>
        public static GsePacket Parse(PDUStreamReader reader)
        {
            // Header
            var h = GseHeader.Parse(reader);

            // Type
            PacketType type;
            if (!h.StartIndicator && !h.EndIndicator && h.LabelTypeIndicator == LabelType.Byte6)
            {
                type = PacketType.Padding;
            }
            else if (!h.StartIndicator && !h.EndIndicator && h.LabelTypeIndicator == LabelType.LabelReuse)
            {
                type = PacketType.Intermediate;
            }
            else if (h.StartIndicator && !h.EndIndicator)
            {
                type = PacketType.Start;
            }
            else if (!h.StartIndicator && h.EndIndicator && h.LabelTypeIndicator == LabelType.LabelReuse)
            {
                type = PacketType.End;
            }
            else if (h.StartIndicator && h.EndIndicator)
            {
                type = PacketType.Complete;
            }
            else
            {
                throw new InvalidPacketFormatException(
                    $"Invalid packet type. StartIndicator={h.StartIndicator}, EndIndicator={h.EndIndicator}, LabelTypeIndicator={h.LabelTypeIndicator}.");
            }

            // PDU
            // Check whether this GSE packet isn't a padding packet
            byte[] pdu = null;
            if (type != PacketType.Padding)
            {
                if (h.GSELength == null)
                {
                    throw new InvalidPacketFormatException("GSELength field is missing.");
                }

                if (h.BytesAfterGseLengthField == null)
                {
                    throw new InvalidPacketFormatException("BytesAfterGseLengthField unknown.");
                }

                var pduLength = (ushort) h.GSELength - (ushort) h.BytesAfterGseLengthField;
                if (!h.StartIndicator && h.EndIndicator) // if CRC is present after PDU
                {
                    pduLength -= 4; // CRC is 4 bytes long
                }

                pdu = new byte[pduLength];
                var readLength = reader.Read(pdu, 0, pduLength);
                if (readLength != pduLength)
                {
                    throw new InvalidPacketFormatException(
                        $"Reader could not read whole PDU. Read {readLength} out of {pduLength} bytes.");
                }
            }

            // CRC-32
            int? crc32 = null;
            if (!h.StartIndicator && h.EndIndicator)
            {
                crc32 = reader.ReadInt32();
                // CRC-32 can't be verified here, because we need all GSE packets from this fragmentation group.
            }

            return new GsePacket(h, pdu, crc32, type, reader.PDUStreamBasedProvider.GetCurrentPDU().FrameList.Last());
        }

        #region Implementation of IFragment

        public byte[] Payload => this.PDU;
        public PmFrameBase LastFrame { get; }

        #endregion
    }

    /// <summary>
    /// GSE Packet types based on combination of <see cref="GseHeader"/> fields. Other combinations shall not
    /// be used.
    /// </summary>
    internal enum PacketType
    {
        /// <summary>
        /// <see cref="GseHeader"/> fields combination:
        /// <list type="bullet">
        /// <item><see cref="GseHeader.StartIndicator"/> == <c>false</c></item>
        /// <item><see cref="GseHeader.EndIndicator"/> == <c>false</c></item>
        /// <item><see cref="GseHeader.LabelTypeIndicator"/> == <see cref="LabelType.Byte6"/></item> 
        /// </list>
        /// </summary>
        Padding,

        /// <summary>
        /// <see cref="GseHeader"/> fields combination:
        /// <list type="bullet">
        /// <item><see cref="GseHeader.StartIndicator"/> == <c>false</c></item>
        /// <item><see cref="GseHeader.EndIndicator"/> == <c>false</c></item>
        /// <item><see cref="GseHeader.LabelTypeIndicator"/> == <see cref="LabelType.LabelReuse"/></item> 
        /// </list>
        /// </summary>
        Intermediate,

        /// <summary>
        /// <see cref="GseHeader"/> fields combination:
        /// <list type="bullet">
        /// <item><see cref="GseHeader.StartIndicator"/> == <c>true</c></item>
        /// <item><see cref="GseHeader.EndIndicator"/> == <c>false</c></item>
        /// <item><see cref="GseHeader.LabelTypeIndicator"/> == <see cref="LabelType"/> (any)</item> 
        /// </list>
        /// </summary>
        Start,

        /// <summary>
        /// <see cref="GseHeader"/> fields combination:
        /// <list type="bullet">
        /// <item><see cref="GseHeader.StartIndicator"/> == <c>false</c></item>
        /// <item><see cref="GseHeader.EndIndicator"/> == <c>true</c></item>
        /// <item><see cref="GseHeader.LabelTypeIndicator"/> == <see cref="LabelType.LabelReuse"/></item> 
        /// </list>
        /// </summary>
        End,

        /// <summary>
        /// <see cref="GseHeader"/> fields combination:
        /// <list type="bullet">
        /// <item><see cref="GseHeader.StartIndicator"/> == <c>true</c></item>
        /// <item><see cref="GseHeader.EndIndicator"/> == <c>true</c></item>
        /// <item><see cref="GseHeader.LabelTypeIndicator"/> == <see cref="LabelType"/> (any)</item> 
        /// </list>
        /// </summary>
        Complete
    }

    internal class GseHeader
    {
        public const byte HeaderLenghtMinimum = 2;

        /// <summary>
        /// Indicates how many bytes of <see cref="GseHeader"/> have been read after the <see cref="GSELength"/> field.
        /// This is used to determine a number of GSE data bytes.
        /// </summary>
        public readonly ushort? BytesAfterGseLengthField;

        /// <summary>
        /// Indicates whether this GSE Packet contains the end of the encapsulated PDU. For padding, this shall be 
        /// set to <c>false</c>.
        /// </summary>
        public readonly bool EndIndicator;

        /// <summary>
        /// All GSE Packets containing PDU fragments belonging to the same PDU shall contain the same
        /// <see cref="FragmentID"/>. The selected <see cref="FragmentID"/> shall not be re-used on the link until
        /// the last fragment of the PDU has been transmitted.
        /// <para>This is present (<b>not null</b>) when a PDU fragment is included in the GSE Packet, while it is
        /// not present (<b>null</b>) if <see cref="StartIndicator"/> and <see cref="EndIndicator"/> are both set
        /// to <c>true</c>.</para>
        /// </summary>
        public readonly byte? FragmentID;

        /// <summary>
        /// This 12-bit field indicates the number of bytes following in this GSE Packet, counted from the byte
        /// following this GSE Length field. The GSE Length field allows for a length of up to 4 096 bytes for
        /// a GSE Packet. The GSE Length field points to the start of the following GSE Packet. If the GSE packet
        /// is the last in the Base Band frame it points to the end of the Base Band frame Data Field or the start
        /// of the padding field. For End packets, it also covers the CRC_32 field.
        /// <para>This field is not present (<b>null</b>) only if <see cref="PacketType"/> == <see cref="PacketType.Padding"/>.</para>
        /// </summary>
        /// <seealso cref="BytesAfterGseLengthField"/>
        public readonly ushort? GSELength;

        /// <summary>
        /// This contains 6 byte label, 3 byte label or <c>null</c>.
        /// </summary>
        public readonly byte[] Label;

        /// <summary>
        /// For Start and Complete GSE packets, it indicates the type of label field in use. For Intermediate and
        /// End GSE packets, it shall be set to <c>0b11</c> (<see cref="LabelType.LabelReuse"/>). For Padding GSE
        /// packets, it shall be set to <c>0b00</c> (<see cref="LabelType.Byte6"/>).
        /// </summary>
        public readonly LabelType LabelTypeIndicator;

        /// <summary>
        /// This 16-bit field indicates the type of payload carried in the PDU, or the presence of a Next-Header.
        /// <para>This field is present (<b>not null</b>) only if <see cref="StartIndicator"/> == <c>true</c></para>.
        /// </summary>
        /// <remarks>
        /// The set of values that may be assigned to this field is divided into two ranges, similar to
        /// the allocation of Ethernet. The two ranges are:
        /// <list type="bullet">
        /// <item><term>Type 1: Next-Header Type field</term>
        /// <description>
        /// The first range of the Type space corresponds to the range of values 0 to 1535 decimal. These values
        /// may be used to identify link-specific protocols and/or to indicate the presence of Extension Headers
        /// that carry additional optional protocol fields (e.g.a bridging encapsulation). The range is sub-divided
        /// into values less than 256 and greater than 256, depending on the type of extension. The use of these
        /// values is co-ordinated by an IANA registry. 
        /// </description></item>  
        /// <item><term>Type 2: EtherType compatible Type Fields</term>
        /// <description>
        /// The second range of the Type space corresponds to the values between 0x600 (1536 decimal) and 0xFFFF.
        /// This set of type assignments follow DIX/IEEE assignments (but exclude use of this field as a frame
        /// length indicator). All assignments in this space shall use the values defined for EtherType,
        /// the following two Type values are used as examples (taken from the IEEE EtherTypes registry):
        /// <example>
        /// <c>0x0800</c>: IPv4 payload<br/>
        /// <c>0x86DD</c>: IPv6 payload
        /// </example>
        /// </description></item>  
        /// </list>
        /// </remarks>
        /// <seealso cref="PacketDotNet.EthernetPacketType"/>
        public readonly ushort? ProtocolType;

        /// <summary>
        /// Indicates whether this GSE Packet contains the start of the encapsulated PDU. For padding, this shall
        /// be set to <c>false</c>.
        /// </summary>
        public readonly bool StartIndicator;

        /// <summary>
        /// The 16-bit field carries the value of the total length, defined as the length, in bytes, of
        /// the Protocol Type, Label (6 byte Label or 3 byte Label), Extension Headers, and the full PDU.
        /// <para>This field is present (<b>not null</b>) in the GSE Header carrying the first fragment of
        /// a fragmented PDU.</para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Although the length of a single GSE Packet is limited to almost 4 096 bytes, larger PDUs are supported
        /// through fragmentation, up to a total length of 65 536 bytes.
        /// </para>
        /// <para>
        /// Since the information in the total length field is intended for use by higher layers in the
        /// reassembly process, the length of the CRC_32 field is therefore not included in
        /// the <see cref="TotalLength"/>.
        /// </para>
        /// </remarks>
        public readonly ushort? TotalLength;

        public GseHeader(
            bool startIndicator,
            bool endIndicator,
            LabelType labelTypeIndicator,
            ushort? gseLength,
            ushort? bytesAfterGseLengthField,
            byte? fragmentID,
            ushort? totalLength,
            ushort? protocolType,
            byte[] label)
        {
            this.StartIndicator = startIndicator;
            this.EndIndicator = endIndicator;
            this.LabelTypeIndicator = labelTypeIndicator;
            this.GSELength = gseLength;
            this.BytesAfterGseLengthField = bytesAfterGseLengthField;
            this.FragmentID = fragmentID;
            this.TotalLength = totalLength;
            this.ProtocolType = protocolType;
            this.Label = label;
        }

        /// <summary>
        /// Factory method parsing <see cref="GseHeader"/>.
        /// </summary>
        /// <param name="reader"><see cref="PDUStreamReader"/> for stream from which the <see cref="GseHeader"/>
        /// should be parsed.</param>
        /// <exception cref="InvalidPacketFormatException">If read data does not correspond to valid format.</exception>
        /// <returns>Parsed <see cref="GseHeader"/> object.</returns>
        /// <remarks>
        /// For complete syntax of GSE Packet structure, see Table 2 in 
        /// <a href="http://www.etsi.org/deliver/etsi_ts/102600_102699/10260601/01.02.01_60/ts_10260601v010201p.pdf#page=13">
        /// ETSI TS 102 606-1 V1.2.1.</a>
        /// </remarks>
        public static GseHeader Parse(PDUStreamReader reader)
        {
            var b = reader.ReadByte();

            // Byte 0-1, Fixed Header Fields
            // Bit 15: S
            var startIndicator = (b & 0b1000_0000) == 0b1000_0000;
            // Bit 14: E
            var endIndicator = (b & 0b0100_0000) == 0b0100_0000;
            // Bit 13-12: LT
            // LabelType enum has 4 values, therefore we use Enum.Parse instead of Enum.TryParse.
            var labelTypeIndicator = (LabelType) Enum.Parse(typeof(LabelType), ((b & 0b0011_0000) >> 4).ToString());

            // possibly null values which might not be present in packet
            ushort? gseLength = null;
            ushort? bytesAfterGseLengthField = null;
            byte? fragmentID = null;
            ushort? totalLength = null;
            ushort? protocolType = null;
            byte[] label = null;

            long? gseLengthFieldStreamPosition = null;

            // Bit 11-0
            if (!startIndicator && !endIndicator && labelTypeIndicator == LabelType.Byte6)
            {
                // 4 padding bits
                if ((b & 0b0000_1111) != 0b0000_0000)
                {
                    throw new InvalidPacketFormatException($"Expected 4 zero-padding bits.");
                }

                // N1 Padding bytes (N1 is the number of bytes until the end of the Base-Band frame.)
                while (!reader.EndOfPDU)
                {
                    if (reader.ReadByte() != 0)
                    {
                        throw new InvalidPacketFormatException(
                            $"Expected zero-padding bits until the end of the Base-Band frame.");
                    }
                }
            }
            else
            {
                // 12 bits
                byte[] tmpMaskedBytes =
                {
                    reader.ReadByte(),
                    (byte) (b & 0b0000_1111)
                }; // BigEndian
                gseLength = BitConverter.ToUInt16(tmpMaskedBytes, 0);
                gseLengthFieldStreamPosition = reader.PDUStreamBasedProvider.Position;

                if (!startIndicator || !endIndicator)
                {
                    fragmentID = reader.ReadByte();
                }

                if (startIndicator && !endIndicator)
                {
                    totalLength = reader.ReadUInt16();
                }

                if (startIndicator)
                {
                    protocolType = reader.ReadUInt16();
                    if (labelTypeIndicator == LabelType.Byte6)
                    {
                        label = new byte[6];
                        if (reader.Read(label, 0, 6) != 6)
                        {
                            throw new InvalidPacketFormatException("Reader could not read 6 bytes.");
                        }
                    }
                    else if (labelTypeIndicator == LabelType.Byte3)
                    {
                        label = new byte[3];
                        if (reader.Read(label, 0, 3) != 3)
                        {
                            throw new InvalidPacketFormatException("Reader could not read 3 bytes.");
                        }
                    }
                }
            }

            if (gseLengthFieldStreamPosition != null)
            {
                bytesAfterGseLengthField =
                    (ushort) (reader.PDUStreamBasedProvider.Position - gseLengthFieldStreamPosition);
            }

            return new GseHeader(startIndicator, endIndicator, labelTypeIndicator, gseLength, bytesAfterGseLengthField,
                fragmentID, totalLength, protocolType, label);
        }
    }

    internal enum LabelType
    {
        /// <summary>Indicates that a 6 byte Label is present and shall be used for filtering.</summary>
        Byte6 = 0b00,

        /// <summary>Indicates that a 3 byte Label is present and shall be used for filtering.</summary>
        Byte3 = 0b01,

        /// <summary>
        /// Broadcast. No label field is present. All receivers shall process this GSE Packet. This combination
        /// shall be used also in non broadcast systems when no filtering is applied at Layer 2, but IP header
        /// processing is utilized.
        /// </summary>
        Broadcast = 0b10,

        /// <summary>
        /// Label re-use. No label field is present. All receivers shall reuse the label that was present in
        /// the previous Start or Complete GSE Packet of the same Base Band frame. This method is used for 
        /// transmitting a sequence of GSE Packets with the same label without repeating the label field. This
        /// value shall not be used for the first GSE Packet in the frame.
        /// </summary>
        LabelReuse = 0b11
    }
}