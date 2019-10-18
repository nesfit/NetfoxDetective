using System;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     An Echo Option
    ///     throws an exception because Echo Options
    ///     are obsolete as per their spec
    /// </summary>
    public class Echo : Option
    {
        #region Constructors
        /// <summary>
        ///     Creates an Echo Option
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
        public Echo(byte[] bytes, int offset, int length) : base(bytes, offset, length) { throw new NotSupportedException("Obsolete: The Echo Option has been deprecated."); }
        #endregion
    }
}