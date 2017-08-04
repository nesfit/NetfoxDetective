using ClassLibrary1EntityFramework.InMemory.Test.Model;
using EntityFramework.InMemory;
using NUnit.Framework;
using TestContext = ClassLibrary1EntityFramework.InMemory.Test.DBContexts.TestContext;

namespace ClassLibrary1EntityFramework.InMemory.Test
{
    [TestFixture]
    public class QueryTests
    {
        [Test][Explicit][Category("Explicit")]
        public void GetAllBlogs_orders_by_name()
        {
            var context = new TestContext();
            context.Blogs.Add(new Blog { Name = "BBB" });
            context.Blogs.Add(new Blog { Name = "ZZZ" });
            context.Blogs.Add(new Blog { Name = "AAA" });

            var service = new BlogService(context);
            var blogs = service.GetAllBlogs();

            Assert.AreEqual(3, blogs.Count);
            Assert.AreEqual("AAA", blogs[0].Name);
            Assert.AreEqual("BBB", blogs[1].Name);
            Assert.AreEqual("ZZZ", blogs[2].Name);
        }

        [Test][Explicit][Category("Explicit")]
        public void GetAllBlogs_orders_by_name_InMemoryContext()
        {
            var context = new InheritedDbContext();

            context.Blogs.Add(new Blog { Name = "BBB" });
            context.Blogs.Add(new Blog { Name = "ZZZ" });
            context.Blogs.Add(new Blog { Name = "AAA" });

            var service = new BlogService(context);
            var blogs = service.GetAllBlogs();

            Assert.AreEqual(3, blogs.Count);
            Assert.AreEqual("AAA", blogs[0].Name);
            Assert.AreEqual("BBB", blogs[1].Name);
            Assert.AreEqual("ZZZ", blogs[2].Name);
        }
    }
}