namespace PacketDotNet.Tcp
{
    /// <summary>
    ///     Specifies the different types of algorithms that the
    ///     Alternative Checksum option are allowed to use
    /// </summary>
    /// <remarks>
    ///     References:
    ///     http://datatracker.ietf.org/doc/rfc1146/
    /// </remarks>
    public enum ChecksumAlgorighmType
    {
        /// <summary>Standard TCP Checksum Algorithm</summary>
        TCPChecksum = 0,

        /// <summary>8-bit Fletchers Algorighm</summary>
        EightBitFletchersAlgorithm = 1,

        /// <summary>16-bit Fletchers Algorithm</summary>
        SixteenBitFletchersAlgorithm = 2,

        /// <summary>Redundant Checksum Avoidance</summary>
        RedundantChecksumAvoidance = 3
    }
}