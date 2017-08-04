namespace PacketDotNet
{
    /// <summary>
    ///     Code constants for IGMP message types.
    ///     From RFC #2236.
    /// </summary>
    public enum IGMPMessageType : byte
    {
#pragma warning disable 1591
        MembershipQuery = 0x11,
        MembershipReportIGMPv1 = 0x12,
        MembershipReportIGMPv2 = 0x16,
        MembershipReportIGMPv3 = 0x22,
        LeaveGroup = 0x17
#pragma warning restore 1591
    }
}