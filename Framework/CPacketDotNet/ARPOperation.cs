namespace PacketDotNet
{
    /// <summary>
    ///     The possible ARP operation values
    /// </summary>
    /// <remarks>
    ///     References:
    ///     - http://www.networksorcery.com/enp/default1101.htm
    /// </remarks>
    public enum ARPOperation : ushort
    {
        /// <summary>Request</summary>
        /// <remarks>See RFC 826, RFC 5227</remarks>
        Request = 1,

        /// <summary>Response</summary>
        /// <remarks>See RFC 826, RFC 1868, RFC 5227</remarks>
        Response = 2,

        /// <summary>Request Reverse</summary>
        /// <remarks>See RFC 903</remarks>
        RequestReverse = 3,

        /// <summary>Reply Reverse</summary>
        /// <remarks>See RFC 903</remarks>
        ReplyReverse = 4,

        /// <summary>DRARP Request</summary>
        /// <remarks>See RFC 1931</remarks>
        DRARPRequest = 5,

        /// <summary>DRARP Reply</summary>
        /// <remarks>See RFC 1931</remarks>
        DRARPReply = 6,

        /// <summary>DRARP Error</summary>
        /// <remarks>See RFC 1931</remarks>
        DRARPError = 7,

        /// <summary>InARP Request</summary>
        /// <remarks>See RFC 1293</remarks>
        InARPRequest = 8,

        /// <summary>InARP Reply</summary>
        /// <remarks>See RFC 1293</remarks>
        InARPReply = 9,

        /// <summary>ARP NAK</summary>
        /// <remarks>See RFC 1577</remarks>
        ARPNAK = 10,

        /// <summary>MARS Request</summary>
        MARSRequest = 11,

        /// <summary>MARS Multi</summary>
        MARSMulti = 12,

        /// <summary>MARS MServ</summary>
        MARSMServ = 13,

        /// <summary>MARS Join</summary>
        MARSJoin = 14,

        /// <summary>MARS Leave</summary>
        MARSLeave = 15,

        /// <summary>MARS NAK</summary>
        MARSNAK = 16,

        /// <summary>MARS Unserv</summary>
        MARSUnserv = 17,

        /// <summary>MARS SJoin</summary>
        MARSSJoin = 18,

        /// <summary>MARS SLeave</summary>
        MARSSLeave = 19,

        /// <summary>MARS Grouplist Request</summary>
        MARSGrouplistRequest = 20,

        /// <summary>MARS Grouplist Reply</summary>
        MARSGrouplistReply = 21,

        /// <summary>MARS Redirect Map</summary>
        MARSRedirectMap = 22,

        /// <summary>MARS UNARP</summary>
        /// <remarks>See RFC 2176</remarks>
        MaposUnarp = 23,

        /// <summary>OP_EXP1</summary>
        /// <remarks>See RFC 5494</remarks>
        OP_EXP1 = 24,

        /// <summary>OP_EXP2</summary>
        OP_EXP2 = 25
    }
}