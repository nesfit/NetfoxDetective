using System;
using System.Net.NetworkInformation;
using PacketDotNet.Utils;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Format of a CTS frame
        /// </summary>
        public class CtsFrame : MacFrame
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="bas">
            ///     A <see cref="ByteArraySegment" />
            /// </param>
            public CtsFrame(ByteArraySegment bas)
            {
                this.header = new ByteArraySegment(bas);

                this.FrameControl = new FrameControlField(this.FrameControlBytes);
                this.Duration = new DurationField(this.DurationBytes);
                this.ReceiverAddress = this.GetAddress(0);

                this.header.Length = this.FrameSize;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="CtsFrame" /> class.
            /// </summary>
            /// <param name='ReceiverAddress'>
            ///     Receiver address.
            /// </param>
            public CtsFrame(PhysicalAddress ReceiverAddress)
            {
                this.FrameControl = new FrameControlField();
                this.Duration = new DurationField();
                this.ReceiverAddress = ReceiverAddress;

                this.FrameControl.SubType = FrameControlField.FrameSubTypes.ControlCTS;
            }

            /// <summary>
            ///     Length of the frame
            /// </summary>
            public override int FrameSize
            {
                get { return (MacFields.FrameControlLength + MacFields.DurationIDLength + MacFields.AddressLength); }
            }

            /// <summary>
            ///     Receiver address
            /// </summary>
            public PhysicalAddress ReceiverAddress { get; set; }

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
                return String.Format("RA {0}", this.ReceiverAddress);
            }
        }
    }
}