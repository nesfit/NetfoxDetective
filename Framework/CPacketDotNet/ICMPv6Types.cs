namespace PacketDotNet
{
    /// <summary>
    ///     ICMPv6 types, see http://en.wikipedia.org/wiki/ICMPv6 and
    ///     http://www.iana.org/assignments/icmpv6-parameters
    /// </summary>
    public enum ICMPv6Types : byte
    {
#pragma warning disable 1591

        #region ICMPv6 Error Messages
        DestinationUnreachable = 1,
        PacketTooBig = 2,
        TimeExceeded = 3,
        ParameterProblem = 4,
        PrivateExperimentation1 = 100,
        PrivateExperimentation2 = 101,
        ReservedForExpansion1 = 127,
        #endregion

        #region ICMPv6 Informational Messages
        EchoRequest = 128,
        EchoReply = 129,
        RouterSolicitation = 133,
        NeighborSolicitation = 135,
        PrivateExperimentation3 = 200,
        PrivateExperimentation4 = 201,
        ReservedForExpansion2 = 255
        #endregion

#pragma warning restore 1591
    }
}