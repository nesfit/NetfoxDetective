namespace PacketDotNet
{
    /// <summary> IGMP protocol field encoding information. </summary>
    public class IGMPv2Fields
    {
        /// <summary> Length of the IGMP message type code in bytes.</summary>
        public static readonly int TypeLength = 1;

        /// <summary> Length of the IGMP max response code in bytes.</summary>
        public static readonly int MaxResponseTimeLength = 1;

        /// <summary> Length of the IGMP header checksum in bytes.</summary>
        public static readonly int ChecksumLength = 2;

        /// <summary> Length of group address in bytes.</summary>
        public static readonly int GroupAddressLength;

        /// <summary> Position of the IGMP message type.</summary>
        public static readonly int TypePosition = 0;

        /// <summary> Position of the IGMP max response code.</summary>
        public static readonly int MaxResponseTimePosition;

        /// <summary> Position of the IGMP header checksum.</summary>
        public static readonly int ChecksumPosition;

        /// <summary> Position of the IGMP group address.</summary>
        public static readonly int GroupAddressPosition;

        /// <summary> Length in bytes of an IGMP header.</summary>
        public static readonly int HeaderLength; // 8

        static IGMPv2Fields()
        {
            GroupAddressLength = IPv4Fields.AddressLength;
            MaxResponseTimePosition = TypePosition + TypeLength;
            ChecksumPosition = MaxResponseTimePosition + MaxResponseTimeLength;
            GroupAddressPosition = ChecksumPosition + ChecksumLength;
            HeaderLength = GroupAddressPosition + GroupAddressLength;
        }
    }
}