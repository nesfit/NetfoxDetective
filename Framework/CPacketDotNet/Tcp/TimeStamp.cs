using PacketDotNet.MiscUtil.Conversion;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     A Time Stamp Option
    ///     Used for RTTM (Round Trip Time Measurement)
    ///     and PAWS (Protect Against Wrapped Sequences)
    ///     Opsoletes the Echo and EchoReply option fields
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc1323/
    /// </remarks>
    public class TimeStamp : Option
    {
        #region Constructors
        /// <summary>
        ///     Creates a Timestamp Option
        /// </summary>
        /// <param name="bytes">
        ///     A <see cref="System.Byte[]" />
        /// </param>
        /// <param name="offset">
        ///     A <see cref="System.Int32" />
        /// </param>
        /// <param name="length">
        ///     A <see cref="System.Int32" />
        /// </param>
        public TimeStamp(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Methods
        /// <summary>
        ///     Returns the Option info as a string
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return "[" + this.Kind + ": Value=" + this.Value + " EchoReply=" + this.EchoReply + "]"; }
        #endregion

        #region Properties
        /// <summary>
        ///     The Timestamp value
        /// </summary>
        public uint Value
        {
            get { return EndianBitConverter.Big.ToUInt32(this.Bytes, ValueFieldOffset); }
        }

        /// <summary>
        ///     The Echo Reply
        /// </summary>
        public uint EchoReply
        {
            get { return EndianBitConverter.Big.ToUInt32(this.Bytes, EchoReplyFieldOffset); }
        }
        #endregion

        #region Members

        // the offset (in bytes) of the Value Field
        private const int ValueFieldOffset = 2;

        // the offset (in bytes) of the Echo Reply Field
        private const int EchoReplyFieldOffset = 6;
        #endregion
    }
}