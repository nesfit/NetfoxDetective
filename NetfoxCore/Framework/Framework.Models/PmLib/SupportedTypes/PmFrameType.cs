namespace Netfox.Framework.Models.PmLib.SupportedTypes
{
    /// <summary>
    ///     More general frame type Base enum definde in IFrameworkIOController so values could be
    ///     exported out of framework without referencing PmLib.
    /// </summary>
    public enum PmFrameType
    {
        /// <summary>
        ///     Frame will be parsed as LibPCAP
        /// </summary>
        Pcap,

        /// <summary>
        ///     Frame will be parsed as PCAPng
        /// </summary>
        PcapNg,

        /// <summary>
        ///     Frame will be parsed as Microsoft Network Monitor
        /// </summary>
        Mnm,

        /// <summary>
        ///     Physically non-existing frame in PCAP file.
        /// </summary>
        /// <seealso cref="Netfox.Framework.Models.PmLib.Frames.PmFrameVirtual"/>
        Virtual,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content all zeros for DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirtualBlank,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content of predefined noise DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirutalNoise,

        /// <summary>
        ///     Physically non-existing frame in PCAP file. Frame encapsulated in one or more carrier datagrams, where carrier
        ///     datagrams can be either base band frames or encapsulation packets (GSE, GRE, etc).
        /// </summary>
        Encapsulated
    }
}