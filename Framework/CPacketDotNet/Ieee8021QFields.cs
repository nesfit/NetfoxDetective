namespace PacketDotNet
{
    /// <summary> 802.1Q fields </summary>
    public class Ieee8021QFields
    {
        /// <summary> Length of the ethertype value in bytes.</summary>
        public static readonly int TypeLength = 2;

        /// <summary> Length of the tag control information in bytes. </summary>
        public static readonly int TagControlInformationLength = 2;

        /// <summary> Position of the tag control information </summary>
        public static readonly int TagControlInformationPosition = 0;

        /// <summary> Position of the type field </summary>
        public static readonly int TypePosition;

        /// <summary> Length in bytes of a Ieee8021Q header.</summary>
        public static readonly int HeaderLength; // 4

        static Ieee8021QFields()
        {
            TypePosition = TagControlInformationPosition + TagControlInformationLength;
            HeaderLength = TypePosition + TypeLength;
        }
    }
}