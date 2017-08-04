using System;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     The Sequence control field occurs in management and data frames and is used to
        ///     relate together fragmented payloads carried in multiple 802.11 frames.
        /// </summary>
        public class SequenceControlField
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="SequenceControlField" /> class.
            /// </summary>
            public SequenceControlField() { }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="field">
            ///     A <see cref="UInt16" />
            /// </param>
            public SequenceControlField(UInt16 field) { this.Field = field; }

            /// <summary>
            ///     Gets or sets the field that backs all the other properties in the class.
            /// </summary>
            /// <value>
            ///     The field.
            /// </value>
            public UInt16 Field { get; set; }

            /// <summary>
            ///     Gets or sets the sequence number.
            /// </summary>
            /// <value>
            ///     The sequence number.
            /// </value>
            public short SequenceNumber
            {
                get { return (short) (this.Field >> 4); }

                set
                {
                    //Use the & mask to make sure we only overwrite the sequence number part of the field
                    this.Field &= 0xF;
                    this.Field |= (UInt16) (value << 4);
                }
            }

            /// <summary>
            ///     Gets or sets the fragment number.
            /// </summary>
            /// <value>
            ///     The fragment number.
            /// </value>
            public byte FragmentNumber
            {
                get { return (byte) (this.Field&0x000F); }

                set
                {
                    this.Field &= unchecked((ushort) ~0xF);
                    this.Field |= (UInt16) (value&0x0F);
                }
            }
        }
    }
}