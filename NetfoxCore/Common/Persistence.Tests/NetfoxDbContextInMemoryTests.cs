using System;
using System.Linq;
using Netfox.Core.Database;
using Netfox.Framework.Models.Snoopers;
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.FrameworkAPI.Tests;
using Netfox.Persistence;
using Netfox.Snoopers.SnooperSMTP.Models;
using NUnit.Framework;
using SnooperFacebook.Models;
using SnooperFacebook.Models.Events;

namespace Persistence.Tests
{
    public class NetfoxDbContextInMemoryTests : FrameworkBaseTests
    {
        #region Overrides of FrameworkControllerTests

        [SetUp]
        public override void SetUpInMemory()
        {
            base.SetUpInMemory();
        }

        #endregion

        [Test]
        public void ConcreteTypeGetTest()
        {
            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            var exportSMTP = new SnooperExportSMTP(this.CurrentTestBaseDirectory);

            dbx.SnooperExports.Add(exportSMTP);
            dbx.SaveChanges();

            var exportObjects = this.WindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
            var exportObjectsEmail = this.WindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportEmailBase>>();


            Assert.AreEqual(1, exportObjects.Count);
            Assert.AreEqual(1, exportObjectsEmail.Count);
        }


        [Test]
        public void ConcreteTypeGetFilteredTest()
        {
            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            var exportSMTP = new SnooperExportSMTP(this.CurrentTestBaseDirectory);
            var exportFacebook = new FacebookSnooperExport();

            dbx.SnooperExports.Add(exportSMTP);
            dbx.SnooperExports.Add(exportFacebook);
            dbx.SaveChanges();

            var exportObjects = this.WindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
            var exportObjectsEmail = this.WindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportEmailBase>>();


            Assert.AreEqual(2, exportObjects.Count);
            Assert.AreEqual(1, exportObjectsEmail.Count);
        }

        [Test]
        public void GetDbxSetTest()
        {
            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            var exports = this.WindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();

            var exportSMTP = new SnooperExportSMTP(this.CurrentTestBaseDirectory);
            dbx.SnooperExports.Add(exportSMTP);
            dbx.SaveChanges();

            Assert.AreEqual(1, exports.Count);
            Assert.AreEqual(1, dbx.SnooperExports.Count());
        }


        [Test]
        public void AddProperyPersistenceObjectTest()
        {
            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            var exports = this.WindsorContainer
                .Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();

            var exportFacebook = new FacebookSnooperExport();
            var exportObject = new FacebookEventTyping(exportFacebook);
            exportObject.ExportSources.Add(new EmailExportResult());
            exportFacebook.OnBeforeProtocolParsing();
            exportFacebook.OnAfterProtocolParsing();
            exportFacebook.OnBeforeDataExporting();
            exportFacebook.AddExportObject(exportObject);
            exportFacebook.OnAfterDataExporting();
            dbx.SnooperExports.Add(exportFacebook);
            dbx.SaveChanges();

            Assert.AreEqual(1, exports.Count);
            Assert.AreEqual(1, dbx.SnooperExports.Count());
            Assert.AreEqual(1, dbx.SnooperExportedObjects.Count());
        }

        [Test]
        public void Set_InvalidOperationOnNonExistingEntityTest()
        {
            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            Assert.Throws(typeof(InvalidOperationException), () => dbx.Set<NetfoxDbContextInMemoryTests>());
        }
    }
}