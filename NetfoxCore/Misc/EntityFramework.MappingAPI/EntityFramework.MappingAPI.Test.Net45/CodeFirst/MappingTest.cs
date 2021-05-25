using System;
using System.Diagnostics;
using EntityFramework.MappingAPI.Extensions;
using EntityFramework.MappingAPI.Test.CodeFirst.Domain;
using NUnit.Framework;

namespace EntityFramework.MappingAPI.Test.CodeFirst
{
    [TestFixture]
    public class MappingTest : TestBase
    {
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
                    Console.WriteLine("{0}: {1}.{2}", tableMapping.Type.FullName, tableMapping.Schema,
                        tableMapping.TableName);
                }

                Assert.AreEqual(ctx.Db<Page>().TableName, "Pages");
                Assert.AreEqual(ctx.Db<Page>().Schema, "dbo");
                Assert.AreEqual(ctx.Db<PageTranslations>().TableName, "PageTranslations");

                Assert.AreEqual(ctx.Db<TestUser>().TableName, "Users");

                Assert.AreEqual(ctx.Db<MeteringPoint>().TableName, "MeteringPoints");

                Assert.AreEqual(ctx.Db<EmployeeTPH>().TableName, "Employees");
                Assert.AreEqual(ctx.Db<AWorkerTPH>().TableName, "Employees");
                Assert.AreEqual(ctx.Db<ManagerTPH>().TableName, "Employees");

                Assert.AreEqual(ctx.Db<ContractBase>().TableName, "Contracts");
                Assert.AreEqual(ctx.Db<Contract>().TableName, "Contracts");
                Assert.AreEqual(ctx.Db<ContractFixed>().TableName, "Contracts");
                Assert.AreEqual(ctx.Db<ContractStock>().TableName, "Contracts");
                Assert.AreEqual(ctx.Db<ContractKomb1>().TableName, "Contracts");
                Assert.AreEqual(ctx.Db<ContractKomb2>().TableName, "Contracts");

                Assert.AreEqual(ctx.Db<WorkerTPT>().TableName, "WorkerTPTs");
                Assert.AreEqual(ctx.Db<ManagerTPT>().TableName, "ManagerTPTs");

                Assert.AreEqual(ctx.Db<Foo>().TableName, "FOO");
                Assert.AreEqual(ctx.Db<Foo>().Schema, "dbx");
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_WithMappedPk()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<EntityWithMappedPk>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.BancoId)
                    .HasColumnName("BancoID")
                    .IsPk()
                    .IsFk(false)
                    .IsRequired()
                    .IsIdentity(false)
                    .IsNavigationProperty(false);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_ComplexType()
        {
            using (var ctx = new TestContext())
            {
                var map = ctx.Db<TestUser>();

                map.Prop(x => x.Id)
                    .HasColumnName("Id")
                    .IsPk()
                    .IsFk(false)
                    .IsNavigationProperty(false);

                map.Prop(x => x.FirstName)
                    .HasColumnName("Name")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.LastName)
                    .HasColumnName("LastName")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.PhoneNumber)
                    .HasColumnName("Contact_PhoneNumber")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.Country)
                    .HasColumnName("Contact_Address_Country")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.County)
                    .HasColumnName("Contact_Address_County")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.City)
                    .HasColumnName("Contact_Address_City")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.PostalCode)
                    .HasColumnName("Contact_Address_PostalCode")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.StreetAddress)
                    .HasColumnName("Contact_Address_StreetAddress")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);
#if !NET40
                var propertyPropertyMap = map.Prop(x => x.Contact.Address.Location);
                propertyPropertyMap
                    .HasColumnName("Contact_Address_Location")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false);

                Console.WriteLine(propertyPropertyMap.DefaultValue);
                Console.WriteLine(propertyPropertyMap.FixedLength);
                Console.WriteLine(propertyPropertyMap.MaxLength);
                Console.WriteLine(propertyPropertyMap.Precision);
                Console.WriteLine(propertyPropertyMap.Scale);
                Console.WriteLine(propertyPropertyMap.Type);
                Console.WriteLine(propertyPropertyMap.Unicode);

                var shapePropertyMap = map.Prop(x => x.Contact.Address.Shape);
                shapePropertyMap
                    .HasColumnName("Contact_Address_Shape")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false);

                Console.WriteLine(shapePropertyMap.DefaultValue);
                Console.WriteLine(shapePropertyMap.FixedLength);
                Console.WriteLine(shapePropertyMap.MaxLength);
                Console.WriteLine(shapePropertyMap.Precision);
                Console.WriteLine(shapePropertyMap.Scale);
                Console.WriteLine(shapePropertyMap.Type);
                Console.WriteLine(shapePropertyMap.Unicode);
#endif
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_ComplextType_WhereComplexTypeIsLastProperty()
        {
            using (var ctx = new TestContext())
            {
                var map = ctx.Db<House>();

                map.Prop(x => x.Name)
                    .HasColumnName("Name");

                map.Prop(x => x.Address.City)
                    .HasColumnName("Address_City");
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_ComplextType_WhereComplexIsInsideTypeAndAlsoIsLastProperty()
        {
            using (var ctx = new TestContext())
            {
                var map = ctx.Db<TestUserWithSecondAddress>();


                map.Prop(x => x.Id)
                    .HasColumnName("Id")
                    .IsPk()
                    .IsFk(false)
                    .IsNavigationProperty(false);

                map.Prop(x => x.FirstName)
                    .HasColumnName("Name")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.LastName)
                    .HasColumnName("LastName")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.PhoneNumber)
                    .HasColumnName("Contact_PhoneNumber")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.Country)
                    .HasColumnName("Contact_Address_Country")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.County)
                    .HasColumnName("Contact_Address_County")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.City)
                    .HasColumnName("Contact_Address_City")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.PostalCode)
                    .HasColumnName("Contact_Address_PostalCode")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Contact.Address.StreetAddress)
                    .HasColumnName("Contact_Address_StreetAddress")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);
#if !NET40
                var propertyPropertyMap = map.Prop(x => x.Contact.Address.Location);
                propertyPropertyMap
                    .HasColumnName("Contact_Address_Location")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false);

                Console.WriteLine(propertyPropertyMap.DefaultValue);
                Console.WriteLine(propertyPropertyMap.FixedLength);
                Console.WriteLine(propertyPropertyMap.MaxLength);
                Console.WriteLine(propertyPropertyMap.Precision);
                Console.WriteLine(propertyPropertyMap.Scale);
                Console.WriteLine(propertyPropertyMap.Type);
                Console.WriteLine(propertyPropertyMap.Unicode);

                var shapePropertyMap = map.Prop(x => x.Contact.Address.Shape);
                shapePropertyMap
                    .HasColumnName("Contact_Address_Shape")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false);

                Console.WriteLine(shapePropertyMap.DefaultValue);
                Console.WriteLine(shapePropertyMap.FixedLength);
                Console.WriteLine(shapePropertyMap.MaxLength);
                Console.WriteLine(shapePropertyMap.Precision);
                Console.WriteLine(shapePropertyMap.Scale);
                Console.WriteLine(shapePropertyMap.Type);
                Console.WriteLine(shapePropertyMap.Unicode);
#endif
                map.Prop(x => x.Address.Country)
                    .HasColumnName("Address_Country")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Address.County)
                    .HasColumnName("Address_County")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Address.City)
                    .HasColumnName("Address_City")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Address.PostalCode)
                    .HasColumnName("Address_PostalCode")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Address.StreetAddress)
                    .HasColumnName("Address_StreetAddress")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);
#if !NET40
                propertyPropertyMap = map.Prop(x => x.Address.Location);
                propertyPropertyMap
                    .HasColumnName("Address_Location")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false);

                Console.WriteLine(propertyPropertyMap.DefaultValue);
                Console.WriteLine(propertyPropertyMap.FixedLength);
                Console.WriteLine(propertyPropertyMap.MaxLength);
                Console.WriteLine(propertyPropertyMap.Precision);
                Console.WriteLine(propertyPropertyMap.Scale);
                Console.WriteLine(propertyPropertyMap.Type);
                Console.WriteLine(propertyPropertyMap.Unicode);

                shapePropertyMap = map.Prop(x => x.Address.Shape);
                shapePropertyMap
                    .HasColumnName("Address_Shape")
                    .IsPk(false)
                    .IsFk(false)
                    .IsNavigationProperty(false);

                Console.WriteLine(shapePropertyMap.DefaultValue);
                Console.WriteLine(shapePropertyMap.FixedLength);
                Console.WriteLine(shapePropertyMap.MaxLength);
                Console.WriteLine(shapePropertyMap.Precision);
                Console.WriteLine(shapePropertyMap.Scale);
                Console.WriteLine(shapePropertyMap.Type);
                Console.WriteLine(shapePropertyMap.Unicode);
#endif


            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_TPH_MultipleInheritence()
        {
            using(var ctx = GetContext())
            {
                var map = ctx.Db<MultipleInheritenceBase>();
                map.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                map.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                map.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                map.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);

                var mapA = ctx.Db<MiA>();
                mapA.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                mapA.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                mapA.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                mapA.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);
                mapA.Prop(x => x.MiRefARefId).HasColumnName("MiRefARefId").IsNavigationProperty(false);

                var mapB = ctx.Db<MiB>();
                mapB.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                mapB.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                mapB.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                mapB.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);
                mapB.Prop(x => x.MiRefARefId).HasColumnName("MiRefARefId").IsNavigationProperty(false);
                mapB.Prop(x => x.MiRefBRefId).HasColumnName("MiRefBRefId").IsNavigationProperty(false);

                var mapC = ctx.Db<MiC>();
                mapC.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                mapC.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                mapC.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                mapC.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);
                mapC.Prop(x => x.MiRefARefId).HasColumnName("MiRefARefId").IsNavigationProperty(false);
                mapC.Prop(x => x.MiRefBRefId).HasColumnName("MiRefBRefId").IsNavigationProperty(false);
                mapC.Prop(x => x.MiRefCRefId).HasColumnName("MiRefCRefId").IsNavigationProperty(false);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_TPH_FlatInheritence()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<FlatInheritenceBase>();
                map.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                map.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                map.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                map.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);

                var mapA = ctx.Db<FiA>();
                mapA.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                mapA.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                mapA.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                mapA.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);
                mapA.Prop(x => x.StringA).HasColumnName("StringA").IsNavigationProperty(false);

                var mapB = ctx.Db<FiB>();
                mapB.Prop(x => x.Id).IsPk().IsFk(false).HasColumnName("Id").IsNavigationProperty(false);
                mapB.Prop(x => x.DateTime).HasColumnName("DateTime").IsNavigationProperty(false);
                mapB.Prop(x => x.String).HasColumnName("String").IsNavigationProperty(false);
                mapB.Prop(x => x.Int).HasColumnName("Int").IsNavigationProperty(false);
                mapB.Prop(x => x.StringB).HasColumnName("StringB").IsNavigationProperty(false);
            }
        }


        [Test][Explicit][Category("Explicit")]
        public void Entity_TPT_WorkerTPT()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<WorkerTPT>();

                map.Prop(x => x.Id)
                    .IsPk()
                    .IsFk(false)
                    .HasColumnName("Id")
                    .IsNavigationProperty(false);

                map.Prop(x => x.Name)
                    .IsPk(false)
                    .IsFk(false)
                    .HasColumnName("Name")
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.JobTitle)
                    .IsPk(false)
                    .IsFk(false)
                    .HasColumnName("JobTitle")
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Boss)
                    .IsPk(false)
                    .IsFk()
                    .HasColumnName("Boss_Id")
                    .IsNavigationProperty();

                var refereeIdProp = map.Prop(x => x.RefereeId);
                refereeIdProp
                    .IsPk(false)
                    .IsFk()
                    .HasColumnName("RefereeId")
                    .IsNavigationProperty(false)
                    .NavigationPropertyName("Referee");

                
                
                map.Prop(x => x.Referee)
                    .HasColumnName("RefereeId")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsNavigationProperty()
                    .ForeignKeyPropertyName("RefereeId")
                    .ForeignKey(refereeIdProp);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_TPT_ManagerTPT()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<ManagerTPT>();

                map.Prop(x => x.Id)
                    .IsPk()
                    .IsFk(false)
                    .HasColumnName("Id")
                    .IsNavigationProperty(false);

                map.Prop(x => x.Name)
                    .IsPk(false)
                    .IsFk(false)
                    .HasColumnName("Name")
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.JobTitle)
                    .IsPk(false)
                    .IsFk(false)
                    .HasColumnName("JobTitle")
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.Rank)
                    .IsPk(false)
                    .IsFk(false)
                    .HasColumnName("Rank")
                    .IsNavigationProperty(false);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Entity_Simple()
        {
            using (var ctx = new TestContext())
            {
                var map = ctx.Db<Page>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.PageId)
                    .HasColumnName("PageId")
                    .IsPk()
                    .IsFk(false)
                    .IsIdentity()
                    .IsNavigationProperty(false);

                map.Prop(x => x.Title)
                    .HasColumnName("Title")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired()
                    .IsNavigationProperty(false)
                    .MaxLength(255);

                map.Prop(x => x.Content)
                    .HasColumnName("Content")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired(false)
                    .IsNavigationProperty(false)
                    .MaxLength(NvarcharMax);

                map.Prop(x => x.ParentId)
                    .HasColumnName("ParentId")
                    .IsPk(false)
                    .IsFk()
                    .IsIdentity(false)
                    .IsRequired(false)
                    .IsNavigationProperty(false)
                    .NavigationPropertyName("Parent");

                Assert.AreEqual(map.Prop(x => x.PageId), map.Prop(x => x.ParentId).FkTargetColumn);
               
                map.Prop(x => x.Parent)
                    .HasColumnName("ParentId")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsNavigationProperty()
                    .ForeignKeyPropertyName("ParentId")
                    .ForeignKey(map.Prop(x => x.ParentId));
                
                map.Prop(x => x.CreatedAt)
                    .HasColumnName("CreatedAt")
                    .IsPk(false)
                    .IsFk(false)
                    .IsIdentity(false)
                    .IsRequired(true)
                    .IsNavigationProperty(false);

                map.Prop(x => x.ModifiedAt)
                    .HasColumnName("ModifiedAt")
                    .IsPk(false)
                    .IsFk(false)
                    .IsRequired(false)
                    .IsIdentity(false)
                    .IsNavigationProperty(false);
            }
        }
    }
}
