using System.IO;
using PacketDotNet.Utils;

namespace PacketDotNet
{
    /// <summary>
    ///     Encapsulates and ensures that we have either a Packet OR
    ///     a ByteArraySegment but not both
    /// </summary>
    public class PacketOrByteArraySegment
    {
        private ByteArraySegment theByteArraySegment;
        private Packet thePacket;

        /// <summary>
        ///     Gets or sets the byte array segment.
        /// </summary>
        /// <value>
        ///     The byte array segment.
        /// </value>
        public ByteArraySegment TheByteArraySegment
        {
            get { return this.theByteArraySegment; }

            set
            {
                this.thePacket = null;
                this.theByteArraySegment = value;
            }
        }

        /// <summary>
        ///     Gets or sets the packet.
        /// </summary>
        /// <value>
        ///     The packet.
        /// </value>
        public Packet ThePacket
        {
            get { return this.thePacket; }

            set
            {
                this.theByteArraySegment = null;
                this.thePacket = value;
            }
        }

        /// <value>
        ///     Whether or not this container contains a packet, a byte[] or neither
        /// </value>
        public PayloadType Type
        {
            get
            {
                if(this.ThePacket != null) { return PayloadType.Packet; }
                if(this.TheByteArraySegment != null) { return PayloadType.Bytes; }
                return PayloadType.None;
            }
        }

        /// <summary>
        ///     Appends to the MemoryStream either the byte[] represented by TheByteArray, or
        ///     if ThePacket is non-null, the Packet.Bytes will be appended to the memory stream
        ///     which will append ThePacket's header and any encapsulated packets it contains
        /// </summary>
        /// <param name="ms">
        ///     A <see cref="MemoryStream" />
        /// </param>
        public void AppendToMemoryStream(MemoryStream ms)
        {
            if(this.ThePacket != null)
            {
                var theBytes = this.ThePacket.Bytes;
                ms.Write(theBytes, 0, theBytes.Length);
            }
            else if(this.TheByteArraySegment != null)
            {
                var theBytes = this.TheByteArraySegment.ActualBytes();
                ms.Write(theBytes, 0, theBytes.Length);
            }
        }
    }
}