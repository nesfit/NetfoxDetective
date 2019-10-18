using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netfox.Core.Helpers;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Netfox.Core.Tests.Helpers
{
    [TestFixture]
    class PathHelperTests
    {
        [Test]
        public void CombineLongPath_PassTwoStrings_ReturnsStringInLongPathFormat()
        {
            var result = PathHelper.CombineLongPath("path\\representing\\path\\number\\one", "PathRepresentingPathNumbe\\Two");

            Assert.IsTrue(result.StartsWith(@"\\?\"),$@"Result: {result} does not start with '\\?\'");
        }
    }
}
