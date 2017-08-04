using System;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     The Qos null data frame serves the same purpose as <see cref="NullDataFrame" /> but also includes a
        ///     quality of service control field.
        /// </summary>
        public class QosNullDataFrame : DataFrame
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="QosNullDataFrame" /> class.
            /// </summary>
            /// <param name='bas'>
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public QosNullDataFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);
                this.QosControl = this.QosControlBytes;
                this.ReadAddresses();

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="QosNullDataFrame" /> class.
            /// </summary>
            public QosNullDataFrame()
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.SequenceControl = new SequenceControlField();

                this.AssignDefaultAddresses();

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.QosNullData;
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
                            + QosNullDataField.QosControlLength);
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
                    if(this.header.Length >= (QosNullDataField.QosControlPosition + QosNullDataField.QosControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + QosNullDataField.QosControlPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + QosNullDataField.QosControlPosition); }
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

            private class QosNullDataField
            {
                public static readonly int QosControlLength = 2;
                public static readonly int QosControlPosition;
                static QosNullDataField() { QosControlPosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength; }
            }
        }
    }
}