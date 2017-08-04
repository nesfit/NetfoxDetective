using PacketDotNet.Utils;

namespace PacketDotNet.LLDP
{
    /// <summary>
    ///     An End Of LLDPDU TLV
    /// </summary>
    public class EndOfLLDPDU : TLV
    {
        #region Constructors
        /// <summary>
        ///     Parses bytes into an End Of LLDPDU TLV
        /// </summary>
        /// <param name="bytes">
        ///     TLV bytes
        /// </param>
        /// <param name="offset">
        ///     The End Of LLDPDU TLV's offset from the
        ///     origin of the LLDP
        /// </param>
        public EndOfLLDPDU(byte[] bytes, int offset) : base(bytes, offset)
        {
            this.Type = 0;
            this.Length = 0;
        }

        /// <summary>
        ///     Creates an End Of LLDPDU TLV
        /// </summary>
        public EndOfLLDPDU()
        {
            var bytes = new byte[TLVTypeLength.TypeLengthLength];
            var offset = 0;
            var length = bytes.Length;
            this.tlvData = new ByteArraySegment(bytes, offset, length);

            this.Type = 0;
            this.Length = 0;
        }

        /// <summary>
        ///     Convert this TTL TLV to a string.
        /// </summary>
        /// <returns>
        ///     A human readable string
        /// </returns>
        public override string ToString() { return string.Format("[EndOfLLDPDU]"); }
        #endregion
    }
}