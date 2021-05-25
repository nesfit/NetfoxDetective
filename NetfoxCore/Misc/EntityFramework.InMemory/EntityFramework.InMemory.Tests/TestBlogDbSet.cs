using System;
using System.Collections.Generic;
using System.Linq;
using ClassLibrary1EntityFramework.InMemory.Test.Model;
using EntityFramework.InMemory;

namespace ClassLibrary1EntityFramework.InMemory.Test
{
    class TestBlogDbSet : TestDbSet<Blog>
    {
        public TestBlogDbSet(Action<Blog> addCallback, Action<IEnumerable<Blog>> addBulkCallback, Action<Blog> removeCallback) : base(addCallback, addBulkCallback,removeCallback) {}
        public TestBlogDbSet(Action<Blog> addCallback, Action<IEnumerable<Blog>> addBulkCallback, Action<Blog> removeCallback, TestDbSet dbSet = null) : base(addCallback, addBulkCallback, removeCallback, dbSet) {}

        public override Blog Find(params object[] keyValues)
        {
            var id = (int)keyValues.Single();
            return this.SingleOrDefault(b => b.BlogId == id);
        }
    }
}