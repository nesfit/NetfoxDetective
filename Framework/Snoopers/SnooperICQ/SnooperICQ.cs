// Copyright (c) 2017 Jan Pluskal
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
using Castle.Core.Internal;
using Castle.Windsor;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperICQ.Enums;
using Netfox.SnooperICQ.Models;

namespace Netfox.SnooperICQ
{
    public class SnooperICQ : SnooperBase
    {
        #region Overrides of SnooperBase
        public override string Name => "ICQ";

        public override string[] ProtocolNBARName => new[]
        {
            "aol-messenger"
        };

        public override string Description => "SnooperICQ reconstruct captured communication.";

        public override int[] KnownApplicationPorts => new[]
        {
            5190
        };

        public SnooperICQ() { } //Must exists for creation of snooper prototype to obtain decribing information

        public SnooperICQ(
            WindsorContainer investigationWindsorContainer,
            SelectedConversations conversations,
            DirectoryInfo exportDirectory,
            
            bool ignoreApplicationTags) : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) {}

        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportICQ();

        protected override void RunBody()
        {
            base.ProcessAssignedConversations();
            //this.SelectedConversations.LockSelectedConversations();

            //long conversationIndex;
            //var sleuthType = this.GetType();
            //ILxConversation currentConversation;

            ////Main cycle on all conversations
            //while(this.SelectedConversations.TryGetNextConversations(this.GetType(), out currentConversation, out conversationIndex))
            //{
            //    var selectedL7Conversations = new List<L7Conversation>();

            //    if(currentConversation.GetType() == typeof(L7Conversation)) //todo refactor to SnooperBase.. or make more readable.. to method or somenting...
            //    {
            //        selectedL7Conversations.Add(currentConversation as L7Conversation);
            //    }
            //    else if(currentConversation.GetType() == typeof(L4Conversation)) {
            //        selectedL7Conversations.AddRange((currentConversation as L4Conversation).L7Conversations);
            //    }
            //    else if(currentConversation.GetType() == typeof(L3Conversation)) { selectedL7Conversations.AddRange((currentConversation as L3Conversation).L7Conversations); }

            //    foreach(var selectedL7Conversation in selectedL7Conversations)
            //    {
            //        this._currentConversation = selectedL7Conversation;
            //        //eventExporter.ActualizeOpContext();

            //        if(!this.ForceExportOnAllConversations && !this.CurrentConversation.isXYProtocolConversation(this.ProtocolNBARName)) { continue; }
            //        // RunBody(CurrentConversation, conversationIndex);
            //        this.OnConversationProcessingBegin();

            //        this.ProcessConversation();

            //        this.OnConversationProcessingEnd();
            //    }
            //}
        }

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportICQ();

        protected override void ProcessConversation()
        {
            // we need a stream to read from
            var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.Breaked);
            // now we can create a reader that will be reading from the stream we just created


            //////////////////////////////////
            // reader will spawn messages, cycle through them
            do
            {
                var reader = new PDUStreamReader(stream, Encoding.ASCII)
                {
                    ReadBigEndian = true
                };
                this.OnBeforeProtocolParsing();

                var msg = new ICQMsg(reader);

                if(!msg.Valid)
                {
                    // parsing went wrong, we have to report it
                    this.SnooperExport.TimeStampFirst = msg.Timestamp;
                    this.SnooperExport.AddExportReport(ExportReport.ReportLevel.Warn, this.Name, "parsing of ICQ message failed: " + msg.InvalidReason, msg.ExportSources);
                    // skip processing, go to next message
                    continue;
                }
                this.OnAfterProtocolParsing();

                this.OnBeforeDataExporting();
                this.ProcessMsg(msg);
                this.OnAfterDataExporting();
            } while(stream.NewMessage());
        }
        #endregion

        private void ProcessMsg(ICQMsg msg)
        {
            switch(msg.Type)
            {
                case ICQMsgType.Msg:
                    if(!msg.Body.IsNullOrEmpty())
                    {
                        var exportObject = new SnooperExportedObjectICQ(this.SnooperExport)
                        {
                            Message = msg.Body,
                            Receiver = msg.Receiver,
                            Sender = msg.Sender,
                            TimeStamp = msg.Timestamp
                        };
                        exportObject.ExportSources.AddRange(msg.ExportSources);
                        this.SnooperExport.AddExportObject(exportObject);
                    }
                    break;
            }
        }
    }
}