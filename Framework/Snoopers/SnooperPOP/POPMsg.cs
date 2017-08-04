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

using System;
using System.Collections.Generic;
using System.Linq;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.SnooperPOP
{
    public class POPMsg
    {
        public enum POPMsgType
        {
            OTHER,
            //LIST, // LIST command, listing of messages
            USER, // USER login
            PASS, // PASSword
            RETR  // RETReive, get message
        }

        private readonly PDUStreamReader _reader;
        public byte[] DataContent;
        public IEnumerable<PmFrameBase> Frames;
        public string InvalidReason;
        public string MessageContent;
        public DateTime Timestamp;
        public POPMsgType Type;
        public bool Valid;
        public List<IExportSource> ExportSources;

        public POPMsg(PDUStreamReader reader)
        {
            // fill default values and store things we'll need later
            this._reader = reader;
            this.Valid = true;
            this.InvalidReason = string.Empty;
            this.MessageContent = string.Empty;
            this.Type = POPMsgType.OTHER;
            this.DataContent = null;
            this.ExportSources = new List<IExportSource>();

            // do the parsing itself
            this.Parse();
        }

        private void Parse()
        {
            // transform reader to stream provider to get timestamp and frame numbers values
            var _streamProvider = this._reader.PDUStreamBasedProvider;
            this.Frames = _streamProvider.ProcessedFrames;
            if(_streamProvider.GetCurrentPDU() != null)
            {
                this.Timestamp = _streamProvider.GetCurrentPDU().FirstSeen;
            }
            else
            {
                this.InvalidReason = "could not retrieve PDU";
                this.ExportSources.Add(_streamProvider.Conversation);
                this.Valid = false;
                return;
            }

            //Console.WriteLine("FTPMsg created, frame numbers: " + string.Join(",", Frames.ToArray()));
            if(!_streamProvider.GetCurrentPDU().L7Conversation.ApplicationTags.Any())
            {
                this.Valid = false;
                this.InvalidReason = "no application tag";
                this.ExportSources.Add(_streamProvider.GetCurrentPDU());
                return;
            }

            this.ExportSources.Add(_streamProvider.GetCurrentPDU());

            var _line = this._reader.ReadLine();
            var _splittedLine = _line.Split(' ');
            //Console.Write("  "+_line.Trim()+"\n");
            if(_splittedLine[0].IndexOf("USER", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.Type = POPMsgType.USER;
                this.MessageContent = _splittedLine[1];
                //Console.WriteLine("  USER: " + MessageContent);
            }
            else if (_splittedLine[0].IndexOf("PASS", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.Type = POPMsgType.PASS;
                this.MessageContent = _splittedLine[1];
                //Console.WriteLine("  PASS: " + MessageContent);
            }
            else if (_splittedLine[0].IndexOf("RETR", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if(!this._reader.NewMessage()) return; // actual messsage will be in the next PDU
                _line = this._reader.ReadLine(); // get the first line
                _splittedLine = _line.Split(' ');
                if(_splittedLine[0].IndexOf("+OK", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.Type = POPMsgType.RETR;
                    this.MessageContent = this._reader.ReadToEnd();
                    //Console.WriteLine("  RETR: " + MessageContent);
                }
            }
        }
    }
}
