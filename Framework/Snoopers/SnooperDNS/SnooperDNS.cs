// Copyright (c) 2017 Jan Pluskal, Pavol Vican
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
using System.IO;
using System.Text;
using Castle.Windsor;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperDNS.Models;

namespace Netfox.SnooperDNS
{
    public class SnooperDNS : SnooperBase
    {
        public override string Name => "DNS";

        public override string[] ProtocolNBARName => new[]
        {
            "dns"
        };

        public override string Description => string.Empty;

        public override int[] KnownApplicationPorts => new[]
        {
            53
        };

        protected new SnooperExportDNS SnooperExport => base.SnooperExport as SnooperExportDNS;
        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportDNS();
        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportDNS();

        protected override void RunBody()
        {
            base.ProcessAssignedConversations();
        }

        protected override void ProcessConversation()
        {
            Console.WriteLine(@"SnooperDNS.ProcessConversation() called");

            // we need a stream to read from
            var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.Mixed);
            // now we can create a reader that will be reading from the stream we just created
            var reader = new PDUStreamReader(stream, Encoding.GetEncoding(437), true);
            
            // reader will spawn messages, cycle through them
            do
            {
                this.OnBeforeProtocolParsing();
                // parse protocol

                // this is self parsing message, it just needs a reader to get data from
                var message = new DNSParseMsg(reader);
                if (!message.Valid)
                {
                    // parsing went wrong, we have to report it
                    this.SnooperExport.TimeStampFirst = message.Timestamp;
                    this.SnooperExport.AddExportReport(
                        ExportReport.ReportLevel.Warn,
                        this.Name,
                        "parsing of DNS message failed, frame numbers: " +
                        string.Join(",", message.Frames) + ": " +
                        message.InvalidReason,
                        message.ExportSources);
                    Console.WriteLine(@"parsing of DNS message failed, frame numbers: " +
                                      string.Join(",", message.Frames) + ": " + message.InvalidReason);
                    // skip processing, go to next message
                    continue;
                }

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing
                this.OnBeforeDataExporting();

                var exportedObject = new SnooperExportedDataObjectDNS(this.SnooperExport)
                {
                    TimeStamp = message.Timestamp,
                    MessageId = message.MessageId,
                    Flags = message.Flags,
                    Queries = message.Queries,
                    Answer = message.Answer,
                    Authority = message.Authority,
                    Additional = message.Additional
                };

                //export
                    //TODO there should be list of guids (frames, other objects)
                    foreach (var exportSource in message.ExportSources)
                        exportedObject.ExportSources.Add(exportSource);
                    this.SnooperExport.AddExportObject(exportedObject);


                this.OnAfterDataExporting();

                //finalize processing of current message, moving to next one
                // IMPORTANT !!! this has to be called after each message successfully processed
                // so correct connections between exported data and exported reports can be kept
                //base.ProcessingDone();
            } while (reader.NewMessage());
        }

        public SnooperDNS() { }

        public SnooperDNS(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
            : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags)
        { }

    }
}