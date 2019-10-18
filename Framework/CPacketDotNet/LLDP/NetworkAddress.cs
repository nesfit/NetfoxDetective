using System;
using System.Net;
using PacketDotNet.Utils;

namespace PacketDotNet.LLDP
{
    /// <summary>
    ///     A Network Address
    /// </summary>
    public class NetworkAddress
    {
        /// <summary>
        ///     Length of AddressFamily field in bytes
        /// </summary>
        internal const int AddressFamilyLength = 1;

        internal ByteArraySegment data;

        /// <summary>
        ///     Number of bytes in the NetworkAddress
        /// </summary>
        internal int Length
        {
            get { return AddressFamilyLength + this.Address.GetAddressBytes().Length; }
        }

        internal byte[] Bytes
        {
            get
            {
                var addressBytes = this.Address.GetAddressBytes();
                var data = new byte[AddressFamilyLength + addressBytes.Length];
                data[0] = (byte) this.AddressFamily;
                Array.Copy(addressBytes, 0, data, AddressFamilyLength, addressBytes.Length);
                return data;
            }
        }

        #region Constructors
        /// <summary>
        ///     Creates a Network Address entity
        /// </summary>
        /// <param name="address">
        ///     The Network Address
        /// </param>
        public NetworkAddress(IPAddress address) { this.Address = address; }

        /// <summary>
        ///     Create a network address from byte data
        /// </summary>
        /// <param name="bytes">
        ///     A <see cref="System.Byte[]" />
        /// </param>
        /// <param name="offset">
        ///     A <see cref="System.Int32" />
        /// </param>
        /// <param name="length">
        ///     A <see cref="System.Int32" />
        /// </param>
        public NetworkAddress(byte[] bytes, int offset, int length) { this.data = new ByteArraySegment(bytes, offset, length); }
        #endregion

        #region Members
        /// <summary>The format of the Network Address</summary>
        public AddressFamily AddressFamily
        {
            get { return (AddressFamily) this.data.Bytes[this.data.Offset]; }
            set { this.data.Bytes[this.data.Offset] = (byte) value; }
        }

        private static int LengthFromAddressFamily(AddressFamily addressFamily)
        {
            int length;

            if(addressFamily == AddressFamily.IPv4) { length = IPv4Fields.AddressLength; }
            else if(addressFamily == AddressFamily.IPv6) { length = IPv6Fields.AddressLength; }
            else
            { throw new NotImplementedException("Unknown addressFamily of " + addressFamily); }

            return length;
        }

        private static AddressFamily AddressFamilyFromSocketAddress(IPAddress address)
        {
            if(address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) { return AddressFamily.IPv4; }
            return AddressFamily.IPv6;
        }

        /// <summary>The Network Address</summary>
        public IPAddress Address
        {
            get
            {
                var length = LengthFromAddressFamily(this.AddressFamily);
                var bytes = new byte[length];
                Array.Copy(this.data.Bytes, this.data.Offset + AddressFamilyLength, bytes, 0, bytes.Length);

                return new IPAddress(bytes);
            }

            set
            {
                // do we have enough bytes for the address?
                var length = LengthFromAddressFamily(AddressFamilyFromSocketAddress(value));
                length += AddressFamilyLength;

                if((this.data == null) || this.data.Length != length)
                {
                    var bytes = new byte[length];
                    var offset = 0;

                    // allocate enough memory for the new Address
                    this.data = new ByteArraySegment(bytes, offset, length);
                }

                this.AddressFamily = AddressFamilyFromSocketAddress(value);

                var addressBytes = value.GetAddressBytes();
                Array.Copy(addressBytes, 0, this.data.Bytes, this.data.Offset + AddressFamilyLength, addressBytes.Length);
            }
        }

        /// <summary>
        ///     Equals override
        /// </summary>
        /// <param name="obj">
        ///     A <see cref="System.Object" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Boolean" />
        /// </returns>
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if(obj == null || this.GetType() != obj.GetType()) { return false; }

            var na = (NetworkAddress) obj;

            if(this.AddressFamily.Equals(na.AddressFamily) && this.Address.Equals(na.Address)) { return true; }

            return false;
        }

        /// <summary>
        ///     GetHashCode() override
        /// </summary>
        /// <returns>
        ///     A <see cref="System.Int32" />
        /// </returns>
        public override int GetHashCode() { return this.AddressFamily.GetHashCode() + this.Address.GetHashCode(); }

        /// <summary>
        ///     ToString() override
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" />
        /// </returns>
        public override string ToString() { return string.Format("[NetworkAddress: AddressFamily={0}, Address={1}]", this.AddressFamily, this.Address); }
        #endregion
    }
}