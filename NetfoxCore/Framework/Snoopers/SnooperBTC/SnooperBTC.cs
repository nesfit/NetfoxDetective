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
using System.Collections.Generic;
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
using Netfox.Snoopers.SnooperBTC.Interfaces;
using Netfox.Snoopers.SnooperBTC.Models;

namespace Netfox.Snoopers.SnooperBTC
{
	public class SnooperBTC : SnooperBase, ISnooper
	{
		public override string Name => "BitCoin";

		public override string[] ProtocolNBARName => new[]
		{
			"bitcoin"
		};

		public override string Description => "example snooper for BitCoin";

		public override int[] KnownApplicationPorts => new[]
		{
			8333
		};

		private List<SnooperExportedDataObjectBTC> _pendingObjects = new List<SnooperExportedDataObjectBTC>();
        public SnooperBTC() { } //Must exists for creation of snooper prototype to obtain describing information
        public SnooperBTC(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags)
			: base(investigationWindsorContainer,conversations, exportDirectory, ignoreApplicationTags) {}

		protected new SnooperExportBTC SnooperExport => base.SnooperExport as SnooperExportBTC;
        public override SnooperExportBase PrototypeExportObject => new SnooperExportBTC();

	    protected override SnooperExportBase CreateSnooperExport() => new SnooperExportBTC();

        protected override void RunBody()
        {
            base.ProcessAssignedConversations();
            //this.SelectedConversations.LockSelectedConversations();

            //long conversationIndex;
            ////var sleuthType = this.GetType();
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

        protected override void ProcessConversation()
		{
			Console.WriteLine("SnooperBTC.ProcessConversation() called");

			// we need a stream to read from
			var stream = new PDUStreamBasedProvider(this.CurrentConversation, EfcPDUProviderType.SingleMessage);
			//var stream = new PDUStreamBasedProvider(base.CurrentConversation, EFcPDUProviderType.ContinueInterlay);
			// now we can create a reader that will be reading from the stream we just created
			//var reader = new PDUStreamReader(stream, Encoding.GetEncoding(437));
			var reader = new PDUStreamReader(stream, Encoding.Default, true);
			//var reader = new PDUStreamReader(stream);
			//var reader = new PDUStreamReader(stream);
			//reader.Reset();

			// reader will spawn messages, cycle through them
			do
			{
				this.OnBeforeProtocolParsing();
				// parse protocol

				// this is self parsing message, it just needs a reader to get data from
				var message = new BTCMsg(reader);
				if(!message.Valid)
				{
					// parsing went wrong, we have to report it
					this.SnooperExport.TimeStampFirst = message.Timestamp;
                    if(message.ExportSources == null || message.ExportSources.Count == 0)
                        Console.WriteLine("EMPTY SOURCES parsing of BTC message failed, frame numbers: "
                        + string.Join(",", message.Frames) + ": " +
                        message.InvalidReason);
                    this.SnooperExport.AddExportReport(
                        ExportReport.ReportLevel.Warn,
                        this.Name,
                        "parsing of BTC message failed, frames: "
						+ string.Join(",", message.Frames) + ": "+
						message.InvalidReason,
                        message.ExportSources);
					//Console.WriteLine(@"parsing of BTC message failed, frame numbers: " +
					//	string.Join(",", message.FrameNumbers.ToArray()) + ": " + message.InvalidReason);
					// skip processing, go to next message
					continue;
				}
				// parsing done
				this.OnAfterProtocolParsing();

				// start processing
				this.OnBeforeDataExporting();

				/*var exportedObject = new SnooperExportedDataObjectBTC(this.SnooperExport);
				var addObject = true;
				exportedObject.TimeStamp = message.Timestamp;*/
				SnooperExportedDataObjectBTC pendingObject = null;
                switch (message.Type)
				{
					case BTCMsg.BTCMsgType.Version:
						pendingObject = (from obj in this._pendingObjects
							where obj.Type == SnooperExportedDataObjectBTCType.Version_Verack
							select obj).FirstOrDefault();
						if(pendingObject == null) // first message "version" in the stream
						{
							pendingObject = new SnooperExportedDataObjectBTC(this.SnooperExport);
							pendingObject.TimeStamp = message.Timestamp;
							pendingObject.Type = SnooperExportedDataObjectBTCType.Version_Verack;
							pendingObject.State = SnooperExportedDataObjectBTCState.VersionReceived;
							pendingObject.ExportValidity = ExportValidity.ValidFragment;
							pendingObject.ClientAddress = message.SourceAddress;
							pendingObject.ServerAddress = message.DestinationAddress;
						    foreach(var exportSource in message.ExportSources) pendingObject.ExportSources.Add(exportSource);
							if(!pendingObject.UserAgents.Contains(message.UserAgent))
								pendingObject.UserAgents.Add(message.UserAgent);
							this._pendingObjects.Add(pendingObject);
						}
						else
                        {
                            foreach (var exportSource in message.ExportSources)
                                if (!pendingObject.ExportSources.Contains(exportSource))
                                    pendingObject.ExportSources.Add(exportSource);
                            if (pendingObject.State == SnooperExportedDataObjectBTCState.VerackReceived)
							{
								pendingObject.State = SnooperExportedDataObjectBTCState.VersionAndVerackReceived;
								pendingObject.ExportValidity = ExportValidity.ValidWhole;
                                if (!pendingObject.UserAgents.Contains(message.UserAgent))
									pendingObject.UserAgents.Add(message.UserAgent);
							}
						}
						break;
					case BTCMsg.BTCMsgType.Verack:
						pendingObject = (from obj in this._pendingObjects
											 where obj.Type == SnooperExportedDataObjectBTCType.Version_Verack
											 select obj).FirstOrDefault();
						if(pendingObject == null) // there is no object of type SnooperExportedDataObjectBTCType.Version_Verack created meaning message "version" is either lost or will come later
						{
							pendingObject = new SnooperExportedDataObjectBTC(this.SnooperExport);
							pendingObject.TimeStamp = message.Timestamp;
							pendingObject.Type = SnooperExportedDataObjectBTCType.Version_Verack;
							pendingObject.State = SnooperExportedDataObjectBTCState.VerackReceived;
							pendingObject.ExportValidity = ExportValidity.ValidFragment;
							pendingObject.ClientAddress = message.DestinationAddress;
							pendingObject.ServerAddress = message.SourceAddress;
                            foreach (var exportSource in message.ExportSources) pendingObject.ExportSources.Add(exportSource);
                            if (!pendingObject.UserAgents.Contains(message.UserAgent))
								pendingObject.UserAgents.Add(message.UserAgent);
							this._pendingObjects.Add(pendingObject);
						}
						else // object already exists
                        {
                            foreach (var exportSource in message.ExportSources)
                                if (!pendingObject.ExportSources.Contains(exportSource))
                                    pendingObject.ExportSources.Add(exportSource);
                            if (pendingObject.State == SnooperExportedDataObjectBTCState.VersionReceived)
							{
								pendingObject.State = SnooperExportedDataObjectBTCState.VersionAndVerackReceived;
								pendingObject.ExportValidity = ExportValidity.ValidWhole;
							}
                        }
						break;
					case BTCMsg.BTCMsgType.Tx:
						pendingObject = new SnooperExportedDataObjectBTC(this.SnooperExport);
						pendingObject.TimeStamp = message.Timestamp;
						pendingObject.Type = SnooperExportedDataObjectBTCType.Tx;
						pendingObject.ExportValidity = ExportValidity.ValidWhole;
						pendingObject.ClientAddress = message.DestinationAddress;
						pendingObject.ServerAddress = message.SourceAddress;
                        foreach (var exportSource in message.ExportSources) pendingObject.ExportSources.Add(exportSource);
                        this._pendingObjects.Add(pendingObject);
						break;
					case BTCMsg.BTCMsgType.Other:
						// do nothing
						break;
				}

				//export
				/*if (addObject)
				{
					//TODO there should be list of guids (frames, other objects)
					exportedObject.Source.Add(new Guid());
					this.SnooperExport.AddExportObject(exportedObject);
				}
				else
				{
					this.SnooperExport.DiscardExportObject(exportedObject);
				}

				base.OnAfterDataExporting();*/

				//} while(reader.NewMessage());
				if(reader.EndOfPDU) {
					try
					{
						reader.NewMessage();
					} catch (InvalidOperationException) { } }
            } while (!reader.EndOfStream);

			if (this._pendingObjects.Count > 0)
			{
				this.OnBeforeDataExporting();

				foreach(var pendingObject in this._pendingObjects)
				{
					this.SnooperExport.AddExportObject(pendingObject);
				}
				this._pendingObjects.Clear();

				this.OnAfterDataExporting();
			}
		}
	}
}
