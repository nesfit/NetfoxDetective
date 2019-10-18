using PacketDotNet.MiscUtil.Conversion;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     Maximum Segment Size Option
    ///     An extension to the DataOffset/HeaderLength field to
    ///     allow sizes greater than 65,535
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc793/
    /// </remarks>
    public class MaximumSegmentSize : Option
    {
        #region Members

        // the offset (in bytes) of the Value Field
        private const int ValueFieldOffset = 2;
        #endregion

        #region Constructors
        /// <summary>
        ///     Creates a Maximum Segment Size Option
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
        public MaximumSegmentSize(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     The Maximum Segment Size
        /// </summary>
        public ushort Value
        {
            get { return EndianBitConverter.Big.ToUInt16(this.Bytes, ValueFieldOffset); }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Returns the Option info as a string
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return "[" + this.Kind + ": Value=" + this.Value + " bytes]"; }
        #endregion
    }
}