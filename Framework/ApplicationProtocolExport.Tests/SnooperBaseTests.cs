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
using System.Collections.Generic;
using Netfox.Core.Database;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.Infrastructure;
using Netfox.Framework.Models.Snoopers;
using Netfox.NetfoxFrameworkAPI.Tests;
using NUnit.Framework;

namespace Netfox.Framework.ApplicationProtocolExport.Tests
{
    public class SnooperBaseTests : FrameworkBaseTests
    {
        public Type[] AvailableSnoopersTypes { get; private set; }
        public ISnooper[] AvailableSnoopers { get; set; }
        public ISnooperFactory SnooperFactory { get; set; }

        public VirtualizingObservableDBSetPagedCollection<SnooperExportBase> SnooperExports { get; private set; }

        [SetUp]
        public void SetUp()
        {
            base.SetUpInMemory();
            this.SnooperExports = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<SnooperExportBase>>();
            this.SnooperExports.IsNotifyImmidiately = true;
            this.SnooperFactory = this.WindsorContainer.Resolve<ISnooperFactory>();
            this.AvailableSnoopersTypes = this.SnooperFactory.AvailableSnoopersTypes;
            this.AvailableSnoopers = this.SnooperFactory.AvailableSnoopers;
        }
        public IEnumerable<ISnooper> SnoopersToUse { get; set; }
        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            //var exportCounter = 0;
            //var reportCounter = 0;
            //var objectCounter = 0;
            //base.TearDown();
            Console.WriteLine("Exports Total: " + this.SnooperExports.Count);
            foreach (var snooperExport in this.SnooperExports)
            {
                Console.WriteLine("Export:");
                //if(snooperExport == null)
                //    Console.WriteLine("null");
                Console.WriteLine((string) snooperExport.ToString());

                //Console.WriteLine("export: "+exportCounter++);
                foreach (var report in snooperExport.Reports)
                {
                    //Console.WriteLine(" report: " + reportCounter++);
                    if (report.Level == ExportReport.ReportLevel.Error || report.Level == ExportReport.ReportLevel.Fatal || report.Description == "unexpected exception caught")
                    {
                        Console.WriteLine((string) report.ToString());
                        Assert.Fail("unexpected exception or report with too high severity appeared within reports");
                    }
                }
                //reportCounter = 0;
                if (snooperExport.ExportObjects != null)
                {
                    foreach (var exportedObject in snooperExport.ExportObjects)
                    {
                        //Console.WriteLine(" object: " + objectCounter++);
                        foreach (var report in exportedObject.Reports)
                        {
                            //Console.WriteLine("  report: " + reportCounter++);
                            if (report.Level == ExportReport.ReportLevel.Error || report.Level == ExportReport.ReportLevel.Fatal
                               || report.Description == "unexpected exception caught")
                            {
                                Console.WriteLine((string) report.ToString());
                                Assert.Fail("unexpected exception or report with too high severity appeared within reports (of exported object)");
                            }
                        }
                        //reportCounter = 0;
                    }
                }
            }

            Console.WriteLine("this.SnooperExports.Count: " + this.SnooperExports.Count);
            Console.WriteLine("this.SnooperExports exported objects: " + this.GetExportedObjectCount());
        }

        protected Int32 GetExportedObjectCount()
        {
            var count = 0;
            foreach (var export in this.SnooperExports)
            {
                if (export.ExportObjects != null) count += export.ExportObjects.Count;
            }
            return count;
        }
    }
}