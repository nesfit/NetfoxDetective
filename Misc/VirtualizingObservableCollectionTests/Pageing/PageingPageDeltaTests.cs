using System;
using AlphaChiTech.Virtualization.Pageing;
using NUnit.Framework;

namespace VirtualizingObservableCollectionTests.Pageing
{
    [TestFixture]
    class PageingPageDeltaTests
    {
        [Test]
        public void setPage_ValidPageNumber_ValidPageNumber()
        {
            var testingPageDelta = new PageDelta();
            testingPageDelta.Page = 10;

            Assert.AreEqual(10, testingPageDelta.Page);
        }

        [Test]
        public void setPage_FirstPageZeroNumber_0()
        {
            var testingPageDelta = new PageDelta();
            testingPageDelta.Page = 0;

            Assert.AreEqual(0, testingPageDelta.Page);
        }

        [Test]
        public void setPage_InvalidPageNumber_OutOFRangeException()
        {
            var testingPageDelta = new PageDelta();
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => testingPageDelta.Page = -4);
        }

    }
}
