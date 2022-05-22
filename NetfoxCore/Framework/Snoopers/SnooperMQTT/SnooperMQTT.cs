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
using Netfox.Snoopers.SnooperMQTT.Models;

namespace Netfox.Snoopers.SnooperMQTT
{
    public class SnooperMQTT : SnooperBase
    {
        public SnooperMQTT() { }
        public SnooperMQTT(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
            : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) { }

        public override string Name => "MQTT";

        public override string[] ProtocolNBARName => new[]
        {
            "mqtt",
            "ssl",
        };

        public override string Description => "MQTT Snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            443
        };

        public override SnooperExportBase PrototypeExportObject { get; } = new MQTTSnooperExport();


        protected override void RunBody()
        {
            // Calls ProcessConversation for each conversation
            base.ProcessAssignedConversations();
        }

        protected override void ProcessConversation()
        {
            if(this.CurrentConversation == null) return;

            var stream = this.CurrentConversation.Key.IsSet
                ? new PDUDecrypterBase(this.CurrentConversation, EfcPDUProviderType.ContinueInterlay): new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.ContinueInterlay);

            // ASCII encoding
            //var reader = new PDUStreamReader(stream, Encoding.GetEncoding(437));
            var reader = new PDUStreamReader(stream, Encoding.ASCII, true);

            if(!this.ExportBaseDirectory.Exists)
            {
                this.ExportBaseDirectory.Create();
            }

            do {
                // Parse message
                this.OnBeforeProtocolParsing();
                var msg = new MQTTMsg(reader);
                if(msg.InvalidReason != null)
                {
                    var description = "Parsing of MQTT message failed";
                    if(msg.Frames != null) description += ", frame numbers: " + string.Join(",", msg.Frames);
                    if(msg.InvalidReason != null) description += ", reason: " + msg.InvalidReason;

                    this.SnooperExport.TimeStampFirst = msg.TimeStamp;
                    this.SnooperExport.AddExportReport(ExportReport.ReportLevel.Warn, this.Name, description, msg.ExportSources);
                    continue;
                }
                this.OnAfterProtocolParsing();

                // Export message
                this.OnBeforeDataExporting();
                
                this.SnooperExport.AddExportObject(msg);
                this.OnAfterDataExporting();

            } while(reader.NewMessage());

            // Sort messages in time
            //this.SnooperExport.ExportObjects.Sort((e1, e2) => DateTime.Compare(e1.TimeStamp, e2.TimeStamp));
        }

        protected override SnooperExportBase CreateSnooperExport() { return new MQTTSnooperExport(); }
    }
}