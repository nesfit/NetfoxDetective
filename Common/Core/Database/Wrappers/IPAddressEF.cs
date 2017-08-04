// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.Sockets;
using Netfox.Core.Extensions;

namespace Netfox.Core.Database.Wrappers
{
    [ComplexType]
    public class IPAddressEF : IPAddress
    {
        private byte[] _addressData;

        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 16;
        internal const string dns_bad_ip_address = "dns_bad_ip_address";
        internal const int NumberOfLabels = IPv6AddressBytes / 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.IPAddress"/> class with the address specified as an <see cref="T:System.Int64"/>.
        /// </summary>
        /// <param name="newAddress">The long value of the IP address. For example, the value 0x2414188f in big-endian format would be the IP address "143.24.20.36". </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="newAddress"/> &lt; 0 or <paramref name="newAddress"/> &gt; 0x00000000FFFFFFFF </exception>
        public IPAddressEF(long newAddress) : base(newAddress) { this.AddressData = this.GetAddressBytes(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.IPAddress"/> class with the address specified as a <see cref="T:System.Byte"/> array and the specified scope identifier.
        /// </summary>
        /// <param name="address">The byte array value of the IP address. </param><param name="scopeid">The long value of the scope identifier. </param><exception cref="T:System.ArgumentNullException"><paramref name="address"/> is null. </exception><exception cref="T:System.ArgumentException"><paramref name="address"/> contains a bad IP address. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="scopeid"/> &lt; 0 or <paramref name="scopeid"/> &gt; 0x00000000FFFFFFFF </exception>
        public IPAddressEF(byte[] address, long scopeid) : base(address, scopeid) { this.AddressData = this.GetAddressBytes(); }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.IPAddress"/> class with the address specified as a <see cref="T:System.Byte"/> array.
        /// </summary>
        /// <param name="address">The byte array value of the IP address. </param><exception cref="T:System.ArgumentNullException"><paramref name="address"/> is null. </exception><exception cref="T:System.ArgumentException"><paramref name="address"/> contains a bad IP address. </exception>
        public IPAddressEF(byte[] address) : base(address) { this.AddressData = address; }

        public IPAddressEF() : base(0){}

        public IPAddressEF(IPAddress ipAddress) : base(ipAddress.GetAddressBytes()) { this.AddressData = ipAddress.GetAddressBytes(); }

        [MaxLength(16)]
        public byte[] AddressData
        {
            get { return this._addressData; }
            private set
            {
                this._addressData = value;

                AddressFamily m_Family;
                Int64 m_Address = 0;
                var m_Numbers = new ushort[NumberOfLabels];
                if (this._addressData == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (this._addressData.Length != IPv4AddressBytes && this._addressData.Length != IPv6AddressBytes)
                {
                    throw new ArgumentException(dns_bad_ip_address, nameof(value));
                }

                if (this._addressData.Length == IPv4AddressBytes)
                {
                    m_Family = AddressFamily.InterNetwork;
                    m_Address = ((this._addressData[3] << 24 | this._addressData[2] << 16 | this._addressData[1] << 8 | this._addressData[0]) & 0x0FFFFFFFF);
                    this.SetPrivateFieldValue("m_Address", m_Address);
                    this.ScopeId = (long) ProtocolFamily.InterNetwork;
                }
                else
                {
                    m_Family = AddressFamily.InterNetworkV6;
                    for (var i = 0; i < NumberOfLabels; i++)
                    {
                        m_Numbers[i] = (ushort)(this._addressData[i * 2] * 256 + this._addressData[i * 2 + 1]);
                    }
                    this.SetPrivateFieldValue("m_Numbers", m_Numbers);
                    this.ScopeId = (long)ProtocolFamily.InterNetworkV6;
                }
                this.SetPrivateFieldValue("m_Family", m_Family);
            }
        }

        [NotMapped]
        public new long ScopeId { get; set; }
        [NotMapped]
        public new long Address { get; set; }

    }
}