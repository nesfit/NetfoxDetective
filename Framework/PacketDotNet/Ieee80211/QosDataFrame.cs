using System;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Qos data frames are like regualr data frames except they contain a quality of service
        ///     field as deinfed in the 802.11e standard.
        /// </summary>
        public class QosDataFrame : DataFrame
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="QosDataFrame" /> class.
            /// </summary>
            /// <param name='bas'>
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public QosDataFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);
                this.QosControl = this.QosControlBytes;
                this.ReadAddresses();

                this.header.Length = this.FrameSize;
                var availablePayloadLength = this.GetAvailablePayloadLength();
                if(availablePayloadLength > 0) { this.payloadPacketOrData.TheByteArraySegment = this.header.EncapsulatedBytes(availablePayloadLength); }
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="QosDataFrame" /> class.
            /// </summary>
            public QosDataFrame()
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.SequenceControl = new SequenceControlField();
                this.AssignDefaultAddresses();

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.QosData;
            }

            /// <summary>
            ///     Length of the frame header.
            ///     This does not include the FCS, it represents only the header bytes that would
            ///     would preceed any payload.
            /// </summary>
            public override int FrameSize
            {
                get
                {
                    //if we are in WDS mode then there are 4 addresses (normally it is just 3)
                    var numOfAddressFields = (this.FrameControl.ToDS && this.FrameControl.FromDS)? 4 : 3;

                    return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*numOfAddressFields) + MacFields.SequenceControlLength
                            + QosDataField.QosControlLength);
                }
            }

            /// <summary>
            ///     Gets or sets the qos control field.
            /// </summary>
            /// <value>
            ///     The qos control field.
            /// </value>
            public UInt16 QosControl { get; set; }

            private UInt16 QosControlBytes
            {
                get
                {
                    if(this.header.Length >= (QosDataField.QosControlPosition + QosDataField.QosControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + QosDataField.QosControlPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + QosDataField.QosControlPosition); }
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
                this.SequenceControlBytes = this.SequenceControl.Field;
                this.QosControlBytes = this.QosControl;
                this.WriteAddressBytes();
            }

            private class QosDataField
            {
                public static readonly int QosControlLength = 2;
                public static readonly int QosControlPosition;
                static QosDataField() { QosControlPosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength; }
            }
        }
    }
}