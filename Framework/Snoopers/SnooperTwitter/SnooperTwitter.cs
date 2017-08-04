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
using Netfox.SnooperSPDY.Models;
using Netfox.SnooperTwitter.Models;
using Netfox.SnooperTwitter.Models.Events;

namespace Netfox.SnooperTwitter
{
    public class SnooperTwitter : SnooperBase
    {
        public SnooperTwitter() { }

        public SnooperTwitter(
            WindsorContainer investigationWindsorContainer,
            IEnumerable<SnooperExportBase> sourceExports,
            DirectoryInfo exportDirectory) : base(investigationWindsorContainer, sourceExports, exportDirectory)
        { }

        public override string Name => "iOS Twitter";

        public override string[] ProtocolNBARName => new[]
        {
            "spdy",
            "ssl",
            "twitter"
        };

        public override string Description => "iOS Twitter Snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new TwitterSnooperExport();
        protected override SnooperExportBase CreateSnooperExport() { return new TwitterSnooperExport(); }

        protected override void RunBody()
        {
            this.OnConversationProcessingBegin();
            this.ProcessConversation();
            this.OnConversationProcessingEnd();
        }

        protected override void ProcessConversation()
        {
            foreach (var snooperExportBase in this.SourceExports)
            {
                foreach (SnooperSPDYExportedObject exportedObject in snooperExportBase.ExportObjects)
                {
                    this.OnBeforeProtocolParsing();
                    
                    var spdyMessage = exportedObject.Message;

                    if(spdyMessage.Type != MessageType.Request)
                        continue;

                    var requestHeader = spdyMessage.Header as SPDYRequestHeader;
                    switch(requestHeader.Path) {
                        case "/1.1/dm/new.json":
                            this.HandleEvent<TwitterEventSendMessage>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        case "/1.1/statuses/update.json":
                            this.HandleEvent<TwitterEventCreateTweet>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        case "/1.1/search/typeahead.json":
                        case "/1.1/search/universal.json":
                            this.HandleEvent<TwitterEventSearch>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        case "/1.1/timeline/home.json":
                            this.HandleEvent<TwitterEventTimelineView>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        case "/1.1/timeline/user.json":
                            this.HandleEvent<TwitterEventUserTimelineView>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        case "/1.1/friendships/lookup.json":
                            this.HandleEvent<TwitterEventUserLookup>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        case "/1.1/friendships/show.json":
                            this.HandleEvent<TwitterEventUserShow>(this.SnooperExport, exportedObject.ExportSource, spdyMessage);
                            break;
                        default:
                            continue;
                    }

                }
                
            }
        }

        protected TwitterEventBase HandleEvent<TTwitterEventClass>(SnooperExportBase exportObject, IExportSource exportSource, SPDYMsg spdyMsg)
        {
            if (!typeof(TTwitterEventClass).IsSubclassOf(typeof(TwitterEventBase)))
                throw new Exception("Not a TwitterEventBase subclass");

            TwitterEventBase twitterEvent;
            try {
                twitterEvent = (TwitterEventBase)Activator.CreateInstance(typeof(TTwitterEventClass), new object[] { exportObject, spdyMsg });
            }
            catch(Exception) {
                return null;
            }
            
            this.OnAfterProtocolParsing();

            this.OnBeforeDataExporting();
            twitterEvent.ExportSources.Add(exportSource);
            this.SnooperExport.AddExportObject(twitterEvent);
            this.OnAfterDataExporting();

            return twitterEvent;
        }
    }
}