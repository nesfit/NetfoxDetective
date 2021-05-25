using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Newtonsoft.Json;

namespace Netfox.Core.Database.Wrappers
{
    [ComplexType]
    [JsonConverter(typeof(IpEfConverter))]
    public class IPAddressEF
    {
        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 16;
        internal const string dns_bad_ip_address = "dns_bad_ip_address";
        internal const int NumberOfLabels = IPv6AddressBytes / 2;

        [Required, MinLength(4), MaxLength(16)]
        public byte[] AddressBytes { get; set; }

        [NotMapped]
        public IPAddress Address { get => new IPAddress(AddressBytes); set => AddressBytes = value.GetAddressBytes(); }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as an
        ///     <see cref="T:System.Int64" />.
        /// </summary>
        /// <param name="newAddress">
        ///     The long value of the IP address. For example, the value 0x2414188f in big-endian format would
        ///     be the IP address "143.24.20.36".
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="newAddress" /> &lt; 0 or
        ///     <paramref name="newAddress" /> &gt; 0x00000000FFFFFFFF
        /// </exception>
        public IPAddressEF(long newAddress)
        {
            Address = new IPAddress(newAddress);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as a
        ///     <see cref="T:System.Byte" /> array and the specified scope identifier.
        /// </summary>
        /// <param name="address">The byte array value of the IP address. </param>
        /// <param name="scopeid">The long value of the scope identifier. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="address" /> is null. </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="address" /> contains a bad IP address. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="scopeid" /> &lt; 0 or
        ///     <paramref name="scopeid" /> &gt; 0x00000000FFFFFFFF
        /// </exception>
        public IPAddressEF(byte[] address, long scopeid)
        {
            Address = new IPAddress(address, scopeid);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Net.IPAddress" /> class with the address specified as a
        ///     <see cref="T:System.Byte" /> array.
        /// </summary>
        /// <param name="address">The byte array value of the IP address. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="address" /> is null. </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="address" /> contains a bad IP address. </exception>
        public IPAddressEF(byte[] address)
        {
            Address = new IPAddress(address);
        }

        public IPAddressEF()
        {
            Address = new IPAddress(0);
        }

        public IPAddressEF(IPAddress ipAddress)
        {
            Address = ipAddress;
        }
    }
}