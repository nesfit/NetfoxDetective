namespace PacketDotNet
{
    /// <summary>
    ///     Indicates the protocol encapsulated by the PPP packet
    ///     See http://www.iana.org/assignments/ppp-numbers
    /// </summary>
    public enum PPPProtocol : ushort
    {
        /// <summary> Padding </summary>
        Padding = 0x1,

        /// <summary> IPv4 </summary>
        IPv4 = 0x21,

        /// <summary> IPv6 </summary>
        IPv6 = 0x57
    }
}