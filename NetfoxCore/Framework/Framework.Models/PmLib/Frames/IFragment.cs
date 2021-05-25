namespace Netfox.Framework.Models.PmLib.Frames
{
    /// <summary>
    /// Fragment of a fragmented datagram. 
    /// <para><b>Current implementation uses copied data instead of offsets in capture file.</b></para>
    /// </summary>
    public interface IFragment
    {
        /// <summary>
        /// Offset of payload in this fragment in capture file, measured in bytes.
        /// </summary>
        //long PayloadOffset { get; } // NOTE: Current implementation uses copied data instead of offsets in capture file.

        /// <summary>
        /// Length of payload in this fragment, measured in bytes.
        /// </summary>
        //long PayloadLength { get; } // NOTE: Current implementation uses copied data instead of offsets in capture file.

        byte[] Payload { get; } // NOTE: Current implementation uses copied data instead of offsets in capture file.

        PmFrameBase LastFrame { get; }
    }
}