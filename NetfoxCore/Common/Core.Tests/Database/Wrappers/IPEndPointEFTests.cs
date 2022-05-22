using Netfox.Core.Database.Wrappers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests.Database.Wrappers
{
    [TestFixture]
    class IPEndPointEFTests
    {
        [Test]
        public void ConstructorTest()
        {
            Assert.AreEqual(8080, new IPEndPointEF(IPAddress.Loopback, 8080).Port);
            Assert.AreEqual(IPAddress.Loopback, new IPEndPointEF(IPAddress.Loopback, 8080).Address.Address);
        }
    }
}
