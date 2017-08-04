namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     End-of-Options Option
    ///     Marks the end of the Options list
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc793/
    /// </remarks>
    public class EndOfOptions : Option
    {
        #region Members
        /// <summary>
        ///     The length (in bytes) of the EndOfOptions option
        /// </summary>
        internal const int OptionLength = 1;
        #endregion

        #region Constructors
        /// <summary>
        ///     Creates an End Of Options Option
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
        public EndOfOptions(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     The length of the EndOfOptions field
        ///     Returns 1 as opposed to returning the length field because
        ///     the EndOfOptions option is only 1 byte long and doesn't
        ///     contain a length field
        /// </summary>
        public override byte Length
        {
            get { return OptionLength; }
        }
        #endregion
    }
}