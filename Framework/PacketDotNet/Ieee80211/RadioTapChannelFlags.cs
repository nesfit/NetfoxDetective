using System;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Channel flags
        /// </summary>
        [Flags]
        public enum RadioTapChannelFlags : ushort
        {
            /// <summary>Turbo channel</summary>
            Turbo = 0x0010,

            /// <summary>CCK channel</summary>
            Cck = 0x0020,

            /// <summary>OFDM channel</summary>
            Ofdm = 0x0040,

            /// <summary>2 GHz spectrum channel</summary>
            Channel2Ghz = 0x0080,

            /// <summary>5 GHz spectrum channel</summary>
            Channel5Ghz = 0x0100,

            /// <summary>Only passive scan allowed</summary>
            Passive = 0x0200,

            /// <summary>Dynamic CCK-OFDM channel</summary>
            DynamicCckOfdm = 0x0400,

            /// <summary>GFSK channel (FHSS PHY)</summary>
            Gfsk = 0x0800,

            /// <summary>11a static turbo channel only</summary>
            StaticTurbo = 0x2000
        };
    }
}