namespace Netfox.Framework.Models.PmLib.SupportedTypes
{
    /// <summary>
    ///     Link types supproted by this app Base enum definde in IFrameworkIOController so values could
    ///     be exported out of framework without referencing PmLib.
    /// </summary>
    public enum PmLinkType
    {
        /// <summary>
        ///     IEEE802.3
        /// </summary>
        Ethernet,

        /// <summary>
        /// </summary>
        Fddi,

        /// <summary>
        /// </summary>
        Raw,

        /// <summary>
        /// </summary>
        Ieee80211,

        /// <summary>
        /// </summary>
        AtmRfc1483,

        /// <summary>
        /// </summary>
        Null
    }
}