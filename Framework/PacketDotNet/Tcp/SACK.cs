using PacketDotNet.MiscUtil.Conversion;

namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     SACK (Selective Ack) Option
    ///     Provides a means for a receiver to notify the sender about
    ///     all the segments that have arrived successfully.
    ///     Used to cut down on the number of unnecessary re-transmissions.
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc2018/
    ///     http://datatracker.ietf.org/doc/rfc2883/
    /// </remarks>
    public class SACK : Option
    {
        #region Constructors
        /// <summary>
        ///     Creates a SACK (Selective Ack) Option
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
        public SACK(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     Contains an array of SACK (Selective Ack) Blocks
        /// </summary>
        public ushort[] SACKBlocks
        {
            get
            {
                var numOfBlocks = (this.Length - SACKBlocksFieldOffset)/BlockLength;
                var blocks = new ushort[numOfBlocks];
                var offset = 0;
                for(var i = 0; i < numOfBlocks; i++)
                {
                    offset = SACKBlocksFieldOffset + (i*BlockLength);
                    blocks[i] = EndianBitConverter.Big.ToUInt16(this.Bytes, offset);
                }
                return blocks;
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
        public override string ToString()
        {
            var output = "[" + this.Kind + ": ";

            for(var i = 0; i < this.SACKBlocks.Length; i++) { output += "Block" + i + "=" + this.SACKBlocks[i] + " "; }

            output.TrimEnd();
            output += "]";

            return output;
        }
        #endregion

        #region Members

        // the length (in bytes) of a SACK block
        private const int BlockLength = 2;

        // the offset (in bytes) of the ScaleFactor Field
        private const int SACKBlocksFieldOffset = 2;
        #endregion
    }
}