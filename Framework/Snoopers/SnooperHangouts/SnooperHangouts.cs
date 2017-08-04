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

using System;
using System.Collections.Generic;
using System.IO;
using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperHangouts.Models;
using Netfox.SnooperHangouts.Models.Events;
using Netfox.SnooperHTTP.Models;
using HttpRequestHeader = Netfox.SnooperHTTP.Models.HttpRequestHeader;

namespace Netfox.SnooperHangouts
{
    public class SnooperHangouts : SnooperBase
    {
        public SnooperHangouts() {}
        public SnooperHangouts(WindsorContainer investigationWindsorContainer, IEnumerable<SnooperExportBase> sourceExports, DirectoryInfo exportDirectory) : base(investigationWindsorContainer,sourceExports, exportDirectory) {}

        public override string Name => "iOS Hangouts";

        public override string[] ProtocolNBARName => new[]
        {
            "http",
            "ssl",
            "hangouts"
        };

        public override string Description => "iOS Hangouts Snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            80,
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new HangoutsSnooperExport();
        protected override SnooperExportBase CreateSnooperExport() { return new HangoutsSnooperExport(); }

        protected override void RunBody()
        {
            this.OnConversationProcessingBegin();
            this.ProcessConversation();
            this.OnConversationProcessingEnd();
        }

        protected override void ProcessConversation()
        {
            foreach(var snooperExportBase in this.SourceExports)
            {
                foreach(var exportedObject in snooperExportBase.ExportObjects)
                {
                    try
                    {
                        this.OnBeforeProtocolParsing();

                        var httpObject = exportedObject as SnooperExportedDataObjectHTTP;
                        if (httpObject?.Message == null) { continue; }

                        var httpMessage = httpObject.Message;

                        // Only requests
                        if (httpMessage.HTTPHeader?.Fields == null || httpMessage.HTTPHeader.Type != MessageType.REQUEST) { continue; }
                        var requestHeader = httpMessage.HTTPHeader as HttpRequestHeader;

                        // Check Host
                        var host = requestHeader.Fields["Host"][0];
                        if (!host.StartsWith("www.googleapis.com")) { continue; }

                        // Check Content-Type
                        var contentType = requestHeader.Fields["Content-Type"][0];
                        if (contentType != "application/x-protobuf") { continue; }

                        // Ensure that HTTP message has content
                        var content = httpMessage?.HTTPContent?.Content;
                        if (content == null || content.Length == 0) { continue; }

                        var uri = requestHeader.RequestURI;
                        HangoutsEventBase hangoutsEvent;
                        switch (uri)
                        {
                            case "/chat/v1ios/clients/setactiveclient":
                                hangoutsEvent = this.HandleEvent<HangoutsEventActiveClient>(this.SnooperExport, exportedObject.ExportSource, content);
                                break;
                            case "/chat/v1ios/conversations/sendchatmessage?alt=proto":
                                hangoutsEvent = this.HandleEvent<HangoutsEventChatMessage>(this.SnooperExport, exportedObject.ExportSource, content);
                                break;
                            case "/chat/v1ios/conversations/settyping?alt=proto":
                                hangoutsEvent = this.HandleEvent<HangoutsEventTyping>(this.SnooperExport, exportedObject.ExportSource, content);
                                break;
                            default:
                                continue;
                        }

                        hangoutsEvent.TimeStamp = httpMessage.TimeStamp;
                        hangoutsEvent.Frames = httpMessage.Frames;
                    }
                    catch(Exception)
                    {
                    }
                }

            }

            // Sort events in time
            this.SnooperExport.ExportObjects.Sort((e1, e2) => DateTime.Compare(e1.TimeStamp, e2.TimeStamp));
        }

        // Handle event of given event class
        protected HangoutsEventBase HandleEvent<THangoutsEventClass>(SnooperExportBase exportObject, IExportSource exportSource, byte[] content)
        {
            if(!typeof(THangoutsEventClass).IsSubclassOf(typeof(HangoutsEventBase)))
                throw new Exception("Not a HangoutsEventBase subclass");

            this.OnAfterProtocolParsing();
            this.OnBeforeDataExporting();
            
            // try, except create report
            var hangoutsEvent = (HangoutsEventBase)Activator.CreateInstance(typeof(THangoutsEventClass), new object[] { exportObject, content});
            hangoutsEvent.ExportSources.Add(exportSource);

            this.SnooperExport.AddExportObject(hangoutsEvent);
            this.OnAfterDataExporting();

            return hangoutsEvent;
        }
    }
}
