using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Infrastructure;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperFTP;
using Netfox.Snoopers.SnooperFTP.WEB.DTO;
using Netfox.Snoopers.SnooperFTP.WEB.Facade;
using Netfox.Snoopers.SnooperHTTP;
using Netfox.Snoopers.SnooperHTTP.WEB.DTO;
using Netfox.Snoopers.SnooperHTTP.WEB.Facade;
using Netfox.Snoopers.SnooperSIP;
using Netfox.Snoopers.SnooperYMSG;
using Netfox.Web.App.Settings;
using Netfox.Web.BL;
using Netfox.Web.BL.DTO;
using Netfox.Web.BL.Facades;
using Netfox.Web.BL.Facades.Base;
using Netfox.Web.BL.Queries;
using Netfox.Web.BL.Queries.FirstLevel;
using Netfox.Web.DAL;
using Netfox.Web.DAL.Entities;
using NUnit.Framework;
using Riganti.Utils.Infrastructure.AutoMapper;
using Riganti.Utils.Infrastructure.Core;
using Riganti.Utils.Infrastructure.EntityFramework;
using Riganti.Utils.Infrastructure.Services.Facades;

namespace Netfox.Web.Tests
{
    [TestFixture]
    public class ExportTests : ViewModelTestBase
    {

        protected CaptureFacade CaptureFacade { get; set; }
        protected ExportFacade ExportFacade { get; set; }
        protected ExportFTPFacade ExportFTPFacade { get; set; }
        protected ExportHTTPFacade ExportHTTPFacade { get; set; }
        protected IWindsorContainer ContainerAPI { get; set; }


        [OneTimeSetUp]
        public void Init()
        {
            this.Container.Register(Component.For<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>()
                .Instance((container, id, path) => new NetfoxAPIFacade(container.Resolve<INetfoxWebSettings>(), container, id, path)).LifestyleSingleton());

            this.CaptureFacade = this.Container.Resolve<CaptureFacade>();
            this.ExportFacade = this.Container.Resolve<ExportFacade>();
            this.ExportFTPFacade = this.Container.Resolve<ExportFTPFacade>();
            this.ExportHTTPFacade = this.Container.Resolve<ExportHTTPFacade>();
            this.ContainerAPI = new WindsorContainer("TestWC", new DefaultKernel(), new DefaultComponentInstaller());
            this.InitializeWindsor(this.ContainerAPI);
        }

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
                OwnerID = this.UserFacade.GetUser("testuser").Id,
            }, new List<Guid>(), "../Netfox.Web.App/");
        }

        [Test]
        public void TestFramework_FTP()
        {
            this.Configuration.RouteTable.Add("Investigations_overview", "Investigations/overview", "overview.dothtml", null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.AddUser();
            this.AddInvestigation();

            Investigation investigation;
            using(var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                investigation = ctx.Investigations.First();
            }

            var NetfoxAPIFacadeFactory = this.Container.Resolve<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>();
            var netfoxAPI = NetfoxAPIFacadeFactory(this.ContainerAPI, investigation.Id, Directory.GetCurrentDirectory());
            var snooper = Container.Resolve<SnooperFTP>();

            Assert.NotNull(netfoxAPI);

            netfoxAPI.ProcessCapture(PcapPath.GetPcap(PcapPath.Pcaps.ftp_ftp_xkarpi03_02_pcap));

            var capturelList = CaptureFacade.GetCaptureList(investigation.Id).ToList();

            Assert.AreEqual(capturelList.Count, 1);

            var l3 = new GridViewDataSet<L3ConversationDTO>();
            var l4 = new GridViewDataSet<L4ConversationDTO>();
            var l7 = new GridViewDataSet<L7ConversationDTO>();
            var frame = new GridViewDataSet<PmFrameBaseDTO>();

            l3.PagingOptions.PageSize = 15;
            l3.SortingOptions.SortDescending = false;
            l3.SortingOptions.SortExpression = nameof(L3ConversationDTO.Id);

            l4.PagingOptions.PageSize = 15;
            l4.SortingOptions.SortDescending = false;
            l4.SortingOptions.SortExpression = nameof(L4ConversationDTO.Id);

            l7.PagingOptions.PageSize = 15;
            l7.SortingOptions.SortDescending = false;
            l7.SortingOptions.SortExpression = nameof(L7ConversationDTO.Id);

            frame.PagingOptions.PageSize = 15;
            frame.SortingOptions.SortDescending = false;
            frame.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.Id);

            this.CaptureFacade.FillL3ConversationDataSet(l3, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l3.PagingOptions.TotalItemsCount, 1);
            this.CaptureFacade.FillL4ConversationDataSet(l4, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l4.PagingOptions.TotalItemsCount, 5);
            this.CaptureFacade.FillL7ConversationDataSet(l7, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l7.PagingOptions.TotalItemsCount, 5);
            this.CaptureFacade.FillPmFrameDataSet(frame, capturelList.First().Id, investigation.Id, new FrameFilterDTO());
            Assert.AreEqual(frame.PagingOptions.TotalItemsCount, 69);

            var listSnooper = new List<string>();
            listSnooper.Add(snooper.GetType().FullName);

            netfoxAPI.ExportData(listSnooper);
            var exports = new GridViewDataSet<SnooperFTPListDTO>();

            exports.PagingOptions.PageSize = 15;
            exports.SortingOptions.SortDescending = false;
            exports.SortingOptions.SortExpression = nameof(SnooperFTPListDTO.FirstSeen);

            ExportFTPFacade.FillDataSet(exports, investigation.Id, new ExportFilterDTO());
            
            Assert.AreEqual(exports.PagingOptions.TotalItemsCount, 21);
        }

        [Test]
        public void TestFramework_SIP()
        {
            this.Configuration.RouteTable.Add("Investigations_overview", "Investigations/overview", "overview.dothtml", null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.AddUser();
            this.AddInvestigation();

            Investigation investigation;
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                investigation = ctx.Investigations.First();
            }

            var NetfoxAPIFacadeFactory = this.Container.Resolve<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>();
            var netfoxAPI = NetfoxAPIFacadeFactory(this.ContainerAPI, investigation.Id, Directory.GetCurrentDirectory());
            var snooperSIP = Container.Resolve<SnooperSIP>();

            Assert.NotNull(netfoxAPI);

            netfoxAPI.ProcessCapture(PcapPath.GetPcap(PcapPath.Pcaps.sip_caps_sip_rtcp_pcap));

            var capturelList = CaptureFacade.GetCaptureList(investigation.Id).ToList();

            Assert.AreEqual(capturelList.Count, 1);

            var l3 = new GridViewDataSet<L3ConversationDTO>();
            var l4 = new GridViewDataSet<L4ConversationDTO>();
            var l7 = new GridViewDataSet<L7ConversationDTO>();
            var frame = new GridViewDataSet<PmFrameBaseDTO>();

            l3.PagingOptions.PageSize = 15;
            l3.SortingOptions.SortDescending = false;
            l3.SortingOptions.SortExpression = nameof(L3ConversationDTO.Id);

            l4.PagingOptions.PageSize = 15;
            l4.SortingOptions.SortDescending = false;
            l4.SortingOptions.SortExpression = nameof(L4ConversationDTO.Id);

            l7.PagingOptions.PageSize = 15;
            l7.SortingOptions.SortDescending = false;
            l7.SortingOptions.SortExpression = nameof(L7ConversationDTO.Id);

            frame.PagingOptions.PageSize = 15;
            frame.SortingOptions.SortDescending = false;
            frame.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.Id);

            this.CaptureFacade.FillL3ConversationDataSet(l3, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l3.PagingOptions.TotalItemsCount, 31);
            this.CaptureFacade.FillL4ConversationDataSet(l4, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l4.PagingOptions.TotalItemsCount, 73);
            this.CaptureFacade.FillL7ConversationDataSet(l7, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l7.PagingOptions.TotalItemsCount, 76);
            this.CaptureFacade.FillPmFrameDataSet(frame, capturelList.First().Id, investigation.Id, new FrameFilterDTO());
            Assert.AreEqual(frame.PagingOptions.TotalItemsCount, 31869);

            var listSnooper = new List<string>();
            listSnooper.Add(snooperSIP.GetType().FullName);

            netfoxAPI.ExportData(listSnooper);
            var exports = new GridViewDataSet<ExportCallDTO>();

            exports.PagingOptions.PageSize = 15;
            exports.SortingOptions.SortDescending = false;
            exports.SortingOptions.SortExpression = nameof(ExportCallDTO.Id);


            ExportFacade.FillCallDataSet(exports, investigation.Id, new ExportFilterDTO());

            Assert.AreEqual(exports.PagingOptions.TotalItemsCount, 1);
        }

        [Test]
        public void TestFramework_HTTP()
        {
            this.Configuration.RouteTable.Add("Investigations_overview", "Investigations/overview", "overview.dothtml", null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.AddUser();
            this.AddInvestigation();

            Investigation investigation;
            // var exportService = Container.Resolve<ExportService>(new { investigationId = investigation.Id });
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                investigation = ctx.Investigations.First();
            }

            var NetfoxAPIFacadeFactory = this.Container.Resolve<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>();
            var netfoxAPI = NetfoxAPIFacadeFactory(this.ContainerAPI, investigation.Id, Directory.GetCurrentDirectory());
            var snooper = Container.Resolve<SnooperHTTP>();

            Assert.NotNull(netfoxAPI);

            netfoxAPI.ProcessCapture(PcapPath.GetPcap(PcapPath.Pcaps.http_caps_malyweb2_pcapng));

            var capturelList = CaptureFacade.GetCaptureList(investigation.Id).ToList();

            Assert.AreEqual(capturelList.Count, 1);

            var l3 = new GridViewDataSet<L3ConversationDTO>();
            var l4 = new GridViewDataSet<L4ConversationDTO>();
            var l7 = new GridViewDataSet<L7ConversationDTO>();
            var frame = new GridViewDataSet<PmFrameBaseDTO>();

            l3.PagingOptions.PageSize = 15;
            l3.SortingOptions.SortDescending = false;
            l3.SortingOptions.SortExpression = nameof(L3ConversationDTO.Id);

            l4.PagingOptions.PageSize = 15;
            l4.SortingOptions.SortDescending = false;
            l4.SortingOptions.SortExpression = nameof(L4ConversationDTO.Id);

            l7.PagingOptions.PageSize = 15;
            l7.SortingOptions.SortDescending = false;
            l7.SortingOptions.SortExpression = nameof(L7ConversationDTO.Id);

            frame.PagingOptions.PageSize = 15;
            frame.SortingOptions.SortDescending = false;
            frame.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.Id);

            this.CaptureFacade.FillL3ConversationDataSet(l3, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l3.PagingOptions.TotalItemsCount, 24);
            this.CaptureFacade.FillL4ConversationDataSet(l4, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l4.PagingOptions.TotalItemsCount, 247);
            this.CaptureFacade.FillL7ConversationDataSet(l7, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l7.PagingOptions.TotalItemsCount, 247);
            this.CaptureFacade.FillPmFrameDataSet(frame, capturelList.First().Id, investigation.Id, new FrameFilterDTO());
            Assert.AreEqual(frame.PagingOptions.TotalItemsCount, 3330);

            var listSnooper = new List<string>();
            listSnooper.Add(snooper.GetType().FullName);

            netfoxAPI.ExportData(listSnooper);
            var exports = new GridViewDataSet<SnooperHTTPListDTO>();

            exports.PagingOptions.PageSize = 15;
            exports.SortingOptions.SortDescending = false;
            exports.SortingOptions.SortExpression = nameof(SnooperHTTPListDTO.Id);

            this.ExportHTTPFacade.FillMessages(investigation.Id, exports, new ExportFilterDTO());

            Assert.AreEqual(exports.PagingOptions.TotalItemsCount, 355);
        }

        [Test]
        public void TestFramework_Messages()
        {
            this.Configuration.RouteTable.Add("Investigations_overview", "Investigations/overview", "overview.dothtml", null);
            var context = new TestDotvvmRequestContext()
            {
                Configuration = this.Configuration,
                ApplicationHostPath = "../Netfox.Web.App/",
                Parameters = new Dictionary<string, object>()
            };

            this.AddUser();
            this.AddInvestigation();

            Investigation investigation;
            // var exportService = Container.Resolve<ExportService>(new { investigationId = investigation.Id });
            using (var uow = this.UnitOfWorkProvider.Create())
            {
                var ctx = EntityFrameworkUnitOfWork.TryGetDbContext<NetfoxWebDbContext>(this.UnitOfWorkProvider);
                investigation = ctx.Investigations.First();
            }

            var NetfoxAPIFacadeFactory = this.Container.Resolve<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>();
            var netfoxAPI = NetfoxAPIFacadeFactory(this.ContainerAPI, investigation.Id, Directory.GetCurrentDirectory());
            var snooper = Container.Resolve<SnooperYMSG>();

            Assert.NotNull(netfoxAPI);

            netfoxAPI.ProcessCapture(PcapPath.GetPcap(PcapPath.Pcaps.pcap_mix_ymsg_without_file_cap));

            var capturelList = CaptureFacade.GetCaptureList(investigation.Id).ToList();

            Assert.AreEqual(capturelList.Count, 1);

            var l3 = new GridViewDataSet<L3ConversationDTO>();
            var l4 = new GridViewDataSet<L4ConversationDTO>();
            var l7 = new GridViewDataSet<L7ConversationDTO>();
            var frame = new GridViewDataSet<PmFrameBaseDTO>();

            l3.PagingOptions.PageSize = 15;
            l3.SortingOptions.SortDescending = false;
            l3.SortingOptions.SortExpression = nameof(L3ConversationDTO.Id);

            l4.PagingOptions.PageSize = 15;
            l4.SortingOptions.SortDescending = false;
            l4.SortingOptions.SortExpression = nameof(L4ConversationDTO.Id);

            l7.PagingOptions.PageSize = 15;
            l7.SortingOptions.SortDescending = false;
            l7.SortingOptions.SortExpression = nameof(L7ConversationDTO.Id);

            frame.PagingOptions.PageSize = 15;
            frame.SortingOptions.SortDescending = false;
            frame.SortingOptions.SortExpression = nameof(PmFrameBaseDTO.Id);

            this.CaptureFacade.FillL3ConversationDataSet(l3, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l3.PagingOptions.TotalItemsCount, 9);
            this.CaptureFacade.FillL4ConversationDataSet(l4, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l4.PagingOptions.TotalItemsCount, 9);
            this.CaptureFacade.FillL7ConversationDataSet(l7, capturelList.First().Id, investigation.Id, new ConversationFilterDTO());
            Assert.AreEqual(l7.PagingOptions.TotalItemsCount, 9);
            this.CaptureFacade.FillPmFrameDataSet(frame, capturelList.First().Id, investigation.Id, new FrameFilterDTO());
            Assert.AreEqual(frame.PagingOptions.TotalItemsCount, 158);

            var listSnooper = new List<string>();
            listSnooper.Add(snooper.GetType().FullName);

            netfoxAPI.ExportData(listSnooper);
            var exports = new GridViewDataSet<ExportChatMessageDTO>();

            exports.PagingOptions.PageSize = 15;
            exports.SortingOptions.SortDescending = false;
            exports.SortingOptions.SortExpression = nameof(ExportChatMessageDTO.Id);

            ExportFacade.FillChatMessageDataSet(exports, investigation.Id, new ExportFilterDTO());

            Assert.AreEqual(exports.PagingOptions.TotalItemsCount, 4);
        }

        private void InitializeWindsor(IWindsorContainer container)
        {
            container.Register(Component.For<IWindsorContainer, WindsorContainer>().Instance(container));
            container.Register(
                Classes.FromAssemblyInDirectory(new AssemblyFilter(new FileInfo(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.ToString()))
                    .BasedOn<ISnooper>().WithServiceFromInterface(typeof(ISnooper)).WithServiceSelf().LifestyleTransient(),
                Component.For<Func<IWindsorContainer, Guid, string, NetfoxAPIFacade>>().Instance((c, id, path) => new NetfoxAPIFacade(container.Resolve<INetfoxWebSettings>(),c, id, path)).LifestyleSingleton(),

                /* DAL */
                Component.For<Func<NetfoxWebDbContext>>().Instance(() => new NetfoxWebDbContext()).LifestyleSingleton(),
                Component.For<IUnitOfWorkProvider>().ImplementedBy<EntityFrameworkUnitOfWorkProvider<NetfoxWebDbContext>>().LifestyleSingleton(),
                Component.For<IUnitOfWorkRegistry>().UsingFactoryMethod(p => new AsyncLocalUnitOfWorkRegistry()).LifestyleSingleton(),
                Classes.FromAssemblyContaining<BusinessLayer>().BasedOn(typeof(AppQueryBase<>)).LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>().BasedOn(typeof(AppFirstLevelQueryBase<>)).LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>().BasedOn(typeof(IRepository<,>)).WithServiceAllInterfaces().WithServiceSelf().LifestyleTransient(),
                Component.For(typeof(IRepository<,>)).ImplementedBy(typeof(EntityFrameworkRepository<,>)).IsFallback().LifestyleTransient(),
                Component.For<IDateTimeProvider>().Instance(new UtcDateTimeProvider()),
                Component.For(typeof(IEntityDTOMapper<,>)).ImplementedBy(typeof(EntityDTOMapper<,>)).LifestyleSingleton(),
                /* // DAL */

                /* BL */
                Classes.FromAssemblyContaining<BusinessLayer>().BasedOn<FacadeBase>().LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>().BasedOn<IFilterDTO>().LifestyleTransient(),
                Classes.FromAssemblyContaining<BusinessLayer>().BasedOn<NetfoxFacadeBase>().LifestyleTransient());

            var netfoxFrameworkApiWindsorInstaller = new NetfoxFrameworkApiWindsorInstaller();
            netfoxFrameworkApiWindsorInstaller.Install(container, null);
        }
    }
}
