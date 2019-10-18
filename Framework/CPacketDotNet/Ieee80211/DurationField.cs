using System;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Duration field.
        /// </summary>
        public class DurationField
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="DurationField" /> class.
            /// </summary>
            public DurationField() { }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="field">
            ///     A <see cref="UInt16" />
            /// </param>
            public DurationField(UInt16 field) { this.Field = field; }

            /// <summary>
            ///     This is the raw Duration field
            /// </summary>
            public UInt16 Field { get; set; }
        }
    }
}