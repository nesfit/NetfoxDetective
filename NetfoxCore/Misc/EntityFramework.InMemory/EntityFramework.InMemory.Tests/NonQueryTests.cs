using System.Linq;
using EntityFramework.InMemory;
using NUnit.Framework;
using TestContext = ClassLibrary1EntityFramework.InMemory.Test.DBContexts.TestContext;

namespace ClassLibrary1EntityFramework.InMemory.Test
{
    [TestFixture]
    public class NonQueryTests
    {
        [Test][Explicit][Category("Explicit")]
        public void CreateBlog_saves_a_blog_via_context()
        {
            var context = new TestContext();

            var service = new BlogService(context);
            service.AddBlog("ADO.NET Blog", "http://blogs.msdn.com/adonet");

            Assert.AreEqual(1, context.Blogs.Count());
            Assert.AreEqual("ADO.NET Blog", context.Blogs.Single().Name);
            Assert.AreEqual("http://blogs.msdn.com/adonet", context.Blogs.Single().Url);
            Assert.AreEqual(1, context.SaveChangesCount);
        }

        [Test][Explicit][Category("Explicit")]
        public void CreateBlog_saves_a_blog_via_InMemoryContext()
        {
            var context = new InheritedDbContext();
            var service = new BlogService(context);
            service.AddBlog("ADO.NET Blog", "http://blogs.msdn.com/adonet");

            Assert.AreEqual(1, context.Blogs.Count());
            Assert.AreEqual("ADO.NET Blog", context.Blogs.Single().Name);
            Assert.AreEqual("http://blogs.msdn.com/adonet", context.Blogs.Single().Url);
            //Assert.AreEqual(1, context.SaveChangesCount);
        }
    }
}