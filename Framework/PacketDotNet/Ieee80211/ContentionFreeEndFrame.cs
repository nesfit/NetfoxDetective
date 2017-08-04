using System;
using System.Net.NetworkInformation;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Contention free end frame.
        /// </summary>
        public class ContentionFreeEndFrame : MacFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public ContentionFreeEndFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.ReceiverAddress = this.GetAddress(0);
                this.BssId = this.GetAddress(1);

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="ContentionFreeEndFrame" /> class.
            /// </summary>
            /// <param name='ReceiverAddress'>
            ///     Receiver address.
            /// </param>
            /// <param name='BssId'>
            ///     Bss identifier (MAC Address of the Access Point).
            /// </param>
            public ContentionFreeEndFrame(PhysicalAddress ReceiverAddress, PhysicalAddress BssId)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.ReceiverAddress = ReceiverAddress;
                this.BssId = BssId;

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ControlCFEnd;
            }

            /// <summary>
            ///     Length of the frame
            /// </summary>
            public override int FrameSize
            {
                get { return (MacFields.FrameControlLength + MacFields.DurationIDLength + (MacFields.AddressLength*2)); }
            }

            /// <summary>
            ///     Receiver address
            /// </summary>
            public PhysicalAddress ReceiverAddress { get; set; }

            /// <summary>
            ///     BSS ID
            /// </summary>
            public PhysicalAddress BssId { get; set; }

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
                this.SetAddress(0, this.ReceiverAddress);
                this.SetAddress(1, this.BssId);

                this.header.Length = this.FrameSize;
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
                return String.Format("RA {0} BSSID {1}", this.ReceiverAddress, this.BssId);
            }
        }
    }
}