using System;
using System.Net.NetworkInformation;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     RTS Frame has a ReceiverAddress[6], TransmitterAddress[6] and a FrameCheckSequence[4],
        ///     these fields follow the common FrameControl[2] and DurationId[2] fields
        /// </summary>
        public class RtsFrame : MacFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public RtsFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.ReceiverAddress = this.GetAddress(0);
                this.TransmitterAddress = this.GetAddress(1);

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Length of the frame
            /// </summary>
            public override int FrameSize
            {
                get { return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*2)); }
            }

            /// <summary>
            ///     ReceiverAddress
            /// </summary>
            public PhysicalAddress ReceiverAddress { get; set; }

            /// <summary>
            ///     TransmitterAddress
            /// </summary>
            public PhysicalAddress TransmitterAddress { get; set; }

            /// <summary>
            ///     Returns a string with a description of the addresses used in the packet.
            ///     This is used as a compoent of the string returned by ToString().
            /// </summary>
            /// <returns>
            ///     The address string.
            /// </returns>
            protected override String GetAddressString() { return String.Format("RA {0} TA {1}", this.ReceiverAddress, this.TransmitterAddress); }
        }
    }
}