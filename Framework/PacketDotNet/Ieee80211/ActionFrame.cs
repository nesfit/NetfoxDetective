using System;
using System.Net.NetworkInformation;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {

        #region Action Category Enums

        //The following enums define the category and type of action. At present these are 
        //not handled and parsed but they are left here for future reference as tracking them down 
        //was not that easy

        //enum ActionCategory
        //{
        //    SpectrumManagement = 0x0,
        //    Qos = 0x1,
        //    Dls = 0x2,
        //    BlockAck = 0x3,
        //    VendorSpecific = 0x127
        //}

        //enum SpectrumManagementAction
        //{
        //    MeasurementRequest = 0x0,
        //    MeasurementReport = 0x1,
        //    TpcRequest = 0x2,
        //    TpcReport = 0x3,
        //    ChannelSwitchAnnouncement = 0x4
        //}

        //enum QosAction
        //{
        //    TrafficSpecificationRequest = 0x0,
        //    TrafficSpecificationResponse = 0x1,
        //    TrafficSpecificationDelete = 0x2,
        //    Schedule = 0x3
        //}

        //enum DlsAction
        //{
        //    DlsRequest = 0x0,
        //    DlsResponse = 0x1,
        //    DlsTeardown = 0x2
        //}

        //enum BlockAcknowledgmentActions
        //{
        //    BlockAcknowledgmentRequest = 0x0,
        //    BlockAcknowledgmentResponse = 0x1,
        //    BlockAcknowledgmentDelete = 0x2
        //}
        #endregion

        /// <summary>
        ///     Format of an 802.11 management action frame. These frames are used by the 802.11e (QoS) and 802.11n standards to
        ///     request actions of stations.
        /// </summary>
        public class ActionFrame : ManagementFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public ActionFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.DestinationAddress = this.GetAddress(0);
                this.SourceAddress = this.GetAddress(1);
                this.BssId = this.GetAddress(2);
                this.SequenceControl = new SequenceControlField(this.SequenceControlBytes);

                this.header.Length = this.FrameSize;
                var availablePayloadLength = this.GetAvailablePayloadLength();
                if(availablePayloadLength > 0) { this.payloadPacketOrData.TheByteArraySegment = this.header.EncapsulatedBytes(availablePayloadLength); }
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="ActionFrame" /> class.
            /// </summary>
            /// <param name='SourceAddress'>
            ///     Source address.
            /// </param>
            /// <param name='DestinationAddress'>
            ///     Destination address.
            /// </param>
            /// <param name='BssId'>
            ///     Bss identifier.
            /// </param>
            public ActionFrame(PhysicalAddress SourceAddress, PhysicalAddress DestinationAddress, PhysicalAddress BssId)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.DestinationAddress = DestinationAddress;
                this.SourceAddress = SourceAddress;
                this.BssId = BssId;
                this.SequenceControl = new SequenceControlField();

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ManagementAction;
            }

            /// <summary>
            ///     Gets the size of the frame in bytes
            /// </summary>
            /// <value>
            ///     The size of the frame.
            /// </value>
            public override int FrameSize
            {
                get { return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*3) + MacFields.SequenceControlLength); }
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
            }
        }
    }
}