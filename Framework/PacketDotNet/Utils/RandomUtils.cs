using System;
using System.Collections.Generic;
using System.Net;

namespace PacketDotNet.Utils
{
    /// <summary>
    ///     Random utility methods
    /// </summary>
    public class RandomUtils
    {
        /// <summary>
        ///     Generate a random ip address
        /// </summary>
        /// <param name="version">
        ///     A <see cref="IpVersion" />
        /// </param>
        /// <returns>
        ///     A <see cref="System.Net.IPAddress" />
        /// </returns>
        public static IPAddress GetIPAddress(IpVersion version)
        {
            var rnd = new Random();
            byte[] randomAddressBytes;

            if(version == IpVersion.IPv4)
            {
                randomAddressBytes = new byte[IPv4Fields.AddressLength];
                rnd.NextBytes(randomAddressBytes);
            }
            else if(version == IpVersion.IPv6)
            {
                randomAddressBytes = new byte[IPv6Fields.AddressLength];
                rnd.NextBytes(randomAddressBytes);
            }
            else
            { throw new InvalidOperationException("Unknown version of " + version); }

            return new IPAddress(randomAddressBytes);
        }

        /// <summary>
        ///     Get the length of the longest string in a list of strings
        /// </summary>
        /// <param name="stringsList">
        ///     A <see cref="List<System.String>"/>
        /// </param>
        /// <returns>
        ///     A <see cref="System.Int32" />
        /// </returns>
        public static int LongestStringLength(List<string> stringsList)
        {
            var longest = "";

            foreach(var L in stringsList) { if(L.Length > longest.Length) { longest = L; } }
            return longest.Length;
        }
    }
}