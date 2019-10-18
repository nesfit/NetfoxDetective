namespace PacketDotNet
{
    /// <summary>
    ///     The types of cooked packets
    ///     See http://github.com/mcr/libpcap/blob/master/pcap/sll.h
    /// </summary>
    public enum LinuxSLLType
    {
        /// <summary>
        ///     Packet was sent to us by somebody else
        /// </summary>
        PacketSentToUs = 0x0,

        /// <summary>
        ///     Packet was broadcast by somebody else
        /// </summary>
        PacketBroadCast = 0x1,

        /// <summary>
        ///     Packet was multicast, but not broadcast
        /// </summary>
        PacketMulticast = 0x2,

        /// <summary>
        ///     Packet was sent by somebody else to somebody else
        /// </summary>
        PacketSentToSomeoneElse = 0x3,

        /// <summary>
        ///     Packet was sent by us
        /// </summary>
        PacketSentByUs = 0x4
    }
}