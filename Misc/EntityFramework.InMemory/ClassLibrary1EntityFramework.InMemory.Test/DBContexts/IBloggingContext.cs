using System.Data.Entity;
using ClassLibrary1EntityFramework.InMemory.Test.Model;

namespace ClassLibrary1EntityFramework.InMemory.Test.DBContexts
{
    public interface IBloggingContext
    {
        DbSet<Blog> Blogs { get; }
        DbSet<Post> Posts { get; }
        int SaveChanges();
    }
}