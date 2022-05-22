﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Netfox.Core.Database.Wrappers
{
    [ComplexType]
    public class IPEndPointEF : IPEndPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.IPEndPoint"/> class with the specified address and port number.
        /// </summary>
        /// <param name="address">The IP address of the Internet host. </param><param name="port">The port number associated with the <paramref name="address"/>, or 0 to specify any available port. <paramref name="port"/> is in host order.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is less than <see cref="F:System.Net.IPEndPoint.MinPort"/>.-or- <paramref name="port"/> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort"/>.-or- <paramref name="address"/> is less than 0 or greater than 0x00000000FFFFFFFF. </exception>
        public IPEndPointEF(long address, int port) : base(address, port)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.IPEndPoint"/> class with the specified address and port number.
        /// </summary>
        /// <param name="address">An <see cref="T:System.Net.IPAddress"/>. </param><param name="port">The port number associated with the <paramref name="address"/>, or 0 to specify any available port. <paramref name="port"/> is in host order.</param><exception cref="T:System.ArgumentNullException"><paramref name="address"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is less than <see cref="F:System.Net.IPEndPoint.MinPort"/>.-or- <paramref name="port"/> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort"/>.-or- <paramref name="address"/> is less than 0 or greater than 0x00000000FFFFFFFF. </exception>
        public IPEndPointEF(IPAddress address, int port) : base(address, port)
        {
        }

        public IPEndPointEF() : base(0, 0)
        {
        }

        public IPEndPointEF(IPEndPoint ipEndPoint) : base(ipEndPoint.Address, ipEndPoint.Port)
        {
        }

        public new IPAddressEF Address
        {
            get
            {
                return new IPAddressEF(base.Address);
            }
            set { base.Address = value.Address; }
        }
    }
}