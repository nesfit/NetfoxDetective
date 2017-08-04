using System;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {

        #region Enumerations
        /// <summary>
        ///     from PPI v 1.0.10
        /// </summary>
        [Flags]
        public enum PpiFieldType
        {
            /// <summary>
            ///     PpiReserved0
            /// </summary>
            PpiReserved0 = 0,

            /// <summary>
            ///     PpiReserved1
            /// </summary>
            PpiReserved1 = 1,

            /// <summary>
            ///     PpiCommon
            /// </summary>
            PpiCommon = 2,

            /// <summary>
            ///     PpiMacExtensions
            /// </summary>
            PpiMacExtensions = 3,

            /// <summary>
            ///     PpiMacPhy
            /// </summary>
            PpiMacPhy = 4,

            /// <summary>
            ///     PpiSpectrum
            /// </summary>
            PpiSpectrum = 5,

            /// <summary>
            ///     PpiProcessInfo
            /// </summary>
            PpiProcessInfo = 6,

            /// <summary>
            ///     PpiCaptureInfo
            /// </summary>
            PpiCaptureInfo = 7,

            /// <summary>
            ///     PpiAggregation
            /// </summary>
            PpiAggregation = 8,

            /// <summary>
            ///     Ppi802_3
            /// </summary>
            Ppi802_3 = 9,

            /// <summary>
            ///     PpiReservedAll
            /// </summary>
            PpiReservedAll = 10
        }
        #endregion Enumerations
    }
}