namespace PacketDotNet
{
    /// <summary>
    ///     The available types of strings that the ToString(StringOutputType) can handle.
    /// </summary>
    public enum StringOutputType
    {
        /// <summary>
        ///     Outputs the packet info on a single line
        /// </summary>
        Normal,

        /// <summary>
        ///     Outputs the packet info on a single line with coloring
        /// </summary>
        Colored,

        /// <summary>
        ///     Outputs the detailed packet info
        /// </summary>
        Verbose,

        /// <summary>
        ///     Outputs the detailed packet info with coloring
        /// </summary>
        VerboseColored
    }
}