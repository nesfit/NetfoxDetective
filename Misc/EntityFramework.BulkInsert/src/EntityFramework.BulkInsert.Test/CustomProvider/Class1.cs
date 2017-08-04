using System;
using System.Collections.Generic;
//using System.Data.Entity.SqlServerCompact;
using System.Diagnostics;
using System.Linq;
using EntityFramework.BulkInsert.Extensions;
using EntityFramework.BulkInsert.SqlServerCe;
using EntityFramework.BulkInsert.Test.CodeFirst;
using EntityFramework.BulkInsert.Test.CodeFirst.Domain;
using NUnit.Framework;

namespace EntityFramework.BulkInsert.Test.CustomProvider
{
    [TestFixture]
    public class SqlCeTest : TestBase<SqlCeContext>
    {
        [SetUp]
        public void SetUp()
        {
            ProviderFactory.Register<SqlCeBulkInsertProvider>("System.Data.SqlServerCe.SqlCeConnection");
        }

        private IEnumerable<Item> Items(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return new Item {X = i, Y = count - i};
            }
        }
            
        [Test][Explicit][Category("Explicit")]
        public void Test()
        {
            using (var ctx = new SqlCeContext("SqlCeContext"))
            {
                ctx.Database.Initialize(false);

                for (int i = 0; i < 1000; i++)
                {
                    ctx.Pages.Add(new Page { Content = "pla", CreatedAt = DateTime.Now });
                }
                ctx.SaveChanges();

                var pagescount = ctx.Pages.Count();
                Console.WriteLine(pagescount);
            }
        }


        [Test][Explicit][Category("Explicit")]
        public void BulkInsert()
        {
            using (var ctx = new SqlCeContext("SqlCeContext"))
            {
                var sw = new Stopwatch();
                sw.Start();
                ctx.Database.Initialize(false);
                ctx.BulkInsert(Items(1));
                sw.Stop();
                Console.WriteLine("initialized within {0}ms", sw.Elapsed.TotalMilliseconds);

                sw.Restart();
                ctx.BulkInsert(Items(1000000));
                sw.Stop();
                Console.WriteLine("bulk insert elapsed {0}ms", sw.Elapsed.TotalMilliseconds);

                Console.WriteLine(ctx.Items.Count());
            }
        }

        protected override SqlCeContext GetContext()
        {
            return new SqlCeContext("SqlCeContext");
        }
    }
}
