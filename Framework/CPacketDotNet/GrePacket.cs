using System;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    public class GrePacket : Packet
    {
        public GrePacket(ByteArraySegment bas)
        {
            // set the header field, header field values are retrieved from this byte array
            this.header = new ByteArraySegment(bas)
            {
                Length = 4
            };

            //Temporary solution. Consider flags...

            // store the payload bytes
            this.payloadPacketOrData = new PacketOrByteArraySegment
            {
                TheByteArraySegment = this.header.EncapsulatedBytes()
            };
        }

        public GrePacket(ByteArraySegment bas, Packet ParentPacket) : this(bas) { this.ParentPacket = ParentPacket; }

        public bool IsChecksum => Convert.ToBoolean(this.header.Bytes[this.header.Offset]&GreFields.GRE_CHECKSUM_MASK);

        public bool IsKey => Convert.ToBoolean(this.header.Bytes[this.header.Offset]&GreFields.GRE_KEY_MASK);

        public bool IsSequence => Convert.ToBoolean(this.header.Bytes[this.header.Offset]&GreFields.GRE_SEQUENCE_MASK);

        public byte Version => Convert.ToByte(this.header.Bytes[this.header.Offset + GreFields.VersionByteOffset]&GreFields.GRE_VERSION_MASK);

        public Packet GetEncapsuledPacket()
        {
            var bas = new ByteArraySegment(this.header.Bytes, this.header.Offset + this.header.Length, 50);
            Packet packet = new IPv4Packet(bas);
            return packet;
        }
    }
}