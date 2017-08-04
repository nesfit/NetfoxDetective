// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Linq;
using Netfox.Core.Database;
using Netfox.Framework.Models.Snoopers;
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.SnooperFacebook.Models;
using Netfox.SnooperFacebook.Models.Events;
using Netfox.SnooperSMTP.Models;
using NUnit.Framework;

namespace Netfox.Persistence.Tests
{
    [TestFixture]
    public class NetfoxDbContextInMemoryTests: FrameworkBaseTests
    {
        #region Overrides of FrameworkControllerTests
        [SetUp]
        public override void SetUpInMemory() { base.SetUpInMemory(); }
        #endregion

        [Test]
        public void ConcreteTypeGetTest()
        {

            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            var exportSMTP = new SnooperExportSMTP(this.CurrentTestBaseDirectory);

            dbx.SnooperExports.Add(exportSMTP);
            dbx.SaveChanges();

            var exportObjects = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
            var exportObjectsEmail = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportEmailBase>>();


            Assert.AreEqual(1,exportObjects.Count);
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

            var exportObjects = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
            var exportObjectsEmail = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportEmailBase>>();


            Assert.AreEqual(2, exportObjects.Count);
            Assert.AreEqual(1, exportObjectsEmail.Count);
        }

        [Test]
        public void GetDbxSetTest()
        {
            var dbx = this.WindsorContainer.Resolve<NetfoxDbContext>();
            var exports = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();

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
            var exports = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();

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
