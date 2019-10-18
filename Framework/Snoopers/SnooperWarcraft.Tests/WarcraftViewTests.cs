// Copyright (c) 2017 Jan Pluskal, Matus Dobrotka
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Netfox.SnooperWarcraft.WPF.Views;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperWarcraft.WPF.ViewModels;
using NUnit.Framework;
using WarcraftConversationView = Netfox.SnooperWarcraft.WPF.Views.WarcraftConversationView;
using WarcraftSnooperTestWindow = Netfox.SnooperWarcraft.WPF.Views.WarcraftSnooperTestWindow;

namespace Netfox.SnooperWarcraft.Tests
{
    class WarcraftViewTests : SnooperBaseTests
    {
        [Test]
        [Explicit][Category("Explicit")]
        public void Warcraft_Snooper_Grid_View_Test()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(SnoopersPcaps.Default.warcraft_xberan33_1_wdat)
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);


            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 526);

            var exportVm = new ExportVm(this.WindsorContainer, this.SnooperExports.ToList()[0]);
            var snooperWarcraftViewModel = new WarcraftMessageViewModel(this.WindsorContainer, exportVm, null);

            // The dispatcher thread
            var t = new Thread(() =>
            {
                var window = new WarcraftSnooperTestWindow
                {
                    Content = snooperWarcraftViewModel.View
                };

                // Initiates the dispatcher thread shutdown when the window closes
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();

                window.Show();

                // Makes the thread support message pumping
                Dispatcher.Run();
            });

            // Configure the thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        [Test]
        [Explicit][Category("Explicit")]
        public void Warcraft_Snooper_Conversation_View_Test()
        {
            var infos = new List<FileInfo>
            {
                this.PrepareCaptureForProcessing(SnoopersPcaps.Default.warcraft_xberan33_1_wdat)
            };
            this.FrameworkController.ExportData(this.AvailableSnoopers, infos, this.CurrentTestBaseDirectory);


            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 526);

            var exportVm = new ExportVm(this.WindsorContainer, this.SnooperExports.ToList()[0]);
            var snooperWarcraftViewModel = new WarcraftMessageViewModel(this.WindsorContainer, exportVm, null);

            // The dispatcher thread
            var t = new Thread(() =>
            {
                var window = new WarcraftSnooperTestWindow();
                var uc = new WarcraftConversationView
                {
                    DataContext = snooperWarcraftViewModel
                };
                window.Content = uc;

                // Initiates the dispatcher thread shutdown when the window closes
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();

                window.Show();

                // Makes the thread support message pumping
                Dispatcher.Run();
            });

            // Configure the thread
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}

