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
using System.Diagnostics;
using System.Linq;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Snoopers.SnooperIMAP
{
    public class IMAPMsg
    {
        public enum IMAPMsgType
        {
            OTHER,
            //USER, // USER login
            //PASS, // PASSword
            FETCH_BODY // contains e-mail message data
        }


        private readonly PDUStreamReader _reader;
        public IEnumerable<PmFrameBase> Frames;
        public string InvalidReason;
        public string MessageContent;
        public DateTime Timestamp;
        public IMAPMsgType Type;
        public bool Valid;
        public List<IExportSource> ExportSources;

        public IMAPMsg(PDUStreamReader reader)
        {
            // fill default values and store things we'll need later
            this._reader = reader;
            this.Valid = true;
            this.InvalidReason = string.Empty;
            this.MessageContent = string.Empty;
            this.Type = IMAPMsgType.OTHER;
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

            //Console.WriteLine("FTPMsg created, frame numbers: " + string.Join(",", Frames.ToArray()));
            //if(!_streamProvider.GetCurrentPDU().Conversation.ApplicationTags.Any())
            //{
            //    this.Valid = false;
            //    this.InvalidReason = "no application tag";
            //    this.ExportSources.Add(_streamProvider.GetCurrentPDU());
            //    return;
            //}

            this.ExportSources.Add(_streamProvider.GetCurrentPDU());

            // * 7 FETCH (UID 22 RFC822.SIZE 1192 BODY[] {1192}
            // * 5 FETCH (UID 19 RFC822.SIZE 3026350 BODY[]<0> {65536}
            // 35 UID fetch 19 (UID RFC822.SIZE BODY[]<65536.65536>)
            // * 5 FETCH (UID 19 RFC822.SIZE 3026350 BODY[] <65536> {65536}
            // 36 UID fetch 19(UID RFC822.SIZE BODY[] <131072.65536>)
            // * 5 FETCH (UID 19 RFC822.SIZE 3026350 BODY[]<131072> {65536}


            var _line = this._reader.ReadLine();
            var _splittedLine = _line.Split(' ');
            if(_splittedLine.Count() < 3)
            {
                return;
            }
            var chunkSize = 0;
            var chunk = string.Empty;
            // this is response with whole e-mail
            if (_splittedLine[0] == "*" &&
               _splittedLine[2].IndexOf("FETCH", StringComparison.OrdinalIgnoreCase) == 0 &&
               _line.IndexOf("BODY[]", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
            {
                var messageSize = 0;
                for(var i = 3; i < _splittedLine.Count(); ++i)
                {
                    if(_splittedLine[i].IndexOf(".SIZE", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        // this token contains ".SIZE", next token is message size
                        if(i < (_splittedLine.Count() - 1)) { messageSize = Int32.Parse(_splittedLine[i + 1]); }
                        Debug.WriteLine("message size: " + messageSize);
                        ++i;
                    }
                    // size of the chunk
                    if(_splittedLine[i].StartsWith("{"))
                    {
                        chunkSize = Int32.Parse(_splittedLine[i].Trim('{', '}'));
                        Debug.WriteLine("chunkSize " + chunkSize);
                    }
                }
                // read all the content
                chunk = _reader.ReadToEnd();
                // chunk is complete
                if(chunk.Length >= chunkSize)
                {
                    // trim the chunk by its announced size
                    this.MessageContent += chunk.Substring(0, chunkSize);
                    // clear the hunk
                    chunkSize = 0;
                    chunk = string.Empty;
                }
                Debug.WriteLine("content size: " + this.MessageContent.Length);

                // message is incomplete
                while (this.MessageContent.Length < messageSize)
                {
                    // we still need some data but there are none available -> incomplete message
                    if(!this._reader.NewMessage()) { return; }

                    _line = this._reader.ReadLine();
                    _splittedLine = _line.Split(' ');

                    // line contains to few spaces - it's either continuation of chunk or some stuff we don't want
                    if(_splittedLine.Count() < 3)
                    {
                        // there is no space, it's chunk
                        if(_line.Equals(_splittedLine[0]))
                        {
                            // continuation of data
                            chunk += _line + "\r\n" + this._reader.ReadToEnd();
                            // chunk is complete
                            if(chunk.Length >= chunkSize)
                            {
                                // trim the chunk by its announced size
                                this.MessageContent += chunk.Substring(0, chunkSize);
                                // clear the chunk
                                chunkSize = 0;
                                chunk = string.Empty;
                            }
                            // chunk is incomplete
                            else
                            {
                                Debug.WriteLine("new chunk.Length (cont.): " + chunk.Length);
                            }
                            Debug.WriteLine("new content size (cont.): " + this.MessageContent.Length);
                            // this PDU is read, go to next one
                            continue;
                        }
                        // it's not chunk but something else - skip it
                        else
                        { continue; }
                    }
                    // line cotains enough spaces
                    if(_splittedLine[0] == "*" &&
                       _splittedLine[2].IndexOf("FETCH", StringComparison.OrdinalIgnoreCase) == 0 &&
                       _line.IndexOf("BODY[]", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        var skipThis = false;
                        for(var i = 3; i < _splittedLine.Count(); ++i)
                        {
                            if(_splittedLine[i].IndexOf(".SIZE", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                // this token contains ".SIZE", next token is message size
                                if(i < (_splittedLine.Count() - 1))
                                {
                                    // announced messages size differs from previously announced one - this shouldn't be
                                    if(messageSize != Int32.Parse(_splittedLine[i + 1]))
                                    {
                                        // break inner for cycle
                                        i = _splittedLine.Count();
                                        // flag this chunk for skipping
                                        skipThis = true;
                                        continue;
                                    } //else { Debug.Print("matching size"); }
                                }
                                ++i;
                            }
                            // size of the chunk
                            if (_splittedLine[i].StartsWith("{"))
                            {
                                chunkSize = Int32.Parse(_splittedLine[i].Trim('{','}'));
                                Debug.WriteLine("chunkSize "+chunkSize);
                            }
                        }
                        // not skipping
                        if(!skipThis)
                        {
                            chunk += _reader.ReadToEnd();
                            if(chunk.Length >= chunkSize)
                            {
                                // trim chunk by its announced size
                                this.MessageContent += chunk.Substring(0, chunkSize);
                                // clear the chunk
                                chunkSize = 0;
                                chunk = string.Empty;
                            }
                            else { Debug.WriteLine("new chunk.Length: " + chunk.Length); }
                            Debug.WriteLine("new content size: " + this.MessageContent.Length);
                        }
                    }
                }
                // message is complete
                this.Type = IMAPMsgType.FETCH_BODY;
            }
        }
    }
}
