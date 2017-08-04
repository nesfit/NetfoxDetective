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
using System.Linq;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.SnooperFTP
{

    #region FTPMsg
    public class FTPMsg
    {
        public enum FTPMsgType
        {
            OTHER,
            PORT, // type of PORT message, contains endpoint's address in MessageContent
            PWD,  // PWD message, contains remote path in MessageContent
            LIST, // LIST command, listing of directory will follow in some data chunk
            STOR, // STOR command, uploaded file should follow in a data chunk
            RETR, // RETR command, downloaded file should follow in a data chunk
            DELE, // DELE command, delete remote file
            USER, // USER login
            PASS, // PASSword
            DATA // data chunk, contains data in DataContent
        }

        private readonly PDUStreamReader _reader;
        public byte[] DataContent;
        public IEnumerable<PmFrameBase> Frames;
        public string InvalidReason;
        public string MessageContent;
        public DateTime Timestamp;
        public FTPMsgType Type;
        public bool Valid;
        public List<IExportSource> ExportSources;

        public FTPMsg(PDUStreamReader reader)
        {
            // fill default values and store things we'll need later
            this._reader = reader;
            this.Valid = true;
            this.InvalidReason = string.Empty;
            this.Type = FTPMsgType.OTHER;
            this.MessageContent = string.Empty;
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

            switch (_streamProvider.GetCurrentPDU().L7Conversation.ApplicationTags[0])
            {
                case "ftp":
                    var _line = this._reader.ReadLine();
                    var _splittedLine = _line.Split(' ');
                    if(_splittedLine[0].IndexOf("PORT", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // PORT message
                        this.Type = FTPMsgType.PORT;
                        var _value = _splittedLine[1].Split(',');
                        var _address = _value[0] + '.' + _value[1] + '.' + _value[2] + '.' + _value[3];
                        var _port1 = 0;
                        var _port2 = 0;
                        int.TryParse(_value[4], out _port1);
                        int.TryParse(_value[5], out _port2);
                        var _port = _port1*256 + _port2;
                        int.TryParse(_value[4], out _port1);
                        //EndPoint _endpoint = new IPEndPoint(_address);
                        //Console.WriteLine("  PORT: " + _address + ":" + _port);
                        this.MessageContent = _address + ":" + _port;
                    }
                    else if(_splittedLine[0].IndexOf("257", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var _tmp = _line.Split('"');
                        this.MessageContent = _tmp[1];
                        //Console.WriteLine("  PWD: working directory changed: \"" + MessageContent + '"');
                        this.Type = FTPMsgType.PWD;
                    }
                    else if(_splittedLine[0].IndexOf("LIST", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Type = FTPMsgType.LIST;
                        //Console.WriteLine("  LIST");
                    }
                    else if(_splittedLine[0].IndexOf("STOR", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Type = FTPMsgType.STOR;
                        this.MessageContent = _splittedLine[1];
                        //Console.WriteLine("  STOR: uploaded file: " + MessageContent);
                    }
                    else if(_splittedLine[0].IndexOf("RETR", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Type = FTPMsgType.RETR;
                        this.MessageContent = _splittedLine[1];
                        //Console.WriteLine("  RETR: downloaded file: " + MessageContent);
                    }
                    else if(_splittedLine[0].IndexOf("DELE", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Type = FTPMsgType.RETR;
                        this.MessageContent = _splittedLine[1];
                        //Console.WriteLine("  DELE: deleted file: " + MessageContent);
                    }
                    else if(_splittedLine[0].IndexOf("USER", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Type = FTPMsgType.USER;
                        this.MessageContent = _splittedLine[1];
                        //Console.WriteLine("  USER: " + MessageContent);
                    }
                    else if(_splittedLine[0].IndexOf("PASS", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Type = FTPMsgType.PASS;
                        this.MessageContent = _splittedLine[1];
                        //Console.WriteLine("  PASS: " + MessageContent);
                    }
                    else
                    {
                        this.Type = FTPMsgType.OTHER;
                    }
                    break;
                case "ftp-data":
                    //Console.WriteLine("  FTP DATA encountered");
                    this.Type = FTPMsgType.DATA;
                    // assign 
                    this.DataContent = _streamProvider.GetCurrentPDU().PDUByteArr;
                    break;
                default:
                    this.Valid = false;
                    this.InvalidReason = "unknown app tag encountered";
                    break;
            }
        }
    }
    #endregion FTPMsg
}
