using System;

namespace PacketDotNet.Ieee80211
{
    namespace Ieee80211
    {
        /// <summary>
        ///     Capability information field.
        /// </summary>
        public class CapabilityInformationField
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="CapabilityInformationField" /> class.
            /// </summary>
            public CapabilityInformationField() { }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="field">
            ///     A <see cref="UInt16" />
            /// </param>
            public CapabilityInformationField(UInt16 field) { this.Field = field; }

            /// <summary>
            ///     Is set to 1 when the beacon frame is representing an ESS (as opposed to an IBSS)
            ///     This field and IsIbss should be mutually exclusive
            /// </summary>
            public bool IsEss
            {
                get { return this.GetBitFieldValue(0); }

                set { this.SetBitFieldValue(0, value); }
            }

            /// <summary>
            ///     Is set to 1 when the beacon frame is representing an IBSS (as opposed to an ESS)
            ///     This field and IsEss should be mutually exclusive
            /// </summary>
            public bool IsIbss
            {
                get { return this.GetBitFieldValue(1); }

                set { this.SetBitFieldValue(1, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> cf pollable.
            /// </summary>
            /// <value>
            ///     <c>true</c> if cf pollable; otherwise, <c>false</c>.
            /// </value>
            public bool CfPollable
            {
                get { return this.GetBitFieldValue(2); }

                set { this.SetBitFieldValue(2, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> cf poll request.
            /// </summary>
            /// <value>
            ///     <c>true</c> if cf poll request; otherwise, <c>false</c>.
            /// </value>
            public bool CfPollRequest
            {
                get { return this.GetBitFieldValue(3); }

                set { this.SetBitFieldValue(3, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> is privacy.
            /// </summary>
            /// <value>
            ///     <c>true</c> if privacy; otherwise, <c>false</c>.
            /// </value>
            public bool Privacy
            {
                get { return this.GetBitFieldValue(4); }

                set { this.SetBitFieldValue(4, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> short preamble.
            /// </summary>
            /// <value>
            ///     <c>true</c> if short preamble; otherwise, <c>false</c>.
            /// </value>
            public bool ShortPreamble
            {
                get { return this.GetBitFieldValue(5); }

                set { this.SetBitFieldValue(5, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> is pbcc.
            /// </summary>
            /// <value>
            ///     <c>true</c> if pbcc; otherwise, <c>false</c>.
            /// </value>
            public bool Pbcc
            {
                get { return this.GetBitFieldValue(6); }

                set { this.SetBitFieldValue(6, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> channel agility.
            /// </summary>
            /// <value>
            ///     <c>true</c> if channel agility; otherwise, <c>false</c>.
            /// </value>
            public bool ChannelAgility
            {
                get { return this.GetBitFieldValue(7); }

                set { this.SetBitFieldValue(7, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> short time slot.
            /// </summary>
            /// <value>
            ///     <c>true</c> if short time slot; otherwise, <c>false</c>.
            /// </value>
            public bool ShortTimeSlot
            {
                get { return this.GetBitFieldValue(10); }

                set { this.SetBitFieldValue(10, value); }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether this
            ///     <see cref="CapabilityInformationField" /> dss ofdm.
            /// </summary>
            /// <value>
            ///     <c>true</c> if dss ofdm; otherwise, <c>false</c>.
            /// </value>
            public bool DssOfdm
            {
                get { return this.GetBitFieldValue(13); }

                set { this.SetBitFieldValue(13, value); }
            }

            /// <summary>
            ///     Gets or sets the field.
            /// </summary>
            /// <value>
            ///     The field.
            /// </value>
            public UInt16 Field { get; set; }

            /// <summary>
            ///     Returns true if the bit is set false if not.
            /// </summary>
            /// <param name="index">0 indexed position of the bit</param>
            private bool GetBitFieldValue(ushort index) { return (((this.Field >> index)&0x1) == 1)? true : false; }

            private void SetBitFieldValue(ushort index, bool value)
            {
                if(value) { this.Field |= unchecked((UInt16) (1 << index)); }
                else
                { this.Field &= unchecked((UInt16) ~(1 << index)); }
            }
        }
    }
}