using System.Collections.Generic;

namespace ClassLibrary1EntityFramework.InMemory.Test.Model
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public virtual List<Post> Posts { get; set; }
    }
}