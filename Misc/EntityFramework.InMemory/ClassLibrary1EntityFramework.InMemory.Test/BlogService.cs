using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary1EntityFramework.InMemory.Test.DBContexts;
using ClassLibrary1EntityFramework.InMemory.Test.Model;

namespace ClassLibrary1EntityFramework.InMemory.Test
{
    public class BlogService
    {
        private IBloggingContext _context;

        public BlogService(IBloggingContext context)
        {
            this._context = context;
        }

        public Blog AddBlog(string name, string url)
        {
            var blog = new Blog { Name = name, Url = url };
            this._context.Blogs.Add(blog);
            this._context.SaveChanges();

            return blog;
        }

        public List<Blog> GetAllBlogs()
        {
            var query = from b in this._context.Blogs
                orderby b.Name
                select b;

            return query.ToList();
        }

        public async Task<List<Blog>> GetAllBlogsAsync()
        {
            var query = from b in this._context.Blogs
                orderby b.Name
                select b;

            return await query.ToListAsync();
        }
    }
}