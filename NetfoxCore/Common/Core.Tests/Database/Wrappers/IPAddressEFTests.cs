using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Netfox.Core.Database.Wrappers;
using NUnit.Framework;

namespace Core.Tests.Database.Wrappers
{
    [TestFixture]
    class IPAddressEFTests
    {
        [Test]
        public void ConstructorTests()
        {
            Assert.AreEqual(new byte[] { 192, 168, 1, 1 }, new IPAddressEF(0x0101A8C0).AddressBytes);
            Assert.AreEqual(new byte[] { 192, 168, 1, 1 }, new IPAddressEF(new byte[] { 192, 168, 1, 1 }).AddressBytes);
            Assert.AreEqual(new byte[] { 127, 0, 0, 1 }, new IPAddressEF(IPAddress.Loopback).AddressBytes);

            Assert.AreEqual(IPAddress.IPv6Loopback, new IPAddressEF(IPAddress.IPv6Loopback).Address);
            Assert.AreEqual(IPAddress.Any, new IPAddressEF().Address);
        }
    }
}
