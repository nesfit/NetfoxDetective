using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Reassociation request frame.
        ///     Sent when a wireless client is going from one access point to another
        ///     http://en.wikipedia.org/wiki/IEEE_802.11#Frames
        /// </summary>
        public class ReassociationRequestFrame : ManagementFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public ReassociationRequestFrame(ByteArraySegment bas)
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
                this.CurrentAccessPointAddress = this.CurrentAccessPointAddressBytes;

                if(bas.Length > ReassociationRequestFields.InformationElement1Position)
                {
                    //create a segment that just refers to the info element section
                    var infoElementsSegment = new ByteArraySegment(bas.Bytes, (bas.Offset + ReassociationRequestFields.InformationElement1Position),
                        (bas.Length - ReassociationRequestFields.InformationElement1Position));

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
            ///     Initializes a new instance of the <see cref="ReassociationRequestFrame" /> class.
            /// </summary>
            /// <param name='SourceAddress'>
            ///     Source address.
            /// </param>
            /// <param name='DestinationAddress'>
            ///     Destination address.
            /// </param>
            /// <param name='BssId'>
            ///     BssId.
            /// </param>
            /// <param name='InformationElements'>
            ///     Information elements.
            /// </param>
            public ReassociationRequestFrame(PhysicalAddress SourceAddress, PhysicalAddress DestinationAddress, PhysicalAddress BssId, InformationElementList InformationElements)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField();
                this.CapabilityInformation = new CapabilityInformationField();
                this.InformationElements = new InformationElementList(InformationElements);

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementReassociationRequest;
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
                            + ReassociationRequestFields.CapabilityInformationLength + ReassociationRequestFields.ListenIntervalLength + MacFields.AddressLength
                            + this.InformationElements.Length);
                }
            }

            /// <summary>
            ///     Gets or sets the capability information, the type of network the mobile station wants to join
            /// </summary>
            /// <value>
            ///     The capability information.
            /// </value>
            public CapabilityInformationField CapabilityInformation { get; set; }

            /// <summary>
            ///     Gets or sets the listen interval. This is the number of beacon interval time periods that the access
            ///     point must retain buffered packets for.
            /// </summary>
            /// <value>
            ///     The listen interval.
            /// </value>
            public UInt16 ListenInterval { get; set; }

            /// <summary>
            ///     DestinationAddress
            /// </summary>
            public PhysicalAddress CurrentAccessPointAddress { get; set; }

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
                    if(this.header.Length >= (ReassociationRequestFields.CapabilityInformationPosition + ReassociationRequestFields.CapabilityInformationLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + ReassociationRequestFields.CapabilityInformationPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + ReassociationRequestFields.CapabilityInformationPosition); }
            }

            /// <summary>
            ///     Gets or sets the listen interval, the length of buffered frame retention
            /// </summary>
            /// <value>
            ///     The listen interval.
            /// </value>
            private UInt16 ListenIntervalBytes
            {
                get
                {
                    if(this.header.Length >= (ReassociationRequestFields.ListenIntervalPosition + ReassociationRequestFields.ListenIntervalLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + ReassociationRequestFields.ListenIntervalPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + ReassociationRequestFields.ListenIntervalPosition); }
            }

            private PhysicalAddress CurrentAccessPointAddressBytes
            {
                get { return this.GetAddressByOffset(this.header.Offset + ReassociationRequestFields.CurrentAccessPointPosition); }

                set { this.SetAddressByOffset(this.header.Offset + ReassociationRequestFields.CurrentAccessPointPosition, value); }
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
                this.InformationElements.CopyTo(this.header, this.header.Offset + ReassociationRequestFields.InformationElement1Position);

                this.header.Length = this.FrameSize;
            }

            private class ReassociationRequestFields
            {
                public static readonly int CapabilityInformationLength = 2;
                public static readonly int ListenIntervalLength = 2;
                public static readonly int CapabilityInformationPosition;
                public static readonly int ListenIntervalPosition;
                public static readonly int CurrentAccessPointPosition;
                public static readonly int InformationElement1Position;

                static ReassociationRequestFields()
                {
                    CapabilityInformationPosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength;
                    ListenIntervalPosition = CapabilityInformationPosition + CapabilityInformationLength;
                    CurrentAccessPointPosition = ListenIntervalPosition + ListenIntervalLength;
                    InformationElement1Position = CurrentAccessPointPosition + MacFields.AddressLength;
                }
            }
        }
    }
}