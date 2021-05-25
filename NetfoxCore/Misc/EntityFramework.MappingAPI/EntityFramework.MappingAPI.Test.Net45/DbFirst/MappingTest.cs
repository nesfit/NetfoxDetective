using System;
using System.Diagnostics;
using EntityFramework.MappingAPI.Extensions;
using EntityFramework.MappingAPI.Test.CodeFirst;
using NUnit.Framework;

namespace EntityFramework.MappingAPI.Test.DbFirst
{
    [TestFixture]
    public class MappingTest
    {
        protected const int NvarcharMax = 1073741823;

        public efmapping_testEntities GetContext()
        {
            return new efmapping_testEntities();
        }

        [Test][Explicit][Category("Explicit")]
        public void TableNames()
        {

            using (var ctx = GetContext())
            {
                var sw = new Stopwatch();
                sw.Restart();
                var dbmapping = ctx.Db();
                sw.Start();

                Console.WriteLine("Mapping took: {0}ms", sw.Elapsed.TotalMilliseconds);

                foreach (var tableMapping in dbmapping)
                {
                    Console.WriteLine("{0}: {1}.{2}", tableMapping.Type.FullName, tableMapping.Schema, tableMapping.TableName);
                }

                Assert.AreEqual(ctx.Db<Blogs>().TableName, "Blogs");
                Assert.AreEqual(ctx.Db<Blogs>().Schema, "dbo");

                Assert.AreEqual(ctx.Db<Post>().TableName, "Posts");
                Assert.AreEqual(ctx.Db<Post>().Schema, "dbo");
            }
        }


        [Test][Explicit][Category("Explicit")]
        public void Entity_Blogs()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<Blogs>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.BlogId)
                    .HasColumnName("BlogId")
                    .IsPk()
                    .IsFk(false)
                    .IsIdentity()
                    .IsNavigationProperty(false);

                map.Prop(x => x.Name)
                    .HasColumnName("Name")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired(false)
                    .IsNavigationProperty(false)
                    .MaxLength(200)
                    .FixedLength(false)
                    .Unicode();

                map.Prop(x => x.Url)
                    .HasColumnName("Url")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired(false)
                    .IsNavigationProperty(false)
                    .MaxLength(200)
                    .FixedLength(false)
                    .Unicode();

            }
        }


        [Test][Explicit][Category("Explicit")]
        public void Entity_Posts()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<Post>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.PostId)
                    .HasColumnName("PostId")
                    .IsPk()
                    .IsFk(false)
                    .IsIdentity()
                    .IsNavigationProperty(false);

                map.Prop(x => x.Title)
                    .HasColumnName("Title")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired(false)
                    .IsNavigationProperty(false)
                    .MaxLength(200)
                    .FixedLength(false)
                    .Unicode();

                map.Prop(x => x.Content)
                    .HasColumnName("Content")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax)
                    .FixedLength(false)
                    .Unicode();

                map.Prop(x => x.BlogId)
                    .IsFk()
                    .IsRequired()
                    .NavigationPropertyName("Blogs")
                    .NavigationProperty(map.Prop(x => x.Blogs));

                map.Prop(x => x.Blogs)
                    .IsFk(false)
                    .ForeignKey(map.Prop(x => x.BlogId))
                    .ForeignKeyPropertyName("BlogId");
            }
        }


    }

}
