// Copyright (c) 2017 Jan Pluskal
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
using Netfox.SnooperYMSG.Enums;

namespace Netfox.SnooperYMSG
{
    internal class YMSGMsg
    {
        private readonly byte[] _delimiter;
        private readonly PDUStreamReader _reader;

        public YMSGMsg(PDUStreamReader reader)
        {
            this._delimiter = new byte[2];
            this._delimiter[0] = 0xc0;
            this._delimiter[1] = 0x80;
            this.Valid = true;
            this._reader = reader;
            this.ParseMsg();
        }

        public DateTime Timestamp;
        public List<IExportSource> ExportSources { get; } = new List<IExportSource>();
        public List<Tuple<YMSGTVTypes, string>> TVs { get; } = new List<Tuple<YMSGTVTypes, string>>();

        public UInt16 Version { get; private set; }
        public UInt16 VendorID { get; private set; }
        public UInt16 Length { get; private set; }
        public YMSGTypes Type { get; private set; }
        public YMSGHeadStatus Status { get; private set; }
        public UInt32 Session { get; private set; }

        public string InvalidReason { get; private set; }
        public bool Valid { get; private set; }

        public void ParseMsg()
        {
            this.ExportSources.Add(this._reader.PDUStreamBasedProvider.GetCurrentPDU());
            this.Timestamp = this._reader.PDUStreamBasedProvider.GetCurrentPDU().FirstSeen;

            this.ParseHeader();
            if(!this.Valid) { return; }

            this.ParseTVs();


            this.Valid = true;
        }

        private void ParseHeader()
        {
            var controlString = new String(this._reader.ReadChars(4));
            if(!controlString.Equals("YMSG"))
            {
                this.Valid = false;
                this.InvalidReason = "Invalid start of message";
                return;
            }

            try
            {
                this.Version = this._reader.ReadUInt16();
                this.VendorID = this._reader.ReadUInt16();
                this.Length = this._reader.ReadUInt16();
                var typeNum = this._reader.ReadUInt16();
                if(Enum.IsDefined(typeof(YMSGTypes), (int) typeNum)) { this.Type = (YMSGTypes) typeNum; }
                else
                { this.Type = YMSGTypes.Unknown; }
                var statusNum = this._reader.ReadUInt32();
                if(Enum.IsDefined(typeof(YMSGHeadStatus), (int) statusNum)) { this.Status = (YMSGHeadStatus) statusNum; }
                else
                { this.Status = YMSGHeadStatus.Unknown; }
                this.Session = this._reader.ReadUInt32();
            }
            catch(Exception)
            {
                this.Valid = false;
                this.InvalidReason = "Error in message head parsing";
            }
        }

        private void ParseTVs()
        {
            try
            {
                while(!this._reader.EndOfPDU)
                {
                    var keyNum = this._reader.ReadToDelimiter(this._delimiter);
                    YMSGTVTypes key;
                    if(!Enum.TryParse(keyNum, out key)) { key = YMSGTVTypes.Unknown; }

                    var val = this._reader.ReadToDelimiter(this._delimiter);

                    this.TVs.Add(new Tuple<YMSGTVTypes, string>(key, val));
                }
            }
            catch(Exception)
            {
                this.Valid = false;
                this.InvalidReason = "Error while parsing message TV values";
            }
        }
    }
}