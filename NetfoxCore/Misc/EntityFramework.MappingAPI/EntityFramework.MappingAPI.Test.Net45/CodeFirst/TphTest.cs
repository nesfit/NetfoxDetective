using System;
using EntityFramework.MappingAPI.Extensions;
using EntityFramework.MappingAPI.Test.CodeFirst.Domain;
using NUnit.Framework;

namespace EntityFramework.MappingAPI.Test.CodeFirst
{
    public class TphTest : TestBase
    {
        [Test][Explicit][Category("Explicit")]
        public void Employee_BaseClass()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<EmployeeTPH>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.Id)
                    .HasColumnName("Id")
                    .IsPk()
                    .IsFk(false);

                map.Prop(x => x.Name)
                    .HasColumnName("Name")
                    .MaxLength(NvarcharMax)
                    .NavigationPropertyName(null)
                    .IsPk(false)
                    .IsFk(false);

                Assert.IsNull(map.Prop(x => x.NameWithTitle));

                map.Prop(x => x.Title)
                    .HasColumnName("JobTitle")
                    .IsPk(false)
                    .IsFk(false)
                    .MaxLength(NvarcharMax)
                    .IsNavigationProperty(false);

                map.Prop("__employeeType")
                    .HasColumnName("__employeeType")
                    .IsPk(false)
                    .IsFk(false)
                    .IsDiscriminator();
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Employee_DerivedType_First()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<AWorkerTPH>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.Id)
                    .HasColumnName("Id");

                map.Prop(x => x.Name)
                    .HasColumnName("Name");

                map.Prop(x => x.Title)
                    .HasColumnName("JobTitle");

                map.Prop(x => x.Boss)
                    .IsNavigationProperty()
                    .ForeignKeyPropertyName("BossId")
                    .ForeignKey(map.Prop(x => x.BossId))
                    .IsFk(false)
                    .HasColumnName("BossId");

                map.Prop(x => x.BossId)
                    .IsFk()
                    .NavigationPropertyName("Boss")
                    .NavigationProperty(map.Prop(x => x.Boss))
                    .HasColumnName("BossId");

                map.Prop(x => x.RefId)
                    .HasColumnName("RefId");

                Assert.AreEqual(1, map.Discriminators.Length);
                Assert.AreEqual("__employeeType", map.Discriminators[0].ColumnName);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Employee_DerivedType_NotFirst()
        {
            using (var ctx = GetContext())
            {
                var map = ctx.Db<ManagerTPH>();
                Console.WriteLine("{0}:{1}", map.Type, map.TableName);

                map.Prop(x => x.Id)
                    .HasColumnName("Id");

                map.Prop(x => x.Name)
                    .HasColumnName("Name");

                map.Prop(x => x.Title)
                    .HasColumnName("JobTitle");

                map.Prop(x => x.Rank)
                    .HasColumnName("Rank");

                map.Prop(x => x.RefIdNullable)
                    .HasColumnName("RefIdNullable");

                Assert.AreEqual(1, map.Discriminators.Length);
                Assert.AreEqual("__employeeType", map.Discriminators[0].ColumnName);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Contract_ContractBase()
        {
            using (var ctx = new TestContext())
            {
                var map = ctx.Db<ContractBase>();

                var columns = map.Properties;
                Assert.AreEqual(19, columns.Length);

                Assert.AreEqual(1, map.Discriminators.Length);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Contract_Contract()
        {
            using (var ctx = new TestContext())
            {
                var map = ctx.Db<Contract>();

                var columns = map.Properties;
                Assert.AreEqual(19, columns.Length);

                map.Prop(x => x.Id)
                    .IsIdentity()
                    .IsFk(false)
                    .IsDiscriminator(false)
                    .IsRequired()
                    .HasColumnName("Id");


                map.Prop(x => x.AvpContractNr)
                    .IsIdentity(false)
                    .IsFk(false)
                    .IsDiscriminator(false)
                    .IsRequired(false)
                    .HasColumnName("AvpContractNr")
                    .MaxLength(50);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Contract_ContractFixed()
        {
            using (var ctx = new TestContext())
            {
                var tableMapping = ctx.Db<ContractFixed>();

                var columns = tableMapping.Properties;
                Assert.AreEqual(21, columns.Length);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Contract_ContractStock()
        {
            using (var ctx = new TestContext())
            {
                var tableMapping = ctx.Db<ContractStock>();

                var columns = tableMapping.Properties;
                Assert.AreEqual(21, columns.Length);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Contract_ContractKomb1()
        {
            using (var ctx = new TestContext())
            {
                var tableMapping = ctx.Db<ContractKomb1>();

                var columns = tableMapping.Properties;
                Assert.AreEqual(24, columns.Length);

                tableMapping.Prop(x => x.Base)
                    .HasColumnName("Base")
                    .HasPrecision(18)
                    .HasScale(4);
            }
        }

        [Test][Explicit][Category("Explicit")]
        public void Contract_ContractKomb2()
        {
            using (var ctx = new TestContext())
            {
                var tableMapping = ctx.Db<ContractKomb2>();

                var columns = tableMapping.Properties;
                Assert.AreEqual(26, columns.Length);
            }
        }
    }
}