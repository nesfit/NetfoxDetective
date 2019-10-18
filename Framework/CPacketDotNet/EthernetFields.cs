namespace PacketDotNet
{
    /// <summary>
    ///     Ethernet protocol field encoding information.
    /// </summary>
    public class EthernetFields
    {
        /// <summary> Width of the ethernet type code in bytes.</summary>
        public static readonly int TypeLength = 2;

        /// <summary> Position of the destination MAC address within the ethernet header.</summary>
        public static readonly int DestinationMacPosition = 0;

        /// <summary> Position of the source MAC address within the ethernet header.</summary>
        public static readonly int SourceMacPosition;

        /// <summary> Position of the ethernet type field within the ethernet header.</summary>
        public static readonly int TypePosition;

        /// <summary> Total length of an ethernet header in bytes.</summary>
        public static readonly int HeaderLength; // == 14

        /// <summary>
        ///     size of an ethernet mac address in bytes
        /// </summary>
        public static readonly int MacAddressLength = 6;

        static EthernetFields()
        {
            SourceMacPosition = MacAddressLength;
            TypePosition = MacAddressLength*2;
            HeaderLength = TypePosition + TypeLength;
        }
    }
}