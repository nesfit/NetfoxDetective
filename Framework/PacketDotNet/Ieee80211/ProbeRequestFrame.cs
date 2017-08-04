using System;
using System.Net.NetworkInformation;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Probe request frames are used by stations to scan the area for existing networks.
        /// </summary>
        public class ProbeRequestFrame : ManagementFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public ProbeRequestFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.DestinationAddress = this.GetAddress(0);
                this.SourceAddress = this.GetAddress(1);
                this.BssId = this.GetAddress(2);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);

                if(bas.Length > ProbeRequestFields.InformationElement1Position)
                {
                    //create a segment that just refers to the info element section
                    var infoElementsSegment = new ByteArraySegment(bas.Bytes, (bas.Offset + ProbeRequestFields.InformationElement1Position),
                        (bas.Length - ProbeRequestFields.InformationElement1Position));

                    this.InformationElements = new InformationElementList(infoElementsSegment);
                }
                else
                {
                    this.InformationElements = new InformationElementList();
                }
                //cant set length until after we have handled the information elements
                //as they vary in length
                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="ProbeRequestFrame" /> class.
            /// </summary>
            /// <param name='SourceAddress'>
            ///     Source address.
            /// </param>
            /// <param name='DestinationAddress'>
            ///     Destination address.
            /// </param>
            /// <param name='BssId'>
            ///     Bss identifier (Mac Address of the Access Point).
            /// </param>
            /// <param name='InformationElements'>
            ///     Information elements.
            /// </param>
            public ProbeRequestFrame(PhysicalAddress SourceAddress, PhysicalAddress DestinationAddress, PhysicalAddress BssId, InformationElementList InformationElements)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField();
                this.InformationElements = new InformationElementList(InformationElements);

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementProbeRequest;
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
                    return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*3) + MacFields.SequenceControlLength
                            + this.InformationElements.Length);
                }
            }

            /// <summary>
            ///     Gets or sets the information elements included in the frame.
            /// </summary>
            /// <value>
            ///     The information elements.
            /// </value>
            /// <remarks>
            ///     Probe request frames normally contain information elements for
            ///     <see cref="InformationElement.ElementId.ServiceSetIdentity" />,
            ///     <see cref="InformationElement.ElementId.SupportedRates" /> and
            ///     <see cref="InformationElement.ElementId.ExtendedSupportedRates" /> in that order.
            /// </remarks>
            public InformationElementList InformationElements { get; set; }

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
                this.SetAddress(0, this.DestinationAddress);
                this.SetAddress(1, this.SourceAddress);
                this.SetAddress(2, this.BssId);
                this.SequenceControlBytes = this.SequenceControl.Field;

                //we now know the backing buffer is big enough to contain the info elements so we can safely copy them in
                this.InformationElements.CopyTo(this.header, this.header.Offset + ProbeRequestFields.InformationElement1Position);

                this.header.Length = this.FrameSize;
            }

            private class ProbeRequestFields
            {
                public static readonly int InformationElement1Position;
                static ProbeRequestFields() { InformationElement1Position = MacFields.SequenceControlPosition + MacFields.SequenceControlLength; }
            }
        }
    }
}