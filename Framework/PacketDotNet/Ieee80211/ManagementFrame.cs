using System;
using System.Net.NetworkInformation;
using PacketDotNet.MiscUtil.Conversion;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Format of a CTS or an ACK frame
        /// </summary>
        public abstract class ManagementFrame : MacFrame
        {
            /// <summary>
            ///     DestinationAddress
            /// </summary>
            public PhysicalAddress DestinationAddress { get; set; }

            /// <summary>
            ///     SourceAddress
            /// </summary>
            public PhysicalAddress SourceAddress { get; set; }

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
            ///     Returns a string with a description of the addresses used in the packet.
            ///     This is used as a compoent of the string returned by ToString().
            /// </summary>
            /// <returns>
            ///     The address string.
            /// </returns>
            protected override String GetAddressString()
            {
                return String.Format("SA {0} DA {1} BSSID {2}", this.SourceAddress, this.DestinationAddress, this.BssId);
            }
        }
    }
}