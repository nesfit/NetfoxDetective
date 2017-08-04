using ClassLibrary1EntityFramework.InMemory.Test.Model;
using NUnit.Framework;

namespace ClassLibrary1EntityFramework.InMemory.Test
{
    [TestFixture]
    public sealed class InheritedDbContextTests
    {
        [Test][Explicit][Category("Explicit")]
        public void SetGenericTest()
        {
            using(var dbx = new InheritedDbContext())
            {
                Assert.IsTrue(dbx.Blogs.GetType() == dbx.Set<Blog>().GetType());
            }
        }
        [Test][Explicit][Category("Explicit")]
        public void SetTest()
        {
            using (var dbx = new InheritedDbContext())
            {
                Assert.IsTrue(dbx.Blogs.GetType() == dbx.Set(typeof(Blog)).GetType());
            }
        }
    }
}