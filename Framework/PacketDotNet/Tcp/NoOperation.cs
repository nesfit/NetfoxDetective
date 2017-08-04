namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     No Operation Option
    ///     Used in the TCP Options field to pad the length to the next 32 byte boundary
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc793/
    /// </remarks>
    public class NoOperation : Option
    {
        #region Members
        /// <summary>
        ///     The length (in bytes) of the NoOperation option
        /// </summary>
        internal const int OptionLength = 1;
        #endregion

        #region Constructors
        /// <summary>
        ///     Creates a No Operation Option
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
        public NoOperation(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     The length of the NoOperation field
        ///     Returns 1 as opposed to returning the length field because
        ///     the NoOperation option is only 1 byte long and doesn't
        ///     contain a length field
        /// </summary>
        public override byte Length
        {
            get { return OptionLength; }
        }
        #endregion
    }
}