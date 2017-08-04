namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     AlternateChecksumRequest Option
    /// </summary>
    public class AlternateChecksumRequest : Option
    {
        #region Members

        // the offset (in bytes) of the Checksum field
        private const int ChecksumFieldOffset = 2;
        #endregion

        #region Constructors
        /// <summary>
        ///     Creates an Alternate Checksum Request Option
        ///     Used to negotiate an alternative checksum algorithm in a connection
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
        /// <remarks>
        ///     References:
        ///     http://datatracker.ietf.org/doc/rfc1146/
        /// </remarks>
        public AlternateChecksumRequest(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     The Checksum
        /// </summary>
        public ChecksumAlgorighmType Checksum
        {
            get { return (ChecksumAlgorighmType) this.Bytes[ChecksumFieldOffset]; }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Returns the Option info as a string
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return "[" + this.Kind + ": ChecksumType=" + this.Checksum + "]"; }
        #endregion
    }
}