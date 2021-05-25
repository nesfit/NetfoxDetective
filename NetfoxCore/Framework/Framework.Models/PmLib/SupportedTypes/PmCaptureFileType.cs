namespace Netfox.Framework.Models.PmLib.SupportedTypes
{
    /// <summary> CaptureProcessor file formats spported by this app.</summary>
    public enum PmCaptureFileType
    {
        /// <summary>
        ///     Expect version 2.3
        /// </summary>
        MicrosoftNetworkMonitor,

        /// <summary>
        ///     Expect version 2.4
        /// </summary>
        LibPcap,

        /// <summary>
        /// </summary>
        PcapNextGen,

        /// <summary>
        /// </summary>
        Unknown
    }
}