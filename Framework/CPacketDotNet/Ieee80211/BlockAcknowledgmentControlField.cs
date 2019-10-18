using System;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Block acknowledgment control field.
        /// </summary>
        public class BlockAcknowledgmentControlField
        {
            /// <summary>
            ///     The available block acknowledgement policies.
            /// </summary>
            public enum AcknowledgementPolicy
            {
                /// <summary>
                ///     The acknowledgement does not have to be sent immediately after the request
                /// </summary>
                Delayed = 0,

                /// <summary>
                ///     The acknowledgement must be sent immediately after the request
                /// </summary>
                Immediate = 1
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="BlockAcknowledgmentControlField" /> class.
            /// </summary>
            public BlockAcknowledgmentControlField() { }

            /// <summary>
            ///     Initializes a new instance of the <see cref="BlockAcknowledgmentControlField" /> class.
            /// </summary>
            /// <param name='field'>
            ///     Field.
            /// </param>
            public BlockAcknowledgmentControlField(UInt16 field) { this.Field = field; }

            /// <summary>
            ///     The block acknowledgement policy in use
            /// </summary>
            public AcknowledgementPolicy Policy
            {
                get { return (AcknowledgementPolicy) (this.Field&0x1); }

                set
                {
                    if(value == AcknowledgementPolicy.Immediate) { this.Field |= 0x1; }
                    else
                    { this.Field &= unchecked((UInt16) ~(0x1)); }
                }
            }

            /// <summary>
            ///     True if the acknowledgement can ack multi traffic ids
            /// </summary>
            public bool MultiTid
            {
                get { return (((this.Field >> 1)&0x1) == 1)? true : false; }

                set
                {
                    if(value) { this.Field |= (1 << 0x1); }
                    else
                    { this.Field &= unchecked((UInt16) ~(1 << 0x1)); }
                }
            }

            /// <summary>
            ///     True if the frame is using a compressed acknowledgement bitmap.
            ///     Newer standards used a compressed bitmap reducing its size
            /// </summary>
            public bool CompressedBitmap
            {
                get { return (((this.Field >> 2)&0x1) == 1)? true : false; }

                set
                {
                    if(value) { this.Field |= (1 << 0x2); }
                    else
                    { this.Field &= unchecked((UInt16) ~(1 << 0x2)); }
                }
            }

            /// <summary>
            ///     The traffic id being ack'd
            /// </summary>
            public byte Tid
            {
                get { return (byte) (this.Field >> 12); }

                set
                {
                    this.Field &= 0x0FFF;
                    this.Field |= (UInt16) (value << 12);
                }
            }

            /// <summary>
            ///     Gets or sets the field. This provides direct access to the bytes that back all the other properties in the field.
            /// </summary>
            /// <value>
            ///     The field.
            /// </value>
            public UInt16 Field { get; set; }
        }
    }
}