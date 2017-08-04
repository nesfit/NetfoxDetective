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
using Netfox.SnooperSMTP.Models;

namespace Netfox.SnooperSMTP
{
    public class SnooperSMTP : SnooperBase
    {
        public SnooperSMTP() { } //Must exists for creation of snooper prototype to obtain decribing information

        public override string Name => "SMTP";

        public override string[] ProtocolNBARName => new[] { "smtp" };

        public override string Description => "example snooper for SMTP";

        public override int[] KnownApplicationPorts => new[] { 25, 587 };

        protected new SnooperExportSMTP SnooperExport => base.SnooperExport as SnooperExportSMTP;

        public override SnooperExportBase PrototypExportObject => new SnooperExportSMTP(this.ExportBaseDirectory);

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportSMTP(this.ExportBaseDirectory);

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
            Debug.WriteLine(@"SnooperSMTP.ProcessConversation() called [" + this.CurrentConversation.SourceEndPoint + "->" + this.CurrentConversation.DestinationEndPoint + "]");

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
                var message = new SMTPMsg(reader);
                if(!message.Valid)
                {
                    // parsing went wrong, we have to report it
                    this.SnooperExport.TimeStampFirst = message.Timestamp;
                    this.SnooperExport.AddExportReport(
                        ExportReport.ReportLevel.Warn,
                        this.Name,
                        "parsing of SMTP message failed, frame numbers: " + string.Join(",", message.Frames) + ": " +
                        message.InvalidReason,
                        message.ExportSources);
                    Debug.WriteLine(@"parsing of SMTP message failed, frame numbers: " + string.Join(",", message.Frames) + ": " + message.InvalidReason);
                    // skip processing, go to next message
                    continue;
                }

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing
                this.OnBeforeDataExporting();

                // process parsed structure
                switch (message.Type)
                {
                    case SMTPMsg.SMTPMsgType.MAIL:
                        //Debug.WriteLine("  password: " + _message.MessageContent);
                        //exportedObject.Type = "RETR";
                        //exportedObject.Value = message.MessageContent;
                        byte[] toBytes = Encoding.ASCII.GetBytes(message.MessageContent);

                        var exportedObject = new MIMEemail(this.SnooperExport, toBytes, EMailContentType.Whole);
                        exportedObject.EMailType = EMailType.SMTPOrgEmail;
                        exportedObject.TimeStamp = message.Timestamp;

                        foreach (var exportSource in message.ExportSources)
                            exportedObject.ExportSources.Add(exportSource);
                        this.SnooperExport.AddExportObject(exportedObject);
                        break;
                    default:
                        //Debug.WriteLine("  unknown type of FTP message");
                        break;
                }

                this.OnAfterDataExporting();


            } while (reader.NewMessage());
        }

        public SnooperSMTP(
            WindsorContainer investigationWindsorContainer,
            SelectedConversations conversations,
            DirectoryInfo exportDirectory,
            
            bool ignoreApplicationTags) : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) { }
    }
}
