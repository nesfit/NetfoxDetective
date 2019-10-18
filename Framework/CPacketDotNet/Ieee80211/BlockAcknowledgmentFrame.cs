using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Format of the 802.11 block acknowledgment frame.
        ///     http://en.wikipedia.org/wiki/Block_acknowledgement
        /// </summary>
        public class BlockAcknowledgmentFrame : MacFrame
        {
            private byte[] blockAckBitmap;

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public BlockAcknowledgmentFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.ReceiverAddress = this.GetAddress(0);
                this.TransmitterAddress = this.GetAddress(1);
                this.BlockAcknowledgmentControl = new BlockAcknowledgmentControlField(this.BlockAckRequestControlBytes);
                this.BlockAckStartingSequenceControl = this.BlockAckStartingSequenceControlBytes;
                this.BlockAckBitmap = this.BlockAckBitmapBytes;
                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="BlockAcknowledgmentFrame" /> class.
            /// </summary>
            /// <param name='TransmitterAddress'>
            ///     Transmitter address.
            /// </param>
            /// <param name='ReceiverAddress'>
            ///     Receiver address.
            /// </param>
            /// <param name='BlockAckBitmap'>
            ///     The Block ack bitmap signalling the receive status of the MSDUs.
            /// </param>
            public BlockAcknowledgmentFrame(PhysicalAddress TransmitterAddress, PhysicalAddress ReceiverAddress, Byte[] BlockAckBitmap)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.ReceiverAddress = ReceiverAddress;
                this.TransmitterAddress = TransmitterAddress;
                this.BlockAcknowledgmentControl = new BlockAcknowledgmentControlField();
                this.BlockAckBitmap = BlockAckBitmap;

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ControlBlockAcknowledgment;
            }

            /// <summary>
            ///     Length of the frame
            /// </summary>
            public override int FrameSize
            {
                get
                {
                    return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*2) + BlockAcknowledgmentField.BlockAckRequestControlLength
                            + BlockAcknowledgmentField.BlockAckStartingSequenceControlLength + this.GetBitmapLength());
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
            ///     Gets or sets the block ack starting sequence control.
            /// </summary>
            /// <value>
            ///     The block ack starting sequence control.
            /// </value>
            public UInt16 BlockAckStartingSequenceControl { get; set; }

            /// <summary>
            ///     Gets or sets the block ack bitmap used to indicate the receive status of the MPDUs.
            /// </summary>
            /// <value>
            ///     The block ack bitmap.
            /// </value>
            /// <exception cref='ArgumentException'>
            ///     Is thrown when the bitmap is of an incorrect lenght. The bitmap must be either 8 or 64 btyes longs depending on
            ///     whether or not
            ///     it is compressed.
            /// </exception>
            public Byte[] BlockAckBitmap
            {
                get { return this.blockAckBitmap; }

                set
                {
                    if(value.Length == 8) {
                        this.BlockAcknowledgmentControl.CompressedBitmap = true;
                    }
                    else if(value.Length == 64) {
                        this.BlockAcknowledgmentControl.CompressedBitmap = false;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid BlockAckBitmap size. Must be either 8 or 64 bytes long.");
                    }

                    this.blockAckBitmap = value;
                }
            }

            /// <summary>
            ///     Gets or sets the block ack request control bytes.
            /// </summary>
            /// <value>
            ///     The block ack request control bytes.
            /// </value>
            private UInt16 BlockAckRequestControlBytes
            {
                get
                {
                    if(this.header.Length >= (BlockAcknowledgmentField.BlockAckRequestControlPosition + BlockAcknowledgmentField.BlockAckRequestControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + BlockAcknowledgmentField.BlockAckRequestControlPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + BlockAcknowledgmentField.BlockAckRequestControlPosition); }
            }

            private UInt16 BlockAckStartingSequenceControlBytes
            {
                get
                {
                    if(this.header.Length >= (BlockAcknowledgmentField.BlockAckStartingSequenceControlPosition + BlockAcknowledgmentField.BlockAckStartingSequenceControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + BlockAcknowledgmentField.BlockAckStartingSequenceControlPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + BlockAcknowledgmentField.BlockAckStartingSequenceControlPosition); }
            }

            private Byte[] BlockAckBitmapBytes
            {
                get
                {
                    var bitmap = new Byte[this.GetBitmapLength()];
                    if(this.header.Length >= (BlockAcknowledgmentField.BlockAckBitmapPosition + this.GetBitmapLength())) {
                        Array.Copy(this.header.Bytes, (BlockAcknowledgmentField.BlockAckBitmapPosition), bitmap, 0, this.GetBitmapLength());
                    }
                    return bitmap;
                }

                set { Array.Copy(this.BlockAckBitmap, 0, this.header.Bytes, BlockAcknowledgmentField.BlockAckBitmapPosition, this.GetBitmapLength()); }
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
                this.BlockAckBitmapBytes = this.BlockAckBitmap;

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

            private int GetBitmapLength() { return this.BlockAcknowledgmentControl.CompressedBitmap? 8 : 64; }

            private class BlockAcknowledgmentField
            {
                public static readonly int BlockAckRequestControlLength = 2;
                public static readonly int BlockAckStartingSequenceControlLength = 2;
                public static readonly int BlockAckRequestControlPosition;
                public static readonly int BlockAckStartingSequenceControlPosition;
                public static readonly int BlockAckBitmapPosition;

                static BlockAcknowledgmentField()
                {
                    BlockAckRequestControlPosition = MacFields.DurationIDPosition + MacFields.DurationIDLength + (2*MacFields.AddressLength);
                    BlockAckStartingSequenceControlPosition = BlockAckRequestControlPosition + BlockAckRequestControlLength;
                    BlockAckBitmapPosition = BlockAckStartingSequenceControlPosition + BlockAckStartingSequenceControlLength;
                }
            }
        }
    }
}