namespace PacketDotNet
{
    /// <summary>
    ///     Differentiates between a packet class payload, a byte[] payload
    ///     or no payload
    /// </summary>
    public enum PayloadType
    {
        /// <summary>
        ///     Constant packet.
        /// </summary>
        Packet,

        /// <summary>
        ///     Constant bytes.
        /// </summary>
        Bytes,

        /// <summary>
        ///     Constant none.
        /// </summary>
        None
    }
}