// Copyright (c) 2017 Jan Pluskal, Pavel Beran
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
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperMinecraft.Models;

namespace Netfox.Snoopers.SnooperMinecraft
{
    public class SnooperMinecraft : SnooperBase
    {
        public override string Name => "Minecraft";

        // here we say what type of conversations (by NBAR name) we want to process
        public override string[] ProtocolNBARName => new[] { "Minecraft" };

        public override string Description => "Snooper for Minecraft chat";

		public override int[] KnownApplicationPorts => new[] { 25565 }; // base server port, client does not have constant

        internal new SnooperExportMinecraft SnooperExport => base.SnooperExport as SnooperExportMinecraft;
        public override SnooperExportBase PrototypeExportObject { get; } = new SnooperExportMinecraft();

        protected override SnooperExportBase CreateSnooperExport() => new SnooperExportMinecraft();
       
        protected override void RunBody()
        {
            base.ProcessAssignedConversations();
            //this.SelectedConversations.LockSelectedConversations();

            //long conversationIndex;
            //ILxConversation currentConversation;

            //// note: Tests on specific port will fail if this._forceExportOnAllConversations is not set true
            ////       It is because specific port dont pass protocol check
            //this.ForceExportOnAllConversations = true;

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


            //        if (!this.ForceExportOnAllConversations && !this.CurrentConversation.isXYProtocolConversation(this.ProtocolNBARName)) { continue; }

            //        this.OnConversationProcessingBegin();

            //        this.ProcessConversation();

            //        this.OnConversationProcessingEnd();
            //    }
            //}
        }

        protected override void ProcessConversation()
        {
            Console.WriteLine(@"SnooperMinecraft.ProcessConversation() called");

            // we need a stream to read from
            var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.SingleMessage);
            // now we can create a reader that will be reading from the stream we just created
            var reader = new BinaryReader(stream, Encoding.ASCII);

            // reader will spawn messages, cycle through them
            do
            {
                this.OnBeforeProtocolParsing();

                // parse protocol
                // this is self parsing message, it just needs a reader to get data from
                var message = new MinecraftMsg(reader);
                if (!message.Valid) // is not chat message
                    continue;

                // parsing done
                this.OnAfterProtocolParsing();

                // start processing
                this.OnBeforeDataExporting();

                var exportedObject = new SnooperExportedMinecraftMessage(this.SnooperExport)
                {
                    TimeStamp = message.Timestamp,
                    Message = message.MessageContent,
                    Sender = message.Sender,
                    Receiver = message.Receiver,
                    Text = message.Text,
                    Type = message.MessageType
                };

                // export
                exportedObject.ExportSources.Add(this.CurrentConversation);
		        this.SnooperExport.AddExportObject(exportedObject);

                this.OnAfterDataExporting();

                //finalize processing of current message, moving to next one
                // IMPORTANT !!! this has to be called after each message successfully processed
                // so correct connections between exported data and exported reports can be kept
                //base.ProcessingDone();
            } while (stream.NewMessage());
        }

        public SnooperMinecraft() { } //Must exists for creation of snooper prototype to obtain describing information
        public SnooperMinecraft(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags) : base(investigationWindsorContainer,conversations, exportDirectory, ignoreApplicationTags) {}
    }
}
