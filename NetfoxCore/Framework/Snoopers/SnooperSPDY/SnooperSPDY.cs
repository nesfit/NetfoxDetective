// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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
using System.Text;
using Castle.Windsor;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperSPDY.Models;

namespace Netfox.Snoopers.SnooperSPDY
{
    public class SnooperSPDY : SnooperBase
    {
        public SnooperSPDY() { }
        public SnooperSPDY(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
            : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags)
        { }

        public override string Name => "SPDY";

        public override string[] ProtocolNBARName => new[]
        {
            "spdy",
            "ssl",
        };

        public override string Description => "SPDY Snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            443
        };

        public override SnooperExportBase PrototypeExportObject { get; } = new SPDYSnooperExport();


        protected override void RunBody()
        {
            // Calls ProcessConversation for each conversation
            this.ProcessAssignedConversations();
        }

        protected override void ProcessConversation()
        {
            if (this.CurrentConversation == null) return;

            var stream = this.CurrentConversation.Key.IsSet
                ? new PDUDecrypterBase(this.CurrentConversation, EfcPDUProviderType.ContinueInterlay)
                : new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.ContinueInterlay);

            // ASCII encoding
            //var reader = new PDUStreamReader(stream, Encoding.GetEncoding(437));
            var reader = new PDUStreamReader(stream, Encoding.ASCII, true);

            if (!this.ExportBaseDirectory.Exists)
            {
                this.ExportBaseDirectory.Create();
            }

            while (true)
            {
                // Parse message
                this.OnBeforeProtocolParsing();
                var msg = new SPDYMsg(reader);

                if(msg.NothingToRead) { break; }

                if(msg.InvalidReason != null)
                {
                    var description = "Parsing of SPDY message failed";
                    if(msg.Frames != null) description += ", frame numbers: " + string.Join(",", msg.Frames);
                    if(msg.InvalidReason != null) description += ", reason: " + msg.InvalidReason;

                    this.SnooperExport.TimeStampFirst = msg.TimeStamp;
                    this.SnooperExport.AddExportReport(ExportReport.ReportLevel.Warn, this.Name, description, msg.ExportSources);

                    continue;
                }
                this.OnAfterProtocolParsing();

                // Export message
                this.OnBeforeDataExporting();
                var exportedObject = new SnooperSPDYExportedObject(this.SnooperExport, msg);
                this.SnooperExport.AddExportObject(exportedObject);
                this.OnAfterDataExporting();
            }
        }

        protected override SnooperExportBase CreateSnooperExport() { return new SPDYSnooperExport(); }
    }
}