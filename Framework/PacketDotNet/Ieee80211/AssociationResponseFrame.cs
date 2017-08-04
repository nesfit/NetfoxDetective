using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Format of an 802.11 management association response frame.
        /// </summary>
        public class AssociationResponseFrame : ManagementFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public AssociationResponseFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.DestinationAddress = this.GetAddress(0);
                this.SourceAddress = this.GetAddress(1);
                this.BssId = this.GetAddress(2);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);

                this.CapabilityInformation = new CapabilityInformationField(this.CapabilityInformationBytes);
                this.StatusCode = this.StatusCodeBytes;
                this.AssociationId = this.AssociationIdBytes;

                if(bas.Length > AssociationResponseFields.InformationElement1Position)
                {
                    //create a segment that just refers to the info element section
                    var infoElementsSegment = new ByteArraySegment(bas.Bytes, (bas.Offset + AssociationResponseFields.InformationElement1Position),
                        (bas.Length - AssociationResponseFields.InformationElement1Position));

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
            ///     Initializes a new instance of the <see cref="AssociationResponseFrame" /> class.
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
            public AssociationResponseFrame(PhysicalAddress SourceAddress, PhysicalAddress DestinationAddress, PhysicalAddress BssId, InformationElementList InformationElements)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField();
                this.CapabilityInformation = new CapabilityInformationField();

                this.InformationElements = new InformationElementList(InformationElements);

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementAssociationResponse;
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
                            + AssociationResponseFields.CapabilityInformationLength + AssociationResponseFields.StatusCodeLength + AssociationResponseFields.AssociationIdLength
                            + this.InformationElements.Length);
                }
            }

            /// <summary>
            ///     The capability information field that describes the networks capabilities.
            /// </summary>
            public CapabilityInformationField CapabilityInformation { get; set; }

            /// <summary>
            ///     Value indicating the success or failure of the association.
            /// </summary>
            public AuthenticationStatusCode StatusCode { get; set; }

            /// <summary>
            ///     The id assigned to the station by the access point to assist in management and control functions.
            ///     Although this is a 16bit field only 14 of the bits are used to represent the id. Therefore the available values
            ///     for this field are inthe range 1-2,007.
            /// </summary>
            public UInt16 AssociationId { get; set; }

            /// <summary>
            ///     The information elements included in the frame
            /// </summary>
            public InformationElementList InformationElements { get; set; }

            /// <summary>
            ///     The raw capability information bytes
            /// </summary>
            private UInt16 CapabilityInformationBytes
            {
                get
                {
                    if(this.header.Length >= (AssociationResponseFields.CapabilityInformationPosition + AssociationResponseFields.CapabilityInformationLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AssociationResponseFields.CapabilityInformationPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + AssociationResponseFields.CapabilityInformationPosition); }
            }

            private AuthenticationStatusCode StatusCodeBytes
            {
                get
                {
                    if(this.header.Length >= (AssociationResponseFields.StatusCodePosition + AssociationResponseFields.StatusCodeLength)) {
                        return (AuthenticationStatusCode) EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AssociationResponseFields.StatusCodePosition);
                    }
                    //This seems the most sensible value to return when it is not possible
                    //to extract a meaningful value
                    return AuthenticationStatusCode.UnspecifiedFailure;
                }

                set { EndianBitConverter.Little.CopyBytes((UInt16) value, this.header.Bytes, this.header.Offset + AssociationResponseFields.StatusCodePosition); }
            }

            private UInt16 AssociationIdBytes
            {
                get
                {
                    if(this.header.Length >= AssociationResponseFields.AssociationIdPosition + AssociationResponseFields.AssociationIdLength)
                    {
                        var associationID = EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AssociationResponseFields.AssociationIdPosition);
                        return (UInt16) (associationID&0xCF);
                    }
                    return 0;
                }

                set
                {
                    var associationID = (UInt16) (value&0xCF);
                    EndianBitConverter.Little.CopyBytes(associationID, this.header.Bytes, this.header.Offset + AssociationResponseFields.AssociationIdPosition);
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
                this.SetAddress(0, this.DestinationAddress);
                this.SetAddress(1, this.SourceAddress);
                this.SetAddress(2, this.BssId);
                this.SequenceControlBytes = this.SequenceControl.Field;
                this.CapabilityInformationBytes = this.CapabilityInformation.Field;
                this.StatusCodeBytes = this.StatusCode;
                this.AssociationIdBytes = this.AssociationId;

                //we now know the backing buffer is big enough to contain the info elements so we can safely copy them in
                this.InformationElements.CopyTo(this.header, this.header.Offset + AssociationResponseFields.InformationElement1Position);

                this.header.Length = this.FrameSize;
            }

            private class AssociationResponseFields
            {
                public static readonly int CapabilityInformationLength = 2;
                public static readonly int StatusCodeLength = 2;
                public static readonly int AssociationIdLength = 2;
                public static readonly int CapabilityInformationPosition;
                public static readonly int StatusCodePosition;
                public static readonly int AssociationIdPosition;
                public static readonly int InformationElement1Position;

                static AssociationResponseFields()
                {
                    CapabilityInformationPosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength;
                    StatusCodePosition = CapabilityInformationPosition + CapabilityInformationLength;
                    AssociationIdPosition = StatusCodePosition + StatusCodeLength;
                    InformationElement1Position = AssociationIdPosition + AssociationIdLength;
                }
            }
        }
    }
}