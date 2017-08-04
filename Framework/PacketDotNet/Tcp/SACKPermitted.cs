namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     SACK (Selective Ack) Permitted Option
    ///     Notifies the receiver that SACK is allowed.
    ///     Must only be sent in a SYN segment
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc2018/
    /// </remarks>
    public class SACKPermitted : Option
    {
        #region Constructors
        /// <summary>
        ///     Creates a Sack Permitted Option
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
        public SACKPermitted(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion
    }
}