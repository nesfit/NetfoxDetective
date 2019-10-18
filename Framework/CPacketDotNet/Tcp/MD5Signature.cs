using System;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     MD5 Signature
    ///     Carries the MD5 Digest used by the BGP protocol to
    ///     ensure security between two endpoints
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc2385/
    /// </remarks>
    public class MD5Signature : Option
    {
        #region Members

        // the offset (in bytes) of the MD5 Digest field
        private const int MD5DigestFieldOffset = 2;
        #endregion

        #region Constructors
        /// <summary>
        ///     Creates a MD5 Signature Option
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
        public MD5Signature(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     The MD5 Digest
        /// </summary>
        public byte[] MD5Digest
        {
            get
            {
                var data = new byte[this.Length - MD5DigestFieldOffset];
                Array.Copy(this.Bytes, MD5DigestFieldOffset, data, 0, data.Length);
                return data;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Returns the Option info as a string
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return "[" + this.Kind + ": MD5Digest=0x" + this.MD5Digest + "]"; }
        #endregion
    }
}