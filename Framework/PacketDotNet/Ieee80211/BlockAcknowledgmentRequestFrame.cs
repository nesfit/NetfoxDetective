using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Block acknowledgment request frame.
        /// </summary>
        public class BlockAcknowledgmentRequestFrame : MacFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public BlockAcknowledgmentRequestFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.ReceiverAddress = this.GetAddress(0);
                this.TransmitterAddress = this.GetAddress(1);
                this.BlockAcknowledgmentControl = new BlockAcknowledgmentControlField(this.BlockAckRequestControlBytes);
                this.BlockAckStartingSequenceControl = this.BlockAckStartingSequenceControlBytes;

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="BlockAcknowledgmentRequestFrame" /> class.
            /// </summary>
            /// <param name='TransmitterAddress'>
            ///     Transmitter address.
            /// </param>
            /// <param name='ReceiverAddress'>
            ///     Receiver address.
            /// </param>
            public BlockAcknowledgmentRequestFrame(PhysicalAddress TransmitterAddress, PhysicalAddress ReceiverAddress)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.ReceiverAddress = ReceiverAddress;
                this.TransmitterAddress = TransmitterAddress;
                this.BlockAcknowledgmentControl = new BlockAcknowledgmentControlField();

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ControlBlockAcknowledgmentRequest;
            }

            /// <summary>
            ///     Length of the frame
            /// </summary>
            public override int FrameSize
            {
                get
                {
                    return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*2) + BlockAckRequestField.BlockAckRequestControlLength
                            + BlockAckRequestField.BlockAckStartingSequenceControlLength);
                }
            }

            /// <summary>
            ///     Receiver address
            /// </summary>
            public PhysicalAddress ReceiverAddress { get; set; }

            /// <summary>
            ///     Transmitter address
            /// </summary>
            public PhysicalAddress TransmitterAddress { get; set; }

            /// <summary>
            ///     Block acknowledgment control field
            /// </summary>
            public BlockAcknowledgmentControlField BlockAcknowledgmentControl { get; set; }

            /// <summary>
            ///     Gets or sets the sequence number of the first MSDU for which this
            ///     block acknowledgement request frame is sent
            /// </summary>
            /// <value>
            ///     The block ack starting sequence control field value
            /// </value>
            public UInt16 BlockAckStartingSequenceControl { get; set; }

            /// <summary>
            ///     Block acknowledgment control bytes are the first two bytes of the frame
            /// </summary>
            private UInt16 BlockAckRequestControlBytes
            {
                get
                {
                    if(this.header.Length >= (BlockAckRequestField.BlockAckRequestControlPosition + BlockAckRequestField.BlockAckRequestControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + BlockAckRequestField.BlockAckRequestControlPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + BlockAckRequestField.BlockAckRequestControlPosition); }
            }

            /// <summary>
            ///     Gets or sets the block ack starting sequence control.
            /// </summary>
            /// <value>
            ///     The block ack starting sequence control.
            /// </value>
            private UInt16 BlockAckStartingSequenceControlBytes
            {
                get
                {
                    if(this.header.Length >= (BlockAckRequestField.BlockAckStartingSequenceControlPosition + BlockAckRequestField.BlockAckStartingSequenceControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + BlockAckRequestField.BlockAckStartingSequenceControlPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + BlockAckRequestField.BlockAckStartingSequenceControlPosition); }
            }

            /// <summary>
            ///     Writes the current packet properties to the backing ByteArraySegment.
            /// </summary>
            public override void UpdateCalculatedValues()
            {
                if((this.header == null) || (this.header.Length > (this.header.BytesLength - this.header.Offset)) || (this.header.Length < this.FrameSize)) {
                    this.header = new ByteArraySegment(new Byte[this.FrameSize]);
                }

                this.FrameControlBytes = this.FrameControl.Field;
                this.DurationBytes = this.Duration.Field;
                this.SetAddress(0, this.ReceiverAddress);
                this.SetAddress(1, this.TransmitterAddress);

                this.BlockAckRequestControlBytes = this.BlockAcknowledgmentControl.Field;
                this.BlockAckStartingSequenceControlBytes = this.BlockAckStartingSequenceControl;

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Returns a string with a description of the addresses used in the packet.
            ///     This is used as a compoent of the string returned by ToString().
            /// </summary>
            /// <returns>
            ///     The address string.
            /// </returns>
            protected override String GetAddressString()
            {
                return String.Format("RA {0} TA {1}", this.ReceiverAddress, this.TransmitterAddress);
            }

            private class BlockAckRequestField
            {
                public static readonly int BlockAckRequestControlLength = 2;
                public static readonly int BlockAckStartingSequenceControlLength = 2;
                public static readonly int BlockAckRequestControlPosition;
                public static readonly int BlockAckStartingSequenceControlPosition;

                static BlockAckRequestField()
                {
                    BlockAckRequestControlPosition = MacFields.DurationIDPosition + MacFields.DurationIDLength + (2*MacFields.AddressLength);
                    BlockAckStartingSequenceControlPosition = BlockAckRequestControlPosition + BlockAckRequestControlLength;
                }
            }
        }
    }
}