using System;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Data data frame.
        /// </summary>
        public class DataDataFrame : DataFrame
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="DataDataFrame" /> class.
            /// </summary>
            /// <param name='bas'>
            ///     Bas.
            /// </param>
            public DataDataFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);
                this.ReadAddresses(); //must do this after reading FrameControl

                this.header.Length = this.FrameSize;
                var availablePayloadLength = this.GetAvailablePayloadLength();
                if(availablePayloadLength > 0) { this.payloadPacketOrData.TheByteArraySegment = this.header.EncapsulatedBytes(availablePayloadLength); }
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="DataDataFrame" /> class.
            /// </summary>
            public DataDataFrame()
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.SequenceControl = new SequenceControlField();
                this.AssignDefaultAddresses();

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.Data;
            }

            /// <summary>
            ///     Gets the size of the frame.
            /// </summary>
            /// <value>
            ///     The size of the frame.
            /// </value>
            public override int FrameSize
            {
                get
                {
                    //if we are in WDS mode then there are 4 addresses (normally it is just 3)
                    var numOfAddressFields = (this.FrameControl.ToDS && this.FrameControl.FromDS)? 4 : 3;

                    return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*numOfAddressFields) + MacFields.SequenceControlLength);
                }
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
                this.WriteAddressBytes();
            }
        }
    }
}