namespace PacketDotNet
{
    public struct GreFields
    {
        public static readonly byte GRE_CHECKSUM_MASK = 0x80;
        public static readonly byte GRE_KEY_MASK = 0x20;
        public static readonly byte GRE_SEQUENCE_MASK = 0x10;
        public static readonly byte GRE_VERSION_MASK = 0x07;
        public static readonly int VersionByteOffset = 1;
    }
}