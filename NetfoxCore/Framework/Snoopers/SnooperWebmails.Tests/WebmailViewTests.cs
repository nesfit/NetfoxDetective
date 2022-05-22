﻿// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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

using System.IO;
using System.Linq;
using System.Threading;
using ApplicationProtocolExport.Tests;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.Models;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperWebmails.WPF.ViewModels;
using NUnit.Framework;
using TestWindow = Netfox.Snoopers.SnooperWebmails.WPF.Views.TestWindow;
using WebmailExportsView = Netfox.Snoopers.SnooperWebmails.WPF.Views.WebmailExportsView;

namespace Netfox.Snoopers.SnooperWebmails.Tests
{
    [TestFixture]
    class WebmailViewTests : SnooperBaseTests
    {   

        [Test]
        public void WindowTest()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.webmail_webmail_yahoo_rc4_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.pk_pem));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey();
                conversation.Key.ServerPrivateKey = pk;
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is Netfox.Snoopers.SnooperHTTP.SnooperHTTP), conversations, this.CurrentTestBaseDirectory, true);

            this.FrameworkController.ExportData(this.AvailableSnoopers.Where(x => x is SnooperWebmails), this.SnooperExports, this.CurrentTestBaseDirectory);


            var exportVm = new ExportVm(this.WindsorContainer, this.SnooperExports.ToList()[0]);
            var viewModel = new SnooperWebmailViewModel(this.WindsorContainer, exportVm, null);

            Thread t = new Thread(() =>
            {
                var window = new TestWindow();
                var uc = new WebmailExportsView //todo
                {
                    DataContext = viewModel
                };
                window.Content = uc;

                // Initiates the dispatcher thread shutdown when the window closes
                window.Closed += (s, e) => window.Dispatcher.InvokeShutdown();

                window.Show();

                // Makes the thread support message pumping
                System.Windows.Threading.Dispatcher.Run();
            });
            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();
        }
    }
}
