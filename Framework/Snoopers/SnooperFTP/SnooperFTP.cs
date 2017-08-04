// Copyright (c) 2017 Jan Pluskal, Filip Karpisek
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
using Netfox.SnooperFTP.Models;

namespace Netfox.SnooperFTP
{
    public class SnooperFTP : SnooperBase
    {
        public SnooperFTP() { } //Must exists for creation of snooper prototype to obtain decribing information

        public override string Name => "FTP";

        // here we say what type of conversations (by NBAR name) we want to process
        public override string[] ProtocolNBARName => new[] { "ftp", "ftp-data" };

        public override string Description => "example snooper for FTP";

		public override int[] KnownApplicationPorts => new[] { 20, 21 };

		protected new SnooperExportFTP SnooperExport => base.SnooperExport as SnooperExportFTP;
        public override SnooperExportBase PrototypExportObject { get; } = new SnooperExportFTP();

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportFTP();

        protected override void RunBody()
        {
            //       this.SelectedConversations.LockSelectedConversations();

            //       long conversationIndex;

            //       //Main cycle on all conversations
            //       while (this.SelectedConversations.TryGetNextConversations(this.GetType(), out base._currentConversation, out conversationIndex))
            //       {
            //           //eventExporter.ActualizeOpContext();

            //           if (this._forceExportOnAllConversations || this.CurrentConversation.isXYProtocolConversation(this.ProtocolNBARName))
            //           {
            //               // RunBody(CurrentConversation, conversationIndex);
            //               base.OnConversationProcessingBegin();

            //               this.ProcessConversation();

            //base.OnConversationProcessingEnd();
            //           }
            //       }

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
            //    { selectedL7Conversations.Add(currentConversation as L7Conversation); }
            //    else if(currentConversation.GetType() == typeof(L4Conversation)) { selectedL7Conversations.AddRange((currentConversation as L4Conversation).L7Conversations); }
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

        protected override void ProcessConversation()
        {
            Console.WriteLine(@"SnooperFTP.ProcessConversation() called");

            // we need a stream to read from
            var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.SingleMessage);
            // now we can create a reader that will be reading from the stream we just created
            var reader = new PDUStreamReader(stream, Encoding.ASCII);

            // create export directory if it doesn't exist
            //if (!Directory.Exists(base.ExportBaseDirectory.FullName))
            //{
            //    Directory.CreateDirectory(base.ExportBaseDirectory.FullName);
            //}

            var dataFileName = this.GetDataFileName(this.ExportBaseDirectory.FullName);

            // reader will spawn messages, cycle through them
            do
            {
                this.OnBeforeProtocolParsing();
                // parse protocol

                // this is self parsing message, it just needs a reader to get data from
                var message = new FTPMsg(reader);
                if (!message.Valid)
                {
                    // parsing went wrong, we have to report it
                    this.SnooperExport.TimeStampFirst = message.Timestamp;
                    this.SnooperExport.AddExportReport(
                        ExportReport.ReportLevel.Warn,
                        this.Name,
                        "parsing of FTP message failed, frame numbers: " +
                        string.Join(",", message.Frames) + ": " +
                        message.InvalidReason,
                        message.ExportSources);
                    Console.WriteLine(@"parsing of FTP message failed, frame numbers: " +
                                      string.Join(",", message.Frames) + ": " + message.InvalidReason);
                    // skip processing, go to next message
                    continue;
                }

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing
                this.OnBeforeDataExporting();
                
                var exportedObject = new SnooperExportedDataObjectFTP(this.SnooperExport);
                var addObject = true;
	            exportedObject.TimeStamp = message.Timestamp;

                // process parsed structure
                switch (message.Type)
                {
                    case FTPMsg.FTPMsgType.USER:
                        //Console.WriteLine("  user: " + _message.MessageContent);
                        exportedObject.Command = "USER";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.LIST:
                        //Console.WriteLine("  directory listing");
                        exportedObject.Command = "DIRECTORY LIST";
                        break;
                    case FTPMsg.FTPMsgType.PASS:
                        //Console.WriteLine("  password: " + _message.MessageContent);
                        exportedObject.Command = "PASSWORD";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.DELE:
                        //Console.WriteLine("  deleted file: " + _message.MessageContent);
                        exportedObject.Command = "DELETED FILE";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.PORT:
                        //Console.WriteLine("  new connection address: " + _message.MessageContent);
                        exportedObject.Command = "PORT";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.PWD:
                        //Console.WriteLine("  directory path: " + _message.MessageContent);
                        exportedObject.Command = "PATH";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.RETR:
                        //Console.WriteLine("  downloaded file: " + _message.MessageContent);
                        exportedObject.Command = "DOWNLOAD";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.STOR:
                        //Console.WriteLine("  uploaded file: " + _message.MessageContent);
                        exportedObject.Command = "UPLOAD";
                        exportedObject.Value = message.MessageContent;
                        break;
                    case FTPMsg.FTPMsgType.DATA:
                        var file = new FileStream(dataFileName, FileMode.Append);
                        file.Write(message.DataContent, 0, message.DataContent.Length);
                        file.Close();
                        //Console.WriteLine("  data dumped to " + _dataFileName);
                        exportedObject.Command = "DATA";
                        exportedObject.Value = dataFileName;
                        break;
                    default:
                        //Console.WriteLine("  unknown type of FTP message");
                        addObject = false;
                        break;
                }
                //export
	            if(addObject)
	            {
		            //TODO there should be list of guids (frames, other objects)
                    foreach(var exportSource in message.ExportSources)
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

        /// <summary>
        ///     gets filename in format srcIP-srcPort_dstIP-dstPort_dateTime.data
        /// </summary>
        /// <param name="path">parent directory</param>
        /// <returns>string</returns>
        private string GetDataFileName(string path)
        {
            //Console.WriteLine("GetDataFileName() called");
            var fileName = Path.Combine(path,
                this.CurrentConversation.SourceEndPoint.ToString().Replace(':', '-') + '_' + this.CurrentConversation.DestinationEndPoint.ToString().Replace(':', '-')
                + this.CurrentConversation.ConversationStats.FirstSeen.ToString("_yyyy-MM-dd_HH-mm-ss") + ".data");
            if(File.Exists(fileName)) { File.Delete(fileName); }
            return fileName;
        }

	    public SnooperFTP(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
            : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags)
        { }
    }
}
