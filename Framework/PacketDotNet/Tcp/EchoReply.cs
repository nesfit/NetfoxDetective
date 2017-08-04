using System;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     Echo Reply Option
    ///     Marked obsolete in the TCP spec Echo Reply Option has been
    ///     replaced by the TSOPT (Timestamp Option)
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc1072/
    /// </remarks>
    public class EchoReply : Option
    {
        #region Constructors
        /// <summary>
        ///     Creates an Echo Reply Option
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
        public EchoReply(byte[] bytes, int offset, int length) : base(bytes, offset, length) { throw new NotSupportedException("Obsolete: The Echo Option has been deprecated."); }
        #endregion
    }
}