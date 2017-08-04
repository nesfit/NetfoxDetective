// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Vit Janecek
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
using System.Linq;
using System.Text;
using Castle.Windsor;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperHTTP.Models;

namespace Netfox.SnooperHTTP
{
    public class SnooperHTTP : SnooperBase
    {
        public override string Name
        {
            get { return "HTTP"; }
        }

        public override string[] ProtocolNBARName
        {
            get
            {
                return new[]
                {
                    "http",
                    "ssl"
                };
            }
        }

        public override string Description
        {
            get { return string.Empty; }
        }

        public override int[] KnownApplicationPorts
        {
            get
            {
                return new[]
                {
                    80,
                    443
                };
            }
        }

        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportHTTP();

        protected override void ProcessConversation()
        {
            Console.WriteLine("SnooperHTTP.ProcessConversation() called");

            // we need a stream to read from
            var stream = this.CurrentConversation.Key.IsSet
                ? new PDUDecrypterBase(this.CurrentConversation, EfcPDUProviderType.Breaked)
                : new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.Breaked);

            // now we can create a reader that will be reading from the stream we just created
            // encoding set to codepage 437 as extended ascii 
            var reader = new PDUStreamReader(stream, Encoding.GetEncoding(437));

            // create export directory if it doesn't exist
            if (!this.ExportBaseDirectory.Exists)
            {
                this.ExportBaseDirectory.Create();
            }

            HTTPMsg request = null;
            // reader will spawn messages, cycle through them
            do
            {
                this.OnBeforeProtocolParsing();
                // parse protocol

                var message = new HTTPMsg(reader, this.ExportBaseDirectory);
                if(!message.Valid)
                {
                    this.SnooperExport.TimeStampFirst = message.TimeStamp;
                    // parsing went wrong, we have to report it
                    this.SnooperExport.AddExportReport(
                        ExportReport.ReportLevel.Warn, 
                        this.Name,
                        "parsing of HTTP message failed, frames: " + string.Join(",", message.Frames),
                        message.ExportSources);
                    Console.WriteLine("parsing of HTTP message failed, frame numbers: " + string.Join(",", message.Frames) + ": " + message.InvalidReason);
                    // skip processing, go to next message
                    continue;
                }

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing and export
                this.OnBeforeDataExporting();
                SnooperExportedDataObjectHTTP exportedObject;
                // process parsed structure
                switch (message.MessageType)
                {
                    case MessageType.REQUEST:
                        if(request != null)
                        {
                            /* export request with no response ?? */
                            exportedObject = new SnooperExportedDataObjectHTTP(this.SnooperExport);
                            exportedObject.TimeStamp = request.TimeStamp;
                            exportedObject.Message = request;
                            exportedObject.ExportSources.AddRange(request.ExportSources);
                            this.SnooperExport.AddExportObject(exportedObject);
                        }

                        request = message;
                        break;
                    case MessageType.RESPONSE:
                        if (request != null) message.PairMessages.Add(request);

                        request?.PairMessages.Add(message);

                        request = null;

                        /* always export with response message */

                        // export request
                        if (message.PairMessages.Any())
                        {
                            exportedObject = new SnooperExportedDataObjectHTTP(this.SnooperExport)
                            {
                                TimeStamp = message.PairMessages.First().TimeStamp,
                                Message = message.PairMessages.First()
                            };
                            exportedObject.ExportSources.AddRange(message.ExportSources);
                            this.SnooperExport.AddExportObject(exportedObject);
                        }

                        // export response
                        exportedObject = new SnooperExportedDataObjectHTTP(this.SnooperExport);
                        exportedObject.TimeStamp = message.TimeStamp;
                        exportedObject.Message = message;
                        exportedObject.ExportSources.AddRange(message.ExportSources);
                        this.SnooperExport.AddExportObject(exportedObject);

                        break;
                }

                this.OnAfterDataExporting();

                //finalize processing of current message, moving to next one
                // IMPORTANT !!! this has to be called after each message successfully processed
                // so correct connections between exported data and exported reports can be kept
                //base.ProcessingDone();
            } while(reader.NewMessage());

            // export last request if needed
            if(request != null)
            {
                var exportedObject = new SnooperExportedDataObjectHTTP(this.SnooperExport);
                exportedObject.TimeStamp = request.TimeStamp;
                exportedObject.Message = request;
                exportedObject.ExportSources.AddRange(request.ExportSources);
                this.SnooperExport.AddExportObject(exportedObject);
            }

        }

        protected override void RunBody()
        {
            base.ProcessAssignedConversations();
            //if(this.SelectedConversations == null) return;

            //this.SelectedConversations.LockSelectedConversations();

            //long conversationIndex;

            //ILxConversation currentConversation;
            ////Main cycle on all conversations
            //while (this.SelectedConversations.TryGetNextConversations(this.GetType(), out currentConversation, out conversationIndex))
            //{
            //    var selectedL7Conversations = new List<L7Conversation>();

            //    if (currentConversation.GetType() == typeof(L7Conversation)) //todo refactor to SnooperBase.. or make more readable.. to method or somenting...
            //    {
            //        selectedL7Conversations.Add(currentConversation as L7Conversation);
            //    }
            //    else if (currentConversation.GetType() == typeof(L4Conversation))
            //    {
            //        selectedL7Conversations.AddRange((currentConversation as L4Conversation).L7Conversations);
            //    }
            //    else if (currentConversation.GetType() == typeof(L3Conversation))
            //    {
            //        selectedL7Conversations.AddRange((currentConversation as L3Conversation).L7Conversations);
            //    }

            //    foreach (var selectedL7Conversation in selectedL7Conversations)
            //    {
            //        this._currentConversation = selectedL7Conversation;
            //        //eventExporter.ActualizeOpContext();

            //        if (!this.ForceExportOnAllConversations && !this.CurrentConversation.isXYProtocolConversation(this.ProtocolNBARName)) { continue; }
            //        // RunBody(CurrentConversation, conversationIndex);
            //        this.OnConversationProcessingBegin();

            //        this.ProcessConversation();

            //        this.OnConversationProcessingEnd();
            //    }
            //}
        }

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportHTTP();

        public SnooperHTTP()
        {
          //  this.wi.Register(Component.For<HTTPExportsView, IHTTPExportsView>().ImplementedBy<HTTPExportsView>().LifestyleTransient());
        }
        public SnooperHTTP(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
            : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags)
        { }
       
    }
}