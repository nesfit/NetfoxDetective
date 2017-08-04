namespace PacketDotNet
{
    /// <summary>
    ///     Ieee p8021 P priorities.
    ///     http://en.wikipedia.org/wiki/IEEE_802.1p
    /// </summary>
    public enum IeeeP8021PPriorities : byte
    {
        /// <summary>
        ///     Background
        /// </summary>
        Background_0 = 1,

        /// <summary>
        ///     Best effort
        /// </summary>
        BestEffort_1 = 0,

        /// <summary>
        ///     Excellent effort
        /// </summary>
        ExcellentEffort_2 = 2,

        /// <summary>
        ///     Critical application
        /// </summary>
        CriticalApplications_3 = 3,

        /// <summary>
        ///     Video, &lt; 100ms latency and jitter
        /// </summary>
        Video_4 = 4,

        /// <summary>
        ///     Voice, &lt; 10ms latency and jitter
        /// </summary>
        Voice_5 = 5,

        /// <summary>
        ///     Internetwork control
        /// </summary>
        InternetworkControl_6 = 6,

        /// <summary>
        ///     Network control
        /// </summary>
        NetworkControl_7 = 7
    }
}