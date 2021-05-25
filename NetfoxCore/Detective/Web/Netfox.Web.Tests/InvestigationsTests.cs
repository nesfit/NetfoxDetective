using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using Netfox.Web.App.ViewModels.Investigations;
using Netfox.Web.BL.DTO;
using Netfox.Web.DAL;
using NUnit.Framework;
using Riganti.Utils.Infrastructure.EntityFramework;
using UnitOfWork;
using UnitOfWork.EF6Repository;

namespace Netfox.Web.Tests
{
    [TestFixture]
    public class InvestigationsTests : ViewModelTestBase
    {
        public void AddUser()
        {
            this.UserFacade.Save(new UserDTO()
            {
                Firstname = "test",
                IsEnable = true,
                Username = "testuser",
                Surname = "test",
                RoleId = this.AdminRoleID
            }, "testpass");
        }

        public void AddInvestigation()
        {
            this.InvestigationFacade.AddInvestigation(new InvestigationDTO()
            {
                Name = "testInvestigation",
                Description = "Test Description.",
                Owner = this.UserFacade.GetUser("testuser"),
            }, new List<Guid>(), "../Netfox.Web.App/");
        }

        [Test]
        public void TestAddInvestigation()
        {
            this.Configuration.RouteTable.Add("Investigations_overview", "Investigations/overview", "overview.dothtml",
                null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.AddUser();
            context.Parameters.Add("InvestigationId", Guid.Empty);
            var viewModel = this.Container.Resolve<InvestigationViewModel>(new Dictionary<string, object>()
            {
                {"Context", context},
            });
            try
            {
                viewModel.Investigation = new InvestigationDTO()
                {
                    Name = "testInvestigation", Description = "Test Description.",
                    Owner = this.UserFacade.GetUser("testuser")
                };
                viewModel.Save();
            }
            catch (DotvvmInterruptRequestExecutionException ex)
            {
                Assert.AreEqual(InterruptReason.Redirect, ex.InterruptReason);
                Assert.AreEqual(ex.CustomData, "~/Investigations/overview");
                using (var uow = this.UnitOfWorkProvider.Create())
                {
                    var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                    Assert.AreEqual(
                        ctx.Investigations.Count(i =>
                            i.Name == "testInvestigation" && i.Description == "Test Description."), 1);
                }

                return;
            }

            Assert.Fail("A redirect should have been performed!");
        }

        [Test]
        public void TestRemoveInvestigation()
        {
            this.AddUser();
            this.AddInvestigation();
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/"
            };
            var viewModel = this.Container.Resolve<InvestigationOverviewViewModel>(new Dictionary<string, object>()
            {
                {"Context", context},
            });
            viewModel.Init();
            var currentUser = this.UserFacade.GetUser("testuser");
            this.InvestigationFacade.FillDataSet(viewModel.Helper.Items, viewModel.Helper.Filter, currentUser);
            viewModel.RemoveInvestigation(viewModel.Helper.Items.Items.First().Id);
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Investigations.Count(), 0);
            }
        }

        [Test]
        public void TestRemoveSelectedInvestigations()
        {
            this.AddUser();

            for (var i = 1; i <= 5; i++)
            {
                this.AddInvestigation();
            }

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Investigations.Count(), 5);
            }

            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
            };

            var viewModel = this.Container.Resolve<InvestigationOverviewViewModel>(new Dictionary<string, object>()
            {
                {"Context", context},
            });
            viewModel.Init();
            var currentUser = this.UserFacade.GetUser("testuser");
            this.InvestigationFacade.FillDataSet(viewModel.Helper.Items, viewModel.Helper.Filter, currentUser);
            viewModel.InvestigationIDs.AddRange(viewModel.Helper.Items.Items.Select(u => u.Id).Take(2).ToList());
            viewModel.RemoveSelectedInvestigations();

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Investigations.Count(), 3);
            }
        }

        [Test]
        public void TestRemoveSelectedAllInvestigations()
        {
            this.AddUser();

            for (var i = 1; i <= 5; i++)
            {
                this.AddInvestigation();
            }

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Investigations.Count(), 5);
            }

            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
            };

            var viewModel = this.Container.Resolve<InvestigationOverviewViewModel>(new Dictionary<string, object>()
            {
                {"Context", context},
            });
            viewModel.Init();
            var currentUser = this.UserFacade.GetUser("testuser");
            this.InvestigationFacade.FillDataSet(viewModel.Helper.Items, viewModel.Helper.Filter, currentUser);
            viewModel.IsAllSelected = true;
            viewModel.SelectAll();
            viewModel.RemoveSelectedInvestigations();

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Investigations.Count(), 0);
            }
        }

        [Test]
        public void TestInvestigationList()
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.UserFacade.Save(new UserDTO()
            {
                Firstname = "test",
                IsEnable = true,
                Username = "admin",
                Surname = "test",
                RoleId = AdminRoleID
            }, "testpass");

            this.UserFacade.Save(new UserDTO()
            {
                Firstname = "test",
                IsEnable = true,
                Username = "owner",
                Surname = "test",
                RoleId = this.InvestigatorRoleID
            }, "testpass");

            this.UserFacade.Save(new UserDTO()
            {
                Firstname = "test",
                IsEnable = true,
                Username = "user",
                Surname = "test",
                RoleId = this.InvestigatorRoleID
            }, "testpass");

            this.InvestigationFacade.AddInvestigation(new InvestigationDTO()
            {
                Name = "testInvestigation",
                Description = "Test Description.",
                OwnerID = this.UserFacade.GetUser("admin").Id,
            }, new List<Guid>(), "../Netfox.Web.App/");

            this.InvestigationFacade.AddInvestigation(new InvestigationDTO()
            {
                Name = "testInvestigation",
                Description = "Test Description.",
                OwnerID = this.UserFacade.GetUser("owner").Id,
            }, new List<Guid>(), "../Netfox.Web.App/");

            var listID = new List<Guid>();

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                listID.Add(ctx.Users.SingleOrDefault(u => u.Username == "user").Id);
            }

            this.InvestigationFacade.AddInvestigation(new InvestigationDTO()
            {
                Name = "testInvestigation",
                Description = "Test Description.",
                OwnerID = this.UserFacade.GetUser("owner").Id,
            }, listID, "../Netfox.Web.App/");

            var viewModel = this.Container.Resolve<InvestigationOverviewViewModel>(new Dictionary<string, object>()
            {
                {"Context", context},
            });

            viewModel.Init();

            this.InvestigationFacade.FillDataSet(viewModel.Helper.Items, new InvestigationFilterDTO(),
                this.UserFacade.GetUser("admin"));
            Assert.AreEqual(viewModel.Helper.Items.Items.Count, 3);
            this.InvestigationFacade.FillDataSet(viewModel.Helper.Items, new InvestigationFilterDTO(),
                this.UserFacade.GetUser("owner"));
            Assert.AreEqual(viewModel.Helper.Items.Items.Count, 2);
            this.InvestigationFacade.FillDataSet(viewModel.Helper.Items, new InvestigationFilterDTO(),
                this.UserFacade.GetUser("user"));
            Assert.AreEqual(viewModel.Helper.Items.Items.Count, 1);
        }
    }
}