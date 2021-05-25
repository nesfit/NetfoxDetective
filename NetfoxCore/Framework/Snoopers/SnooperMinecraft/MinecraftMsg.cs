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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Snoopers.SnooperMinecraft.Models;
using Newtonsoft.Json;

namespace Netfox.Snoopers.SnooperMinecraft
{

    #region MinecraftMsg
    public class MinecraftMsg
    {
        private const string ChatMessageSpecificier = "{\"extra\":[{";
        private const string PrivateMessageSpecifier = "purple";
        private const int IndexOfJsonStart = 5;
        private readonly BinaryReader _reader;
        public IEnumerable<PmFrameBase> Frames;
        public string MessageContent;
        public string Text;
        public string Sender;
        public string Receiver;
        public DateTime Timestamp;
        public MinecraftMessageType MessageType = MinecraftMessageType.Broadcast;
        public bool Valid = true;

        public MinecraftMsg(BinaryReader reader)
        {
            // fill default values and store things we'll need later
            this._reader = reader;
            this.MessageContent = string.Empty;
            this.Sender = this.Receiver = this.Text = string.Empty;

            // do the parsing itself
            this.Parse();
        }

        private void Parse()
        {
            // transform reader to stream provider to get timestamp and frame numbers values
            var streamProvider = this._reader.BaseStream as PDUStreamBasedProvider;
            this.Timestamp = streamProvider.GetCurrentPDU().FirstSeen;
            this.Frames = streamProvider.ProcessedFrames;

            // parse data in frame into memory stream
            var data = new byte[16*1024];
            var ms = new MemoryStream();
            int read;
            while ((read = streamProvider.Read(data,0,data.Length)) > 0)
                ms.Write(data,0,read);

            // convert memory stream to string
            data = ms.ToArray();
            var str = Encoding.Default.GetString(data);
            
            // check whether there is enough data for chat and find chat specific format in them
            if (str.Length <= IndexOfJsonStart + ChatMessageSpecificier.Length || !str.Contains(ChatMessageSpecificier))
            {
                this.Valid = false;
                return;
            }

            // find start and end of msg
            var Start = str.IndexOf(ChatMessageSpecificier, StringComparison.Ordinal);
            var end = str.LastIndexOf("}", StringComparison.Ordinal);
            var content = str.Substring(Start, end - Start+1);

            // when more than one specifier than packet might be corrupted
            var count = new Regex(Regex.Escape(ChatMessageSpecificier)).Matches(content).Count;
            if (count > 1)
            {
                this.Valid = false;
                return;
            }

            // deserialize json
            var msg = JsonConvert.DeserializeObject<MinecraftChatMessage>(content);
                
            foreach(var txt in msg.Extra)
            {
                // determine chat message type based on color, private messages have their colors always same
                if(txt.Color.Contains(PrivateMessageSpecifier) && txt.Text.Contains("To ")) // begins
                    this.MessageType = MinecraftMessageType.PrivateMessage;

                this.MessageContent += txt.Text; // append strings of text into output
            }

            this.MessageContent += msg.Text; // append last possible output

            // parse senders, text itself and possible receiver
            var pos = 0;
            if((pos = this.MessageContent.IndexOf(":", StringComparison.Ordinal)) <= 0)
            {
                // text doesnt have sender(its sent from server)
                this.Sender = "Server";
            }

            if(this.MessageType == MinecraftMessageType.PrivateMessage) this.Receiver = this.MessageContent.Substring(3, pos);
            else if(this.Sender != "Server") // MinecraftMessageType.Broadcast
                this.Sender = this.MessageContent.Substring(0, pos);

            this.Text = this.MessageContent.Substring(pos + 2);
        }
    }

    #endregion MinecraftMsg
}