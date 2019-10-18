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
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.SnooperSMTP
{
    public class SMTPMsg
    {
        public enum SMTPMsgType
        {
            OTHER,
            //USER, // USER login
            //PASS, // PASSword
            MAIL    // contains e-mail message data
        }

        private readonly PDUStreamReader _reader;
        public IEnumerable<PmFrameBase> Frames;
        public string InvalidReason;
        public string MessageContent;
        public DateTime Timestamp;
        public SMTPMsgType Type;
        public bool Valid;
        public List<IExportSource> ExportSources;

        public SMTPMsg(PDUStreamReader reader)
        {
            // fill default values and store things we'll need later
            this._reader = reader;
            this.Valid = true;
            this.InvalidReason = string.Empty;
            this.MessageContent = string.Empty;
            this.Type = SMTPMsgType.OTHER;
            //this.DataContent = null;
            this.ExportSources = new List<IExportSource>();

            // do the parsing itself
            this.Parse();
        }

        private void Parse()
        {
            // transform reader to stream provider to get timestamp and frame numbers values
            var _streamProvider = this._reader.PDUStreamBasedProvider;
            this.Frames = _streamProvider.ProcessedFrames;
            if(_streamProvider.GetCurrentPDU() != null) { this.Timestamp = _streamProvider.GetCurrentPDU().FirstSeen; }
            else
            {
                this.InvalidReason = "could not retrieve PDU";
                this.ExportSources.Add(_streamProvider.Conversation);
                this.Valid = false;
                return;
            }
            
            this.ExportSources.Add(_streamProvider.GetCurrentPDU());

            var _line = this._reader.ReadLine();
            var _splittedLine = _line.Split(' ');
            if(_splittedLine[0].IndexOf("DATA", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // this means data is coming
                if(!this._reader.NewMessage())
                {
                    this.Valid = false;
                    this.InvalidReason = "expected data after DATA command";
                    return;
                } // no more data, too bad...

                _line = this._reader.ReadLine();
                _splittedLine = _line.Split(' ');
                if(_splittedLine[0].IndexOf("354", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    // 354 code means 'Enter message, ending with "." on a line by itself' and it's followed by the message itself
                    if (!this._reader.NewMessage())
                    {
                        this.Valid = false;
                        this.InvalidReason = "expected data after DATA command and 354 code";
                        return;
                    } // no more data, too bad...

                    this.MessageContent = this._reader.ReadToEnd();
                    this.Type = SMTPMsgType.MAIL;
                }
                else
                {
                    this.Valid = false;
                    this.InvalidReason = "expected code 354 after DATA command";
                    return;
                }

            }
        }
    }
}
