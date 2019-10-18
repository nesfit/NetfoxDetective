// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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

using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperMinecraft.WPF.Views;
using Netfox.SnooperMinecraft.WPF.ViewModels;
using NUnit.Framework;
using MinecraftMsgView = Netfox.SnooperMinecraft.WPF.Views.MinecraftMsgView;
using MinecraftSnooperTestWindow = Netfox.SnooperMinecraft.WPF.Views.MinecraftSnooperTestWindow;

namespace Netfox.SnooperMinecraft.Tests.Tests
{
    class MinecraftViewTests : SnooperBaseTests
    {
        [Test]
        [Explicit][Category("Explicit")]
        public void Minecraft_Snooper_View_Test()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.minecraft_xberan33_minecraft_search_asd_pcap));
            var conversations = this.L7Conversations.Where(c => c.IsXyProtocolConversation("Minecraft")).ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            Assert.IsTrue(this.SnooperExports.Count == 1);
            Assert.IsTrue(this.SnooperExports.First().ExportObjects.Count == 11);

            var exportVm = new ExportVm(this.WindsorContainer, this.SnooperExports.ToList()[0]);
            var snooperMinecraftViewModel = new MinecraftMessageViewModel(this.WindsorContainer, exportVm, null);

            MinecraftSnooperTestWindow window;
            // The dispatcher thread
            var t = new Thread(() =>
            {
                window = new MinecraftSnooperTestWindow();
                var uc = new MinecraftMsgView
                {
                    DataContext = snooperMinecraftViewModel
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

