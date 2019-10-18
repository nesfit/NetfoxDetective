using System;
using PacketDotNet.Utils;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     A TCP Option
    /// </summary>
    public abstract class Option
    {
        #region Constructors
        /// <summary>
        ///     Creates an Option from a byte[]
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
        public Option(byte[] bytes, int offset, int length) { this.optionData = new ByteArraySegment(bytes, offset, length); }
        #endregion

        #region Methods
        /// <summary>
        ///     Returns the Option info as a string
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return "[" + this.Kind + "]"; }
        #endregion

        #region Properties
        /// <summary>
        ///     The Length of the Option type
        /// </summary>
        public virtual byte Length
        {
            get { return this.Bytes[LengthFieldOffset]; }
        }

        /// <summary>
        ///     The Kind of option
        /// </summary>
        public OptionTypes Kind
        {
            get { return (OptionTypes) this.Bytes[KindFieldOffset]; }
        }

        /// <summary>
        ///     Returns a TLV that contains the Option
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                var bytes = new byte[this.optionData.Length];
                Array.Copy(this.optionData.Bytes, this.optionData.Offset, bytes, 0, this.optionData.Length);
                return bytes;
            }
        }
        #endregion

        #region Members

        // stores the data/length/offset of the option
        private ByteArraySegment optionData;

        /// <summary>The length (in bytes) of the Kind field</summary>
        internal const int KindFieldLength = 1;

        /// <summary>The length (in bytes) of the Length field</summary>
        internal const int LengthFieldLength = 1;

        /// <summary>The offset (in bytes) of the Kind Field</summary>
        internal const int KindFieldOffset = 0;

        /// <summary>The offset (in bytes) of the Length field</summary>
        internal const int LengthFieldOffset = 1;
        #endregion
    }
}