using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using ClassLibrary1EntityFramework.InMemory.Test.Model;

namespace ClassLibrary1EntityFramework.InMemory.Test.DBContexts
{
    public class BloggingContext : DbContext, IBloggingContext
    {
        protected BloggingContext() {}
        protected BloggingContext(DbCompiledModel model) : base(model) {}
        public BloggingContext(string nameOrConnectionString) : base(nameOrConnectionString) {}
        public BloggingContext(string nameOrConnectionString, DbCompiledModel model) : base(nameOrConnectionString, model) {}
        public BloggingContext(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection) {}
        public BloggingContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection) {}
        public BloggingContext(ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext) {}
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
