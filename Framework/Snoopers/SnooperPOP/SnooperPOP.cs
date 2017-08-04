// Copyright (c) 2017 Jan Pluskal, Martin Mares
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

using System.Diagnostics;
using System.IO;
using System.Text;
using Castle.Windsor;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.Framework.Models.Snoopers.Email;
using Netfox.SnooperPOP.Models;

namespace Netfox.SnooperPOP
{
    public class SnooperPOP : SnooperBase
    {
        public SnooperPOP() { } //Must exists for creation of snooper prototype to obtain decribing information

        public override string Name => "POP";

        public override string[] ProtocolNBARName => new[] { "pop3" };

        public override string Description => "example snooper for POP3";

        public override int[] KnownApplicationPorts => new[] { 110 };

        protected new SnooperExportPOP SnooperExport => base.SnooperExport as SnooperExportPOP;

        public override SnooperExportBase PrototypExportObject => new SnooperExportPOP(this.ExportBaseDirectory);

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportPOP(this.ExportBaseDirectory);

        protected override void RunBody()
        {
            Debug.WriteLine(@"SnooperPOP.RunBody() called");
            base.ProcessAssignedConversations();
            //this.SelectedConversations.LockSelectedConversations();

            //long conversationIndex;
            //var sleuthType = this.GetType();
            //ILxConversation currentConversation;
            ////Main cycle on all conversations
            //while (this.SelectedConversations.TryGetNextConversations(this.GetType(), out currentConversation, out conversationIndex))
            //{
            //    var selectedL7Conversations = new List<L7Conversation>();

            //    if (currentConversation.GetType() == typeof(L7Conversation)) //todo refactor to SnooperBase.. or make more readable.. to method or somenting...
            //    { selectedL7Conversations.Add(currentConversation as L7Conversation); }
            //    else if (currentConversation.GetType() == typeof(L4Conversation)) { selectedL7Conversations.AddRange((currentConversation as L4Conversation).L7Conversations); }
            //    else if (currentConversation.GetType() == typeof(L3Conversation)) { selectedL7Conversations.AddRange((currentConversation as L3Conversation).L7Conversations); }

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

        protected override void ProcessConversation()
        {
            Debug.WriteLine(@"SnooperPOP.ProcessConversation() called [" + this.CurrentConversation.SourceEndPoint + "->" + this.CurrentConversation.DestinationEndPoint + "]");

            // we need a stream to read from
            var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.SingleMessage);
            // now we can create a reader that will be reading from the stream we just created
            var reader = new PDUStreamReader(stream, Encoding.ASCII);

            // reader will spawn messages, cycle through them
            do
            {
                this.OnBeforeProtocolParsing();
                // parse protocol

                // this is self parsing message, it just needs a reader to get data from
                var message = new POPMsg(reader);
                if (!message.Valid)
                {
                    // parsing went wrong, we have to report it
                    this.SnooperExport.TimeStampFirst = message.Timestamp;
                    this.SnooperExport.AddExportReport(
                        ExportReport.ReportLevel.Warn,
                        this.Name,
                        "parsing of POP message failed, frame numbers: " +
                        string.Join(",", message.Frames) + ": " +
                        message.InvalidReason,
                        message.ExportSources);
                    Debug.WriteLine(@"parsing of POP message failed, frame numbers: " +
                                      string.Join(",", message.Frames) + ": " + message.InvalidReason);
                    // skip processing, go to next message
                    continue;
                }

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing
                this.OnBeforeDataExporting();

                var exportedObject = new SnooperExportedDataObjectPOP(this.SnooperExport);
                var addObject = true;
                exportedObject.TimeStamp = message.Timestamp;

                // process parsed structure
                switch (message.Type)
                {
                    case POPMsg.POPMsgType.USER:
                        //Debug.WriteLine("  user: " + _message.MessageContent);
                        exportedObject.Type = "USER";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case POPMsg.POPMsgType.PASS:
                        //Debug.WriteLine("  password: " + _message.MessageContent);
                        exportedObject.Type = "PASS";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case POPMsg.POPMsgType.RETR:
                        //Debug.WriteLine("  password: " + _message.MessageContent);

                        ///TODO clean up this class.... Improve object design!
                        byte[] toBytes = Encoding.ASCII.GetBytes(message.MessageContent);

                        var mimeEmail = new MIMEemail(this.SnooperExport, toBytes, EMailContentType.Whole);
                        mimeEmail.EMailType = EMailType.POP3OrgEmail;
                        exportedObject.TimeStamp = message.Timestamp;

                        foreach (var exportSource in message.ExportSources) 
                            mimeEmail.ExportSources.Add(exportSource);
                        this.SnooperExport.AddExportObject(mimeEmail);
                        addObject = false;
                        break;
                    default:
                        //Debug.WriteLine("  unknown type of FTP message");
                        addObject = false;
                        break;
                }
                //export
                if (addObject)
                {
                    //TODO there should be list of guids (frames, other objects)
                    foreach (var exportSource in message.ExportSources)
                        exportedObject.ExportSources.Add(exportSource);
                    this.SnooperExport.AddExportObject(exportedObject);
                }
                else
                {
                    this.SnooperExport.DiscardExportObject(exportedObject);
                }

                this.OnAfterDataExporting();

                //finalize processing of current message, moving to next one
                // IMPORTANT !!! this has to be called after each message successfully processed
                // so correct connections between exported data and exported reports can be kept
                //base.ProcessingDone();
            } while (reader.NewMessage());
        }

        public SnooperPOP(
            WindsorContainer investigationWindsorContainer,
            SelectedConversations conversations,
            DirectoryInfo exportDirectory,
            
            bool ignoreApplicationTags) : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) {}
    }
}
