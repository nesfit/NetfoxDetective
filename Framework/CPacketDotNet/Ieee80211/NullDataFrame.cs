using System;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Null data frames are like normal data frames except they carry no payload. They are primarily used for control
        ///     purposes
        ///     such as power management or telling an Access Point to buffer packets while a station scans other channels.
        /// </summary>
        public class NullDataFrame : DataFrame
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="NullDataFrame" /> class.
            /// </summary>
            /// <param name='bas'>
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public NullDataFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);
                this.ReadAddresses();

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="NullDataFrame" /> class.
            /// </summary>
            public NullDataFrame()
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.SequenceControl = new SequenceControlField();
                this.AssignDefaultAddresses();

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.DataNullFunctionNoData;
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