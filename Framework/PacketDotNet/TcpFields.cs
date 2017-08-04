namespace PacketDotNet
{
    /// <summary>
    ///     IP protocol field encoding information.
    /// </summary>
    public struct TcpFields
    {
        /// <summary> Length of a TCP port in bytes.</summary>
        public static readonly int PortLength = 2;

        /// <summary> Length of the sequence number in bytes.</summary>
        public static readonly int SequenceNumberLength = 4;

        /// <summary> Length of the acknowledgment number in bytes.</summary>
        public static readonly int AckNumberLength = 4;

        /// <summary> Length of the data offset and flags field in bytes.</summary>
        public static readonly int DataOffsetLength = 1;

        /// <summary> The length of the flags field </summary>
        public static readonly int FlagsLength = 1;

        /// <summary> Length of the window size field in bytes.</summary>
        public static readonly int WindowSizeLength = 2;

        /// <summary> Length of the checksum field in bytes.</summary>
        public static readonly int ChecksumLength = 2;

        /// <summary> Length of the urgent field in bytes.</summary>
        public static readonly int UrgentPointerLength = 2;

        /// <summary> Position of the source port field.</summary>
        public static readonly int SourcePortPosition = 0;

        /// <summary> Position of the destination port field.</summary>
        public static readonly int DestinationPortPosition;

        /// <summary> Position of the sequence number field.</summary>
        public static readonly int SequenceNumberPosition;

        /// <summary> Position of the acknowledgment number field.</summary>
        public static readonly int AckNumberPosition;

        /// <summary> Position of the data offset </summary>
        public static readonly int DataOffsetPosition;

        /// <summary> Position of the flags field </summary>
        public static readonly int FlagsPosition;

        /// <summary> Position of the window size field.</summary>
        public static readonly int WindowSizePosition;

        /// <summary> Position of the checksum field.</summary>
        public static readonly int ChecksumPosition;

        /// <summary> Position of the urgent pointer field.</summary>
        public static readonly int UrgentPointerPosition;

        /// <summary> Length in bytes of a TCP header.</summary>
        public static readonly int HeaderLength; // == 20

        static TcpFields()
        {
            DestinationPortPosition = PortLength;
            SequenceNumberPosition = DestinationPortPosition + PortLength;
            AckNumberPosition = SequenceNumberPosition + SequenceNumberLength;
            DataOffsetPosition = AckNumberPosition + AckNumberLength;
            FlagsPosition = DataOffsetPosition + DataOffsetLength;
            WindowSizePosition = FlagsPosition + FlagsLength;
            ChecksumPosition = WindowSizePosition + WindowSizeLength;
            UrgentPointerPosition = ChecksumPosition + ChecksumLength;
            HeaderLength = UrgentPointerPosition + UrgentPointerLength;
        }
#pragma warning disable 1591
        // flag bitmasks
        public static readonly int TCP_CWR_MASK = 0x0080;
        public static readonly int TCP_ECN_MASK = 0x0040;
        public static readonly int TCP_URG_MASK = 0x0020;
        public static readonly int TCP_ACK_MASK = 0x0010;
        public static readonly int TCP_PSH_MASK = 0x0008;
        public static readonly int TCP_RST_MASK = 0x0004;
        public static readonly int TCP_SYN_MASK = 0x0002;
        public static readonly int TCP_FIN_MASK = 0x0001;
#pragma warning restore 1591
    }
}