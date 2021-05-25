namespace Netfox.Framework.Models.Enums
{
    /// <summary> More general frame type.</summary>
    public enum EFcFrameType
    {
        /// <summary>
        ///     Frame will be parsed as LibPCAP
        /// </summary>
        Pcap = 1,

        /// <summary>
        ///     Frame will be parsed as PCAPng
        /// </summary>
        PcapNg = 2,

        /// <summary>
        ///     Frame will be parsed as Microsoft Network Monitor
        /// </summary>
        Mnm = 3,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content all zeros for DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirtualBlank = 4,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content of predefined noise DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirutalNoise = 5
    }
}