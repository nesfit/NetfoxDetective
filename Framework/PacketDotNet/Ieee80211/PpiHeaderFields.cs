namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     As defined by Airpcap.h
        ///     NOTE: PresentPosition may not be the only position present
        ///     as this the field can be extended if the high bit is set
        /// </summary>
        public class PpiHeaderFields
        {
            #region Constructors
            static PpiHeaderFields()
            {
                FlagsPosition = VersionPosition + VersionLength;
                LengthPosition = FlagsPosition + FlagsLength;
                DataLinkTypePosition = LengthPosition + LengthLength;
                FirstFieldPosition = DataLinkTypePosition + DataLinkTypeLength;
                PpiPacketHeaderLength = FirstFieldPosition;
            }
            #endregion Constructors

            #region Fields
            /// <summary>Position of the first iField Header</summary>
            public static readonly int FirstFieldPosition;

            /// <summary>Length of the Data Link Type</summary>
            public static readonly int DataLinkTypeLength = 4;

            /// <summary>The data link type position.</summary>
            public static readonly int DataLinkTypePosition;

            /// <summary>Length of the Flags field</summary>
            public static readonly int FlagsLength = 1;

            /// <summary>Position of the Flags field</summary>
            public static readonly int FlagsPosition;

            /// <summary>Length of the length field</summary>
            public static readonly int LengthLength = 2;

            /// <summary>Position of the length field</summary>
            public static readonly int LengthPosition;

            /// <summary>Length of the version field</summary>
            public static readonly int VersionLength = 1;

            /// <summary>Position of the version field</summary>
            public static readonly int VersionPosition = 0;

            /// <summary>The total length of the ppi packet header</summary>
            public static readonly int PpiPacketHeaderLength;

            /// <summary>The length of the PPI field header</summary>
            public static readonly int FieldHeaderLength = 4;
            #endregion Fields
        }
    }
}