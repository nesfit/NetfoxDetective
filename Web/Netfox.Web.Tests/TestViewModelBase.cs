using System;
using System.Data;
using System.IO;
using System.Linq;
using AutoMapper;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel;
using Castle.Windsor;
using Castle.Windsor.Installer;
using NUnit.Framework;
using Castle.MicroKernel.Registration;
using DotVVM.Framework.Configuration;
using Hangfire;
using Netfox.Web.App;
using Netfox.Web.BL.Facades;
using UnitOfWork;
using UnitOfWork.EF6Repository;
using UnitOfWork.EF6UnitOfWork;
using Component = Castle.MicroKernel.Registration.Component;
using Netfox.Web.BL.Mappings;
using Netfox.Web.DAL;
using Netfox.Web.DAL.Entities;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;

namespace Netfox.Web.Tests
{
    public static class HostingEnvironment
    {
        public static string ApplicationPhysicalPath { get; set; } = "/sss/";
    }
    [TestFixture]
    public abstract class ViewModelTestBase
    {
        protected WindsorContainer Container { get; set; }
        protected DotvvmConfiguration Configuration { get; set; }
        protected bool IsAutomapperInitialized { get; set; } = false;

        public IRepository<Role, Guid> Roles { get; set; }

        public IRepository<User, Guid> Users { get; set; }

        public IRepository<Investigation, Guid> Investigations { get; set; }

        public IUnitOfWorkProvider UnitOfWorkProvider { get; set; }

        public UserFacade UserFacade { get; set; }

        public InvestigationFacade InvestigationFacade { get; set; }

        public Guid AdminRoleID { get; set; }

        public Guid InvestigatorRoleID { get; set; }

        [OneTimeTearDown]
        public void Cleanup()
        { 
            this.Container.Dispose();
            this.Container = null;
        }

        [OneTimeSetUp]
        public void Init()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            HostingEnvironment.ApplicationPhysicalPath = Directory.GetCurrentDirectory();
            Directory.CreateDirectory("Investigations");
            this.Container = new WindsorContainer("NetfoxWebTestsBase", new DefaultKernel(), new DefaultComponentInstaller());
            this.Install();

            this.UnitOfWorkProvider = this.Container.Resolve<IUnitOfWorkProvider>();
            this.Roles = this.Container.Resolve<IRepository <Role, Guid>> ();
            this.Users = this.Container.Resolve<IRepository<User, Guid>>();
            this.Investigations = this.Container.Resolve<IRepository<Investigation, Guid>>();
            this.UserFacade = this.Container.Resolve<UserFacade>();
            this.InvestigationFacade = this.Container.Resolve<InvestigationFacade>();
        }
        [TearDown]
        public virtual void Clean()
        {
            using(var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                ctx.Database.Delete();
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            this.Configuration = DotvvmConfiguration.CreateDefault();
            using(var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                ctx.Database.Delete();

                this.Roles.Insert(new Role() { Name = "Administrator" });
                this.Roles.Insert(new Role() { Name = "Investigator" });
                uow.Commit();
                //var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                this.AdminRoleID = ctx.Roles.First(r => r.Name == "Administrator").Id;
                this.InvestigatorRoleID = ctx.Roles.First(r => r.Name == "Investigator").Id;
            }
        }

        public void Install()
        {
            this.Container = new WindsorContainer();
            this.Container.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(this.Container));
            this.Container.AddFacility<TypedFactoryFacility>();
            this.Container.Install(FromAssembly.Containing<Startup>());

            Mapper.Initialize(mapper =>
            {
                foreach (var mapping in this.Container.ResolveAll<IMapping>())
                {
                    mapping.Configure(mapper);
                }
            });
            Mapper.AssertConfigurationIsValid();

        }

    }
}
