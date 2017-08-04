namespace PacketDotNet
{
    /// <summary>
    ///     A struct containing length and position information about IPv6 Fields.
    /// </summary>
    public struct IPv6Fields
    {
        /// <summary>
        ///     The IP Version, Traffic Class, and Flow Label field length. These must be in one
        ///     field due to boundary crossings.
        /// </summary>
        public static readonly int VersionTrafficClassFlowLabelLength = 4;

        /// <summary>
        ///     The payload length field length.
        /// </summary>
        public static readonly int PayloadLengthLength = 2;

        /// <summary>
        ///     The next header field length, identifies protocol encapsulated by the packet
        /// </summary>
        public static readonly int NextHeaderLength = 1;

        /// <summary>
        ///     The hop limit field length.
        /// </summary>
        public static readonly int HopLimitLength = 1;

        /// <summary>
        ///     Address field length
        /// </summary>
        public static readonly int AddressLength = 16;

        /// <summary>
        ///     The byte position of the field line in the IPv6 header.
        ///     This is where the IP version, Traffic Class, and Flow Label fields are.
        /// </summary>
        public static readonly int VersionTrafficClassFlowLabelPosition = 0;

        /// <summary>
        ///     The byte position of the payload length field.
        /// </summary>
        public static readonly int PayloadLengthPosition;

        /// <summary>
        ///     The byte position of the next header field. (Replaces the ipv4 protocol field)
        /// </summary>
        public static readonly int NextHeaderPosition;

        /// <summary>
        ///     The byte position of the hop limit field.
        /// </summary>
        public static readonly int HopLimitPosition;

        /// <summary>
        ///     The byte position of the source address field.
        /// </summary>
        public static readonly int SourceAddressPosition;

        /// <summary>
        ///     The byte position of the destination address field.
        /// </summary>
        public static readonly int DestinationAddressPosition;

        /// <summary>
        ///     The byte length of the IPv6 Header
        /// </summary>
        public static readonly int HeaderLength; // == 40

        /// <summary>
        ///     Commutes the field positions.
        /// </summary>
        static IPv6Fields()
        {
            PayloadLengthPosition = VersionTrafficClassFlowLabelPosition + VersionTrafficClassFlowLabelLength;
            NextHeaderPosition = PayloadLengthPosition + PayloadLengthLength;
            HopLimitPosition = NextHeaderPosition + NextHeaderLength;
            SourceAddressPosition = HopLimitPosition + HopLimitLength;
            DestinationAddressPosition = SourceAddressPosition + AddressLength;
            HeaderLength = DestinationAddressPosition + AddressLength;
        }
    }
}