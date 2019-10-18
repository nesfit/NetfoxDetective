using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using Netfox.Web.App.ViewModels;
using Netfox.Web.App.ViewModels.Settings;
using Netfox.Web.BL.DTO;
using Netfox.Web.DAL;
using Netfox.Web.DAL.Entities;
using NUnit.Framework;
using Riganti.Utils.Infrastructure.EntityFramework;
using UnitOfWork;
using UnitOfWork.EF6Repository;

namespace Netfox.Web.Tests
{
    [TestFixture]
    public class UserManagementTests : ViewModelTestBase
    {

        [Test]
        public void TestAddUser()
        {
            this.Configuration.RouteTable.Add("Settings_UserManagement", "settings/User_management", "user.dothtml", null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };
            context.Parameters.Add("UserId", Guid.Empty);
            var viewModel = this.Container.Resolve<UserViewModel>(new
            {
                Context = context,
                Password = "testpass",
                
            });
            try
            {
               
                viewModel.PreRender();
                viewModel.User.Username = "testuser";
                viewModel.User.Firstname = "test";
                viewModel.User.Surname = "test";
                viewModel.User.IsEnable = true;
                viewModel.Save();
            }
            catch (DotvvmInterruptRequestExecutionException ex)
            {
                Assert.AreEqual(InterruptReason.Redirect, ex.InterruptReason);
                Assert.AreEqual(ex.CustomData, "~/settings/User_management");
                using(var uow = this.UnitOfWorkProvider.Create())
                {
                    var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                    var passHash = this.UserFacade.LoginFacade.SHA256Hash("testpass");
                    Assert.AreEqual(ctx.Users.Count(u => u.Surname == "test" && u.Firstname == "test" && u.Username == "testuser" && u.Password == passHash), 1);
                }


                return;
            }
            Assert.Fail("A redirect should have been performed!");
        }
        [Test]
        public void TestRemoveUser()
        {
            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = true, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
               
                Assert.AreEqual(ctx.Users.Count(), 1);
            }

            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
               
            };
    
            var viewModel = this.Container.Resolve<UserManagementViewModel>(new
            {
                Context = context,

            });
            viewModel.Init();
            viewModel.PreRender();
            viewModel.RemoveUser(viewModel.Helper.Items.Items.First().Id);

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Users.Count(), 0);
            }

        }
        [Test]
        public void TestRemoveSelectedUsers()
        {

            for(var i = 1; i <= 5; i++)
            {
                this.UserFacade.Save(new UserDTO() { Firstname = "test" + i, IsEnable = true, Username = "testuser" + i, Surname = "test" + i, RoleId = this.AdminRoleID }, "testpass");
            }

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Users.Count(), 5);
            }

            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",

            };

            var viewModel = this.Container.Resolve<UserManagementViewModel>(new
            {
                Context = context,
            });
            viewModel.Init();
            viewModel.PreRender();
            viewModel.UserIDs.AddRange(viewModel.Helper.Items.Items.Select(u => u.Id).Take(2).ToList());
            viewModel.RemoveSelectedUsers();

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Users.Count(), 3);
            }

        }
        [Test]
        public void TestRemoveSelectedAllUsers()
        {

            for (var i = 1; i <= 5; i++)
            {
                this.UserFacade.Save(new UserDTO() { Firstname = "test" + i, IsEnable = true, Username = "testuser" + i, Surname = "test" + i, RoleId = this.AdminRoleID }, "testpass");
            }

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);

                Assert.AreEqual(ctx.Users.Count(), 5);
            }

            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",

            };

            var viewModel = this.Container.Resolve<UserManagementViewModel>(new
            {
                Context = context,
            });

            viewModel.Init();
            viewModel.PreRender();
            viewModel.IsAllSelected = true;
            viewModel.SelectAll();
            viewModel.RemoveSelectedUsers();

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                Assert.AreEqual(ctx.Users.Count(), 0);
            }

        }
        [Test]
        public void TestEditDataUser()
        {
            this.Configuration.RouteTable.Add("Settings_UserManagement", "settings/User_management", "article.dothtml", null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };
            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = true, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");
            var userId = new Guid();
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                userId = ctx.Users.First().Id;
            }
            context.Parameters.Add("UserId", userId);

            var viewModel = this.Container.Resolve<UserViewModel>(new
            {
                Context = context,
                
            });
            try
            {
                viewModel.UserId = userId;
                viewModel.Init();
                viewModel.PreRender();
                viewModel.User.IsEnable = false;
                viewModel.User.Surname = "test2";
                viewModel.Save();
            }
            catch (DotvvmInterruptRequestExecutionException ex)
            {
                Assert.AreEqual(InterruptReason.Redirect, ex.InterruptReason);
                Assert.AreEqual(ex.CustomData, "~/settings/User_management");
                using (var uow = this.UnitOfWorkProvider.Create())
                {
                    var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                    var passHash = this.UserFacade.LoginFacade.SHA256Hash("testpass");
                    Assert.AreEqual(ctx.Users.Count(u => u.Surname == "test2" && u.Firstname == "test" && u.Username == "testuser" && !u.IsEnable && u.Password == passHash), 1);
                }
                
                return;
            }
            Assert.Fail("A redirect should have been performed!");
        }
        [Test]
        public void TestChangePassword()
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = true, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");

            var viewModel = this.Container.Resolve<ProfileViewModel>(new
            {
                Context = context,
                Username = "testuser"
            });
 
            viewModel.User = UserFacade.GetUser("testuser");
            viewModel.Password = "testpass";
            viewModel.NewPassword = "newtestpass";
            viewModel.ChangePassword();

            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                var passHash = this.UserFacade.LoginFacade.SHA256Hash("newtestpass");
                Assert.AreEqual(ctx.Users.Count(u => u.Surname == "test" && u.Firstname == "test" && u.Username == "testuser" && u.Password == passHash), 1);
            }
        }
        [Test]
        public void TestLoginEmptyInput()
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = true, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");

            var viewModel = this.Container.Resolve<LoginViewModel>(new
            {
                Context = context,
                LoginData = new LoginDTO() { Username = "testuser", Password = "" }
            });

            try
            {
                viewModel.DoLogin();
            }
            catch (NotSupportedException ex)
            {
                Assert.AreEqual(ex.Message, "This method can be used only in OWIN hosting!");
                Assert.Fail("Login with empty inputs!");
                return;
            }
        }
        [Test]
        public void TestLoginBadPassword()
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = true, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");

            var viewModel = this.Container.Resolve<LoginViewModel>(new
            {
                Context = context,
                LoginData = new LoginDTO() { Username = "testuser", Password = "testpas" }

            });

            try
            {
                viewModel.DoLogin();
            }
            catch (NotSupportedException ex)
            {
                Assert.AreEqual(ex.Message, "This method can be used only in OWIN hosting!");
                Assert.Fail("Login on bad password ");
                return;
            }
            Assert.AreEqual(viewModel.ErrorMessage, "Invalid username or password!");
        }
        [Test]
        public void TestLogin()
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = true, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");

            var viewModel = this.Container.Resolve<LoginViewModel>(new
            {
                Context = context,
                LoginData = new LoginDTO() { Username = "testuser", Password = "testpass" }
            });

            try
            {
                viewModel.DoLogin();
            }
            catch(NotSupportedException ex)
            {
                Assert.AreEqual(ex.Message, "This method can be used only in OWIN hosting!");
                return;
            }
            Assert.Fail("A redirect should have been performed!");
        }
        [Test]
        public void TestNotActiveLogin()
        {
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.UserFacade.Save(new UserDTO() { Firstname = "test", IsEnable = false, Username = "testuser", Surname = "test", RoleId = this.AdminRoleID }, "testpass");

            var viewModel = this.Container.Resolve<LoginViewModel>(new
            {
                Context = context,
                LoginData = new LoginDTO() { Username = "testuser", Password = "testpass" }
            });

            try
            {
                viewModel.DoLogin();
            }
            catch (NotSupportedException ex)
            {
                Assert.AreEqual(ex.Message, "This method can be used only in OWIN hosting!");
                Assert.Fail("Account is not active!");
                return;
            }
            Assert.AreEqual(viewModel.ErrorMessage, "Account is not active!");
        }
    }
}
