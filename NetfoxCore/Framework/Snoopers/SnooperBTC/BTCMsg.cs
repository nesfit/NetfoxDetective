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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Core.Database.Wrappers;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Snoopers.SnooperBTC.Models;

namespace Netfox.Snoopers.SnooperBTC
{
	public class BTCMsg
	{
		public enum BTCMsgType
		{
			Version,
			Verack,
			Tx,
			Other
		}

		private readonly PDUStreamReader _reader;
	    private PersistableJsonSerializableGuid _framesGuids;

	    [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FramesGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }
        public string InvalidReason { get; private set; } = String.Empty;
		public DateTime Timestamp { get; private set; }
        public BTCMsgType Type { get; private set; } = BTCMsgType.Other;
		public bool Valid { get; private set; } = true;
        public List<IExportSource> ExportSources { get; private set; } = new List<IExportSource>();

        //content of version message
        //public string ReceivingNodeAddress;
        //public UInt16 ReceivingNodePort;
        //public string EmmittingNodeAddress;
        //public UInt16 EmmittingNodePort;
        public string UserAgent { get; private set; } = string.Empty;

		//content of version, verack and tx messages
		public IPAddressEF SourceAddress { get; private set; }
        public IPAddressEF DestinationAddress { get; private set; }

        //content of tx message
        public List<BTCTransaction> InputTransactions { get; private set; } = new List<BTCTransaction>();
		public List<BTCTransaction> OutputTransactions { get; private set; } = new List<BTCTransaction>();

		public BTCMsg(PDUStreamReader reader)
		{
			// fill default values and store things we'll need later
			this._reader = reader;

            //this.ReceivingNodeAddress = string.Empty;
            //this.ReceivingNodePort = 0;
            //this.EmmittingNodeAddress = string.Empty;
            //this.EmmittingNodePort = 0;

            //this.Parse();
			Console.WriteLine("BTCMsg: this._reader.EndOfStream: "+ this._reader.EndOfStream);
			//if (!this._reader.EndOfStream)
			//{
				// do the parsing itself
				this.Parse();
			//}
		}

		private void Parse()
		{
			// transform reader to stream provider to get timestamp and frame numbers values
			var streamProvider = this._reader.BaseStream as PDUStreamBasedProvider;
			if (streamProvider.GetCurrentPDU() == null)
			{
				this.Valid = false;
				this.InvalidReason = "empty conversation";
                this.ExportSources.Add(streamProvider.Conversation);
                this._reader.NewMessage();
				return;
			}
		    this.Timestamp = streamProvider.GetCurrentPDU().FirstSeen;
            this.ExportSources.Add(streamProvider.GetCurrentPDU());

            //var bytes = this._reader.ReadToEnd().ToCharArray();
            //var bytes = streamProvider.GetCurrentPDU().GetPDUByteArr();
            var headerFirstHalfLength = 16;
			var headerSecondHalfLength = 8;
			var headerLength = headerFirstHalfLength + headerSecondHalfLength;
			var headerFirstHalf = new byte[headerFirstHalfLength];
			var headerSecondHalf = new byte[headerSecondHalfLength];
            if (this._reader.Read(headerFirstHalf, 0, headerFirstHalfLength) < headerFirstHalfLength)
			{
				this.Valid = false;
				this.InvalidReason = "message too short (less than "+ headerFirstHalfLength + "B - for header)";
				this._reader.NewMessage();
				return;
			}
			//this.Frames.Contains(8398) || this.Frames.Contains(8399)
			//if(bytes.Length < 24)
			//{
			//	this.Valid = false;
			//	this.InvalidReason = "message too short (less than 24B - for header)";
			//	return;
			//}

			//var magic = bytes.Substring(0, 4); // not useful
			//var command = header.Substring(4,12).Trim(new char[] {'\0'}); // trim trailing zeros
			char[] commandArray = new char[12];
			Array.Copy(headerFirstHalf, 4, commandArray, 0, 12);
			var command = new string(commandArray).Trim('\0'); // trim trailing zeros
			//Console.WriteLine("command: "+command+ ", frame numbers: "+ string.Join(",", this.Frames.ToArray()));

			if(command.Length == 0)
			{
				this.Valid = false;
				this.InvalidReason = "command is empty";
				this._reader.NewMessage();
				return;
			}
			foreach(var commandChar in command.ToCharArray()) {
				if(commandChar < 'a' || commandChar > 'z')
				{
					this.Valid = false;
					this.InvalidReason = "command ("+command+") contains invalid characters";
					this._reader.NewMessage();
					return;
				}
			}

			if (this._reader.Read(headerSecondHalf, 0, headerSecondHalfLength) < headerSecondHalfLength)
			{
				this.Valid = false;
				this.InvalidReason = "message too short (less than " + headerFirstHalfLength + headerSecondHalfLength + "B - for header)";
				this._reader.NewMessage();
				return;
			}

			byte[] lengthArray = new byte[4]; //bytes.Substring(16, 4).ToArray();
			Array.Copy(headerSecondHalf, 0, lengthArray, 0, 4);
			int length = 0;
			int multiplier = 1;
			foreach(var lengthByte in lengthArray)
			{
				length += lengthByte * multiplier;
				multiplier *= 256;
			}

			var body = new byte[length];
			var readBytes = this._reader.Read(body, 0, length);
			while(readBytes < length) //message is spread accross multiple PDUs
			{
				this._reader.NewMessage();
                this.ExportSources.Add(streamProvider.GetCurrentPDU());
                if (this._reader.EndOfStream)
				{
					this.Valid = false;
					this.InvalidReason = "message too short (end of stream)";
					return;
				}
				readBytes += this._reader.Read(body, readBytes, length - readBytes);
			}

			//todo check multiple following messages

			switch(command)
			{
				case "version":
					if(length < 81)
					{
						this.Valid = false;
						this.InvalidReason = "message 'version' too short (less than " + headerLength + 81 + "B)";
						this._reader.NewMessage();
						return;
					}
					this.Type = BTCMsgType.Version;
			        this.SourceAddress = new IPAddressEF(streamProvider.GetCurrentPDU().FrameList.First().SourceEndPoint.Address);
					this.DestinationAddress = new IPAddressEF(streamProvider.GetCurrentPDU().FrameList.First().DestinationEndPoint.Address);

					//var receivingNodeAddressArray = new byte[16]; //bytes.Substring(52, 16);
					//               Array.Copy(bytes,52,receivingNodeAddressArray,0,16);
					//if(receivingNodeAddressArray[10] == 255 && receivingNodeAddressArray[11] == 255
					//   && (receivingNodeAddressArray[0] + receivingNodeAddressArray[1] + receivingNodeAddressArray[2] + receivingNodeAddressArray[3] + receivingNodeAddressArray[4]
					//       + receivingNodeAddressArray[5] + receivingNodeAddressArray[6] + receivingNodeAddressArray[7] + receivingNodeAddressArray[8] + receivingNodeAddressArray[9]) == 0)
					//{
					//	this.ReceivingNodeAddress = "::ffff:";
					//	for(int i = 12; i < 16; ++i)
					//	{
					//		this.ReceivingNodeAddress += receivingNodeAddressArray[i].ToString();
					//		if(i < 15) this.ReceivingNodeAddress += ".";
					//	}
					//}
					//else
					//{
					//	for (int i = 0; i < 16; ++i)
					//	{
					//		this.ReceivingNodeAddress += receivingNodeAddressArray[i].ToString("X2").ToLower();
					//		if ((i % 2) == 1 && i < 15) this.ReceivingNodeAddress += ":";
					//	}
					//}
					//this.ReceivingNodePort = (ushort) (bytes[68]*256 + bytes[69]);

					//var emmittingNodeAddressArray = new byte[16]; //bytes.Substring(70, 16);
					//Array.Copy(bytes, 78, emmittingNodeAddressArray, 0, 16);
					//if(emmittingNodeAddressArray[10] == 255 && emmittingNodeAddressArray[11] == 255
					//   && (emmittingNodeAddressArray[0] + emmittingNodeAddressArray[1] + emmittingNodeAddressArray[2] + emmittingNodeAddressArray[3] + emmittingNodeAddressArray[4]
					//       + emmittingNodeAddressArray[5] + emmittingNodeAddressArray[6] + emmittingNodeAddressArray[7] + emmittingNodeAddressArray[8] + emmittingNodeAddressArray[9]) == 0)
					//{
					//	this.EmmittingNodeAddress = "::ffff:";
					//	for(int i = 12; i < 16; ++i)
					//	{
					//		this.EmmittingNodeAddress += emmittingNodeAddressArray[i].ToString();
					//		if(i < 15) this.EmmittingNodeAddress += ".";
					//	}
					//}
					//else
					//{
					//	for (int i = 0; i < 16; ++i)
					//	{
					//		this.EmmittingNodeAddress += emmittingNodeAddressArray[i].ToString("X2").ToLower();
					//		if ((i % 2 ) == 1 && i < 15) this.EmmittingNodeAddress += ":";
					//	}
					//}
					//this.EmmittingNodePort = (ushort) (bytes[94]*256 + bytes[95]);

					//var userAgentLength = bytes[104];
					var userAgentLength = body[80];
					//if (bytes.Length < (106+userAgentLength))
					if(length < (82 + userAgentLength))
					{
						this.Valid = false;
						this.InvalidReason = "message 'version' too short (less than " + 82 + userAgentLength + "B)";
						this._reader.NewMessage();
						return;
					}
					char[] userAgentArray = new char[userAgentLength];
					Array.Copy(body, 81, userAgentArray, 0, userAgentLength);
					this.UserAgent = new string(userAgentArray);
					break;
				case "verack":
					if(length != 0)
					{
						this.Valid = false;
						this.InvalidReason = "message 'verack' has payload length other than 0B)";
						this._reader.NewMessage();
						return;
					}

					this.Type = BTCMsgType.Verack;
					this.SourceAddress = new IPAddressEF(streamProvider.GetCurrentPDU().FrameList.First().SourceEndPoint.Address);
					this.DestinationAddress = new IPAddressEF(streamProvider.GetCurrentPDU().FrameList.First().DestinationEndPoint.Address);
					break;
				case "tx":
					if(length < 5)
					{
						this.Valid = false;
						this.InvalidReason = "message 'tx' too short (less than " + headerLength + 5 + "B)";
						this._reader.NewMessage();
						return;
					}
					// transaction input
					uint transactionsOffset = 0;
					UInt64 transactionCount = ParseVarInt(4, body, out transactionsOffset);
					for(uint i = 0; i < transactionCount; ++i)
					{
						var transaction = new BTCTransaction();
						transaction.Type = BTCTransactionType.Input;
						if(transaction.Parse(transactionsOffset, body, out transactionsOffset)) this.InputTransactions.Add(transaction);
						else
						{
							this.Valid = false;
							this.InvalidReason = "parsing of transaction input failed";
							return;
						}
					}
					// transaction output
					transactionCount = ParseVarInt(transactionsOffset, body, out transactionsOffset);
					for (uint i = 0; i < transactionCount; ++i)
					{
						var transaction = new BTCTransaction();
						transaction.Type = BTCTransactionType.Output;
						if (transaction.Parse(transactionsOffset, body, out transactionsOffset)) this.OutputTransactions.Add(transaction);
						else
						{
							this.Valid = false;
							this.InvalidReason = "parsing of transaction output failed";
							return;
						}
					}
					this.Type = BTCMsgType.Tx;
					this.SourceAddress = new IPAddressEF(streamProvider.GetCurrentPDU().FrameList.First().SourceEndPoint.Address);
					this.DestinationAddress = new IPAddressEF(streamProvider.GetCurrentPDU().FrameList.First().DestinationEndPoint.Address);
					break;
				default:
					this.Type = BTCMsgType.Other;
					break;
			}
		}

		internal static UInt64 ParseVarInt(uint startIndex, byte[] inputArray, out uint endIndex)
		{
			UInt64 varInt = inputArray[startIndex];
			endIndex = startIndex + 1;
			switch (varInt)
			{
				case 253: //0xfd - uint_16t
					endIndex = startIndex + 1 + 2;
					varInt = (ulong)((inputArray[startIndex+1] << 8) | inputArray[startIndex+2]);
					break;
				case 254: //0xfe - uint_32t
					endIndex = startIndex + 1 + 4;
					varInt = (ulong)((inputArray[startIndex + 1] << 24) |
						(inputArray[startIndex + 2] << 16) |
						(inputArray[startIndex + 3] << 8) |
						inputArray[startIndex + 4]);
					break;
				case 255: //0xff - uint_64t
					endIndex = startIndex + 1 + 8;
					varInt = (ulong)((inputArray[startIndex + 1] << 56) |
						(inputArray[startIndex + 2] << 48) |
						(inputArray[startIndex + 3] << 40) |
						(inputArray[startIndex + 4] << 32) |
						(inputArray[startIndex + 5] << 24) |
						(inputArray[startIndex + 6] << 16) |
						(inputArray[startIndex + 7] << 8) |
						inputArray[startIndex + 8]);
					break;
			}
			return varInt;
		}
    }
}
