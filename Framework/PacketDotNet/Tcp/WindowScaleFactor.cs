namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     Window Scale Factor Option
    ///     Expands the definition of the TCP window to 32 bits
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc1323/
    /// </remarks>
    public class WindowScaleFactor : Option
    {
        #region Members

        // the offset (in bytes) of the ScaleFactor Field
        private const int ScaleFactorFieldOffset = 2;
        #endregion

        #region Constructors
        /// <summary>
        ///     Creates a Window Scale Factor Option
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
        public WindowScaleFactor(byte[] bytes, int offset, int length) : base(bytes, offset, length) { }
        #endregion

        #region Properties
        /// <summary>
        ///     The Window Scale Factor
        ///     used as a multiplier to the window value
        ///     The multiplier is equal to 1 left-shifted by the ScaleFactor
        ///     So a scale factor of 7 would equal 1 &lt;&lt; 7 = 128
        /// </summary>
        public byte ScaleFactor
        {
            get { return this.Bytes[ScaleFactorFieldOffset]; }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Returns the Option info as a string
        ///     The multiplier is equal to a value of 1 left-shifted by the scale factor
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return "[" + this.Kind + ": ScaleFactor=" + this.ScaleFactor + " (multiply by " + (1 << this.ScaleFactor) + ")]"; }
        #endregion
    }
}