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

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Netfox.Detective.ViewModelsDataEntity.Exports;
using Netfox.Framework.ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using Netfox.SnooperHTTP.Models;
using NUnit.Framework;
using Netfox.SnooperHTTP.WPF.View;
using Netfox.SnooperHTTP.WPF.ViewModels;
using HTTPExportsView = Netfox.SnooperHTTP.WPF.View.HTTPExportsView;
using TestWindow = Netfox.SnooperHTTP.WPF.View.TestWindow;

namespace Netfox.SnooperHTTP.Tests
{
    [TestFixture]
    class SnooperHTTPViewTests : SnooperBaseTests
    {
        [Test][Explicit][Category("Explicit")]
        public void WindowTest()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.webmail_webmail_gmail_pcapng));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PrivateKeys.Default.pk_pem);

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey
                {
                    ServerPrivateKey = pk
                };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);

            var exportsObservableCollection = new ObservableCollection<HTTPMsg>();

            foreach (var exportobject in this.SnooperExports.ToList()[0].ExportObjects)
            {
                var httpExport = exportobject as SnooperExportedDataObjectHTTP;
                if (httpExport != null) { exportsObservableCollection.Add(httpExport.Message); }
            }

            var exportVm = new ExportVm(this.WindsorContainer,this.SnooperExports.ToList()[0]);
            var viewModel = new SnooperHTTPViewModel(this.WindsorContainer, exportVm, null);

            Thread t = new Thread(() =>
            {
                var window = new TestWindow();
                var uc = new HTTPExportsView
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
