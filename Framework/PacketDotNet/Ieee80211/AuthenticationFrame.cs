using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Format of an 802.11 management authentication frame.
        /// </summary>
        public class AuthenticationFrame : ManagementFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public AuthenticationFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.DestinationAddress = this.GetAddress(0);
                this.SourceAddress = this.GetAddress(1);
                this.BssId = this.GetAddress(2);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);
                this.AuthenticationAlgorithmNumber = this.AuthenticationAlgorithmNumberBytes;
                this.AuthenticationAlgorithmTransactionSequenceNumber = this.AuthenticationAlgorithmTransactionSequenceNumberBytes;

                if(bas.Length > AuthenticationFields.InformationElement1Position)
                {
                    //create a segment that just refers to the info element section
                    var infoElementsSegment = new ByteArraySegment(bas.Bytes, (bas.Offset + AuthenticationFields.InformationElement1Position),
                        (bas.Length - AuthenticationFields.InformationElement1Position));

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
            ///     Initializes a new instance of the <see cref="AuthenticationFrame" /> class.
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
            public AuthenticationFrame(PhysicalAddress SourceAddress, PhysicalAddress DestinationAddress, PhysicalAddress BssId, InformationElementList InformationElements)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField();
                this.InformationElements = new InformationElementList(InformationElements);

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementAuthentication;
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
                            + AuthenticationFields.AuthAlgorithmNumLength + AuthenticationFields.AuthAlgorithmTransactionSequenceNumLength + AuthenticationFields.StatusCodeLength
                            + this.InformationElements.Length);
                }
            }

            /// <summary>
            ///     Number used for selection of authentication algorithm
            /// </summary>
            public UInt16 AuthenticationAlgorithmNumber { get; set; }

            /// <summary>
            ///     Sequence number to define the step of the authentication algorithm
            /// </summary>
            public UInt16 AuthenticationAlgorithmTransactionSequenceNumber { get; set; }

            /// <summary>
            ///     Indicates the success or failure of the authentication operation
            /// </summary>
            public AuthenticationStatusCode StatusCode { get; set; }

            /// <summary>
            ///     The information elements included in the frame
            /// </summary>
            public InformationElementList InformationElements { get; set; }

            private UInt16 AuthenticationAlgorithmNumberBytes
            {
                get
                {
                    if(this.header.Length >= (AuthenticationFields.AuthAlgorithmNumPosition + AuthenticationFields.AuthAlgorithmNumLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AuthenticationFields.AuthAlgorithmNumPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + AuthenticationFields.AuthAlgorithmNumPosition); }
            }

            private UInt16 AuthenticationAlgorithmTransactionSequenceNumberBytes
            {
                get
                {
                    if(this.header.Length >= (AuthenticationFields.AuthAlgorithmTransactionSequenceNumPosition + AuthenticationFields.AuthAlgorithmTransactionSequenceNumLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AuthenticationFields.AuthAlgorithmTransactionSequenceNumPosition);
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, this.header.Offset + AuthenticationFields.AuthAlgorithmTransactionSequenceNumPosition); }
            }

            private AuthenticationStatusCode StatusCodeBytes
            {
                get
                {
                    if(this.header.Length >= (AuthenticationFields.StatusCodePosition + AuthenticationFields.StatusCodeLength)) {
                        return (AuthenticationStatusCode) EndianBitConverter.Little.ToUInt16(this.header.Bytes, this.header.Offset + AuthenticationFields.StatusCodePosition);
                    }
                    //This seems the most sensible value to return when it is not possible
                    //to extract a meaningful value
                    return AuthenticationStatusCode.UnspecifiedFailure;
                }

                set { EndianBitConverter.Little.CopyBytes((UInt16) value, this.header.Bytes, this.header.Offset + AuthenticationFields.StatusCodePosition); }
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
                this.AuthenticationAlgorithmNumberBytes = this.AuthenticationAlgorithmNumber;
                this.AuthenticationAlgorithmTransactionSequenceNumberBytes = this.AuthenticationAlgorithmTransactionSequenceNumber;
                this.StatusCodeBytes = this.StatusCode;
                //we now know the backing buffer is big enough to contain the info elements so we can safely copy them in
                this.InformationElements.CopyTo(this.header, this.header.Offset + AuthenticationFields.InformationElement1Position);

                this.header.Length = this.FrameSize;
            }

            private class AuthenticationFields
            {
                public static readonly int AuthAlgorithmNumLength = 2;
                public static readonly int AuthAlgorithmTransactionSequenceNumLength = 2;
                public static readonly int StatusCodeLength = 2;
                public static readonly int AuthAlgorithmNumPosition;
                public static readonly int AuthAlgorithmTransactionSequenceNumPosition;
                public static readonly int StatusCodePosition;
                public static readonly int InformationElement1Position;

                static AuthenticationFields()
                {
                    AuthAlgorithmNumPosition = MacFields.SequenceControlPosition + MacFields.SequenceControlLength;
                    AuthAlgorithmTransactionSequenceNumPosition = AuthAlgorithmNumPosition + AuthAlgorithmNumLength;
                    StatusCodePosition = AuthAlgorithmTransactionSequenceNumPosition + AuthAlgorithmTransactionSequenceNumLength;
                    InformationElement1Position = StatusCodePosition + StatusCodeLength;
                }
            }
        }
    }
}