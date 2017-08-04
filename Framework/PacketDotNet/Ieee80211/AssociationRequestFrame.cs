using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Format of an 802.11 management association frame.
        /// </summary>
        public class AssociationRequestFrame : ManagementFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public AssociationRequestFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.DestinationAddress = this.GetAddress(0);
                this.SourceAddress = this.GetAddress(1);
                this.BssId = this.GetAddress(2);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);

                this.CapabilityInformation = new CapabilityInformationField(this.CapabilityInformationBytes);
                this.ListenInterval = this.ListenIntervalBytes;

                if(bas.Length > AssociationRequestFields.InformationElement1Position)
                {
                    //create a segment that just refers to the info element section
                    var infoElementsSegment = new ByteArraySegment(bas.Bytes, (bas.Offset + AssociationRequestFields.InformationElement1Position),
                        (bas.Length - AssociationRequestFields.InformationElement1Position));

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
            ///     Initializes a new instance of the <see cref="AssociationRequestFrame" /> class.
            /// </summary>
            /// <param name='SourceAddress'>
            ///     Source address.
            /// </param>
            /// <param name='DestinationAddress'>
            ///     Destination address.
            /// </param>
            /// <param name='BssId'>
            ///     Bss identifier (MAC Address of Access Point).
            /// </param>
            /// <param name='InformationElements'>
            ///     Information elements.
            /// </param>
            public AssociationRequestFrame(PhysicalAddress SourceAddress, PhysicalAddress DestinationAddress, PhysicalAddress BssId, InformationElementList InformationElements)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField();
                this.CapabilityInformation = new CapabilityInformationField();
                this.InformationElements = new InformationElementList(InformationElements);

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementAssociationRequest;
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
                    return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*3) + MacFields.SequenceControlLength
                            + AssociationRequestFields.CapabilityInformationLength + AssociationRequestFields.ListenIntervalLength + this.InformationElements.Length);
                }
            }

            /// <summary>
            ///     Gets or sets the capability information.
            /// </summary>
            /// <value>
            ///     The capability information.
            /// </value>
            public CapabilityInformationField CapabilityInformation { get; set; }

            /// <summary>
            ///     Gets or sets the listen interval.
            /// </summary>
            /// <value>
            ///     The listen interval.
            /// </value>
            public UInt16 ListenInterval { get; set; }

            /// <summary>
            ///     Gets or sets the information elements.
            /// </summary>
            /// <value>
            ///     The information elements.
            /// </value>
            public InformationElementList InformationElements { get; set; }

            /// <summary>
            ///     Frame control bytes are the first two bytes of the frame
            /// </summary>
            private UInt16 CapabilityInformationBytes
            {
                get
                {
                    if(this.header.Length >= (AssociationRequestFields.CapabilityInformationPosition + AssociationRequestFields.CapabilityInformationLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AssociationRequestFields.CapabilityInformationPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + AssociationRequestFields.CapabilityInformationPosition); }
            }

            private UInt16 ListenIntervalBytes
            {
                get
                {
                    if(this.header.Length >= (AssociationRequestFields.ListenIntervalPosition + AssociationRequestFields.ListenIntervalLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AssociationRequestFields.ListenIntervalPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + AssociationRequestFields.ListenIntervalPosition); }
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
                this.SetAddress(0, this.DestinationAddress);
                this.SetAddress(1, this.SourceAddress);
                this.SetAddress(2, this.BssId);
                this.SequenceControlBytes = this.SequenceControl.Field;
                this.CapabilityInformationBytes = this.CapabilityInformation.Field;

                //we now know the backing buffer is big enough to contain the info elements so we can safely copy them in
                this.InformationElements.CopyTo(this.header, this.header.Offset + AssociationRequestFields.InformationElement1Position);

                this.header.Length = this.FrameSize;
            }

            private class AssociationRequestFields
            {
                public static readonly int CapabilityInformationLength = 2;
                public static readonly int ListenIntervalLength = 2;
                public static readonly int CapabilityInformationPosition;
                public static readonly int ListenIntervalPosition;
                public static readonly int InformationElement1Position;

                static AssociationRequestFields()
                {
                    CapabilityInformationPosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength;
                    ListenIntervalPosition = CapabilityInformationPosition + CapabilityInformationLength;
                    InformationElement1Position = ListenIntervalPosition + ListenIntervalLength;
                }
            }
        }
    }
}