using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Data frame.
        /// </summary>
        public abstract class DataFrame : MacFrame
        {
            /// <summary>
            ///     SourceAddress
            /// </summary>
            public PhysicalAddress SourceAddress { get; set; }

            /// <summary>
            ///     DestinationAddress
            /// </summary>
            public PhysicalAddress DestinationAddress { get; set; }

            /// <summary>
            ///     ReceiverAddress
            /// </summary>
            public PhysicalAddress ReceiverAddress { get; set; }

            /// <summary>
            ///     TransmitterAddress
            /// </summary>
            public PhysicalAddress TransmitterAddress { get; set; }

            /// <summary>
            ///     BssID
            /// </summary>
            public PhysicalAddress BssId { get; set; }

            /// <summary>
            ///     Sequence control field
            /// </summary>
            public SequenceControlField SequenceControl { get; set; }

            /// <summary>
            ///     Frame control bytes are the first two bytes of the frame
            /// </summary>
            protected UInt16 SequenceControlBytes
            {
                get
                {
                    if(this.header.Length >= (MacFields.SequenceControlPosition + MacFields.SequenceControlLength)) {
                        return EndianBitConverter.Little.ToUInt16(this.header.Bytes, (this.header.Offset + MacFields.Address1Position + (MacFields.AddressLength*3)));
                    }
                    return 0;
                }

                set { EndianBitConverter.Little.CopyBytes(value, this.header.Bytes, (this.header.Offset + MacFields.Address1Position + (MacFields.AddressLength*3))); }
            }

            /// <summary>
            ///     Assigns the default MAC address of 00-00-00-00-00-00 to all address fields.
            /// </summary>
            protected void AssignDefaultAddresses()
            {
                var zeroAddress = PhysicalAddress.Parse("000000000000");

                this.SourceAddress = zeroAddress;
                this.DestinationAddress = zeroAddress;
                this.TransmitterAddress = zeroAddress;
                this.ReceiverAddress = zeroAddress;
                this.BssId = zeroAddress;
            }

            /// <summary>
            ///     Returns a string with a description of the addresses used in the packet.
            ///     This is used as a compoent of the string returned by ToString().
            /// </summary>
            /// <returns>
            ///     The address string.
            /// </returns>
            protected override String GetAddressString()
            {
                String addresses = null;
                if(this.FrameControl.ToDS && this.FrameControl.FromDS) {
                    addresses = String.Format("SA {0} DA {1} TA {2} RA {3}", this.SourceAddress, this.DestinationAddress, this.TransmitterAddress, this.ReceiverAddress);
                }
                else
                {
                    addresses = String.Format("SA {0} DA {1} BSSID {2}", this.SourceAddress, this.DestinationAddress, this.BssId);
                }
                return addresses;
            }

            /// <summary>
            ///     Reads the addresses from the backing ByteArraySegment into the the address properties.
            /// </summary>
            /// <remarks>
            ///     The <see cref="FrameControlField" /> ToDS and FromDS properties dictate
            ///     which of the 4 possible address fields is read into which address property.
            /// </remarks>
            protected void ReadAddresses()
            {
                if((!this.FrameControl.ToDS) && (!this.FrameControl.FromDS))
                {
                    this.DestinationAddress = this.GetAddress(0);
                    this.SourceAddress = this.GetAddress(1);
                    this.BssId = this.GetAddress(2);
                }
                else if((this.FrameControl.ToDS) && (!this.FrameControl.FromDS))
                {
                    this.BssId = this.GetAddress(0);
                    this.SourceAddress = this.GetAddress(1);
                    this.DestinationAddress = this.GetAddress(2);
                }
                else if((!this.FrameControl.ToDS) && (this.FrameControl.FromDS))
                {
                    this.DestinationAddress = this.GetAddress(0);
                    this.BssId = this.GetAddress(1);
                    this.SourceAddress = this.GetAddress(2);
                }
                else
                {
                    //both are true so we are in WDS mode again. BSSID is not valid in this mode
                    this.ReceiverAddress = this.GetAddress(0);
                    this.TransmitterAddress = this.GetAddress(1);
                    this.DestinationAddress = this.GetAddress(2);
                    this.SourceAddress = this.GetAddress(3);
                }
            }

            /// <summary>
            ///     Writes the address properties into the backing <see cref="PacketDotNet.Utils.ByteArraySegment" />.
            /// </summary>
            /// <remarks>
            ///     The address position into which a particular address property is written is determined by the
            ///     value of <see cref="FrameControlField" /> ToDS and FromDS properties.
            /// </remarks>
            protected void WriteAddressBytes()
            {
                if((!this.FrameControl.ToDS) && (!this.FrameControl.FromDS))
                {
                    this.SetAddress(0, this.DestinationAddress);
                    this.SetAddress(1, this.SourceAddress);
                    this.SetAddress(2, this.BssId);
                }
                else if((this.FrameControl.ToDS) && (!this.FrameControl.FromDS))
                {
                    this.SetAddress(0, this.BssId);
                    this.SetAddress(1, this.SourceAddress);
                    this.SetAddress(2, this.DestinationAddress);
                }
                else if((!this.FrameControl.ToDS) && (this.FrameControl.FromDS))
                {
                    this.SetAddress(0, this.DestinationAddress);
                    this.SetAddress(1, this.BssId);
                    this.SetAddress(2, this.SourceAddress);
                }
                else
                {
                    this.SetAddress(0, this.ReceiverAddress);
                    this.SetAddress(1, this.TransmitterAddress);
                    this.SetAddress(2, this.DestinationAddress);
                    this.SetAddress(3, this.SourceAddress);
                }
            }
        }
    }
}