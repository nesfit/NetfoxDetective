using Netfox.Core.Helpers;
using NUnit.Framework;

namespace Core.Tests.Helpers
{
    class PathHelperTests
    {
        [Test]
        public void CombineLongPath_PassTwoStrings_ReturnsStringInLongPathFormat()
        {
            var result = PathHelper.CombineLongPath("path\\representing\\path\\number\\one",
                "PathRepresentingPathNumbe\\Two");

            Assert.IsTrue(result.StartsWith(@"\\?\"), $@"Result: {result} does not start with '\\?\'");
        }
    }
}