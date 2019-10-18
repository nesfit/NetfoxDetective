namespace PacketDotNet
{
    /// <summary> Code constants for ip ports. </summary>
    public enum IpPort : ushort
    {
#pragma warning disable 1591
        Echo = 7,
        DayTime = 13,
        FtpData = 20,
        Ftp = 21,

        /// <summary>
        ///     Secure shell
        /// </summary>
        Ssh = 22,

        /// <summary>
        ///     Terminal protocol
        /// </summary>
        Telnet = 23,

        /// <summary>
        ///     Simple mail transport protocol
        /// </summary>
        Smtp = 25,
        Time = 37,
        Whois = 63,
        Tftp = 69,
        Gopher = 70,
        Finger = 79,

        /// <summary>
        ///     Hyper text transfer protocol
        /// </summary>
        Http = 80,

        /// <summary>
        ///     Same as Http
        /// </summary>
        Www = 80,
        Kerberos = 88,
        Pop3 = 110,
        Ident = 113,
        Auth = 113,

        /// <summary>
        ///     Secure ftp
        /// </summary>
        Sftp = 115,

        /// <summary>
        ///     Network time protocol
        /// </summary>
        Ntp = 123,
        Imap = 143,

        /// <summary>
        ///     Simple network management protocol
        /// </summary>
        Snmp = 161,
        PrivilegedPortLimit = 1024
#pragma warning restore 1591
    }
}