using System.Data.Entity;
using ClassLibrary1EntityFramework.InMemory.Test.Model;
using EntityFramework.InMemory;

namespace ClassLibrary1EntityFramework.InMemory.Test.DBContexts
{
    public class TestContext : IBloggingContext
    {
        public TestContext()
        {
            this.Blogs = new TestDbSet<Blog>(null,null,null);
            this.Posts = new TestDbSet<Post>(null,null, null);
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public int SaveChangesCount { get; private set; }
        public int SaveChanges()
        {
            this.SaveChangesCount++;
            return 1;
        }
    }
}