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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Netfox.Core.Enums;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperXMPP.Models;

namespace Netfox.Snoopers.SnooperXMPP
{
    public class SnooperXMPP : SnooperBase
    {
        private List<XMPPMsg> _messageBuffer;
        private string _reciever = String.Empty;
        private string _sender = String.Empty;

        public SnooperXMPP() { } //Must exists for creation of snooper prototype to obtain decribing information

        public SnooperXMPP(
            WindsorContainer investigationWindsorContainer,
            SelectedConversations conversations,
            DirectoryInfo exportDirectory,
            
            bool ignoreApplicationTags) : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) {}

        #region Overrides of SnooperBase
        public override string Name => "XMPP";

        public override string[] ProtocolNBARName => new[]
        {
            "xmpp-client"
        };

        public override string Description => "SnooperXMPP reconstruct captured communication.";

        public override int[] KnownApplicationPorts => new[]
        {
            5222
        };

        public override SnooperExportBase PrototypeExportObject { get; } = new SnooperExportXMPP();

        protected override void RunBody()
        {
            Debug.WriteLine(@"SnooperXMPP.RunBody() called");
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

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportXMPP();

        protected override void ProcessConversation()
        {
            this._messageBuffer = new List<XMPPMsg>();
            Debug.WriteLine(@"SnooperXMPP.ProcessConversation() called [" + this.CurrentConversation.SourceEndPoint + "->" + this.CurrentConversation.DestinationEndPoint + "]");

            // we need a stream to read from
            var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.Breaked);
            // now we can create a reader that will be reading from the stream we just created
            var reader = new PDUStreamReader(stream, Encoding.UTF8);

            this.OnBeforeProtocolParsing();
            //////////////////////////////////
            // reader will spawn messages, cycle through them
            do
            {
                // parse protocol

                // this is self parsing message, it just needs a reader to get data from
                var message = new XMPPMsg(reader);
                if(!message.Valid)
                {
                    // parsing went wrong, we have to report it
                    this.SnooperExport.TimeStampFirst = message.Timestamp;
                    this.SnooperExport.AddExportReport(ExportReport.ReportLevel.Warn, this.Name,
                        "parsing of XMPP message failed, frame numbers: " +  message.Frames.ToArray() + ": " + message.InvalidReason, message.ExportSources);
                    // skip processing, go to next message
                    continue;
                }

                // process parsed structure
                switch(message.MsgType)
                {
                    case XMPPMsgType.MSG:
                        this._messageBuffer.Add(message);
                        string sender;
                        string receiver;
                        if(message.Direction == DaRFlowDirection.down)
                        {
                            sender = message.From ?? string.Empty;
                            receiver = message.To ?? string.Empty;
                        }
                        else
                        {
                            sender = message.To ?? string.Empty;
                            receiver = message.From ?? string.Empty;
                        }

                        if(sender.Length > this._sender.Length) { this._sender = sender; }
                        if(receiver.Length > this._reciever.Length) { this._reciever = receiver; }

                        break;
                }
            } while(reader.NewMessage());
            //////////////////////////////////
            this.OnAfterProtocolParsing();

            // start processing
            this.OnBeforeDataExporting();

            foreach(var msg in this._messageBuffer)
            {
                if(string.IsNullOrEmpty(msg.MsgBody)) { continue; }
                var exportedObject = new SnooperExportedObjectXMPP(this.SnooperExport)
                {
                    TimeStamp = msg.Timestamp,
                    Message = msg.MsgBody
                };
                if(msg.Direction == DaRFlowDirection.down)
                {
                    exportedObject.Sender = this._sender;
                    exportedObject.Receiver = this._reciever;
                }
                else
                {
                    exportedObject.Receiver = this._sender;
                    exportedObject.Sender = this._reciever;
                }
                exportedObject.ExportSources.AddRange(msg.ExportSources);
                this.SnooperExport.AddExportObject(exportedObject);
            }

            this.OnAfterDataExporting();
        }
        #endregion
    }
}