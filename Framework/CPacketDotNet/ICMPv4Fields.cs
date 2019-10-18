namespace PacketDotNet
{
    /// <summary>
    ///     ICMP protocol field encoding information.
    ///     See http://en.wikipedia.org/wiki/ICMPv6
    /// </summary>
    public class ICMPv4Fields
    {
        /// <summary> Length of the ICMP message type code in bytes.</summary>
        public static readonly int TypeCodeLength = 2;

        /// <summary> Length of the ICMP header checksum in bytes.</summary>
        public static readonly int ChecksumLength = 2;

        /// <summary> Length of the ICMP ID field in bytes.</summary>
        public static readonly int IDLength = 2;

        /// <summary> Length of the ICMP Sequence field in bytes </summary>
        public static readonly int SequenceLength = 2;

        /// <summary> Position of the ICMP message type/code.</summary>
        public static readonly int TypeCodePosition;

        /// <summary> Position of the ICMP header checksum.</summary>
        public static readonly int ChecksumPosition;

        /// <summary> Position of the ICMP ID field </summary>
        public static readonly int IDPosition;

        /// <summary> Position of the Sequence field </summary>
        public static readonly int SequencePosition;

        /// <summary> Length in bytes of an ICMP header.</summary>
        public static readonly int HeaderLength;

        static ICMPv4Fields()
        {
            TypeCodePosition = 0;
            ChecksumPosition = TypeCodePosition + TypeCodeLength;
            IDPosition = ChecksumPosition + ChecksumLength;
            SequencePosition = IDPosition + IDLength;
            HeaderLength = SequencePosition + SequenceLength;
        }
    }
}