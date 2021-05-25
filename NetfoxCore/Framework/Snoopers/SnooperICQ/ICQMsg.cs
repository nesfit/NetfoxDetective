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
using Netfox.Snoopers.SnooperICQ.Enums;

namespace Netfox.Snoopers.SnooperICQ
{
    internal class ICQMsg
    {
        private const byte FLAPCheck = 0x2a;
        private readonly PDUStreamReader _reader;

        public ICQMsg(PDUStreamReader reader)
        {
            this.Valid = true;
            this._reader = reader;
            this.Timestamp = this._reader.PDUStreamBasedProvider.GetCurrentPDU().FirstSeen;
            var list = new List<IExportSource>
            {
                this._reader.PDUStreamBasedProvider.GetCurrentPDU()
            };
            this.ExportSources = list;
            this.ParseFLAP();
        }

        public ICQMsgType Type { get; set; }
        public string Sender { get; private set; }
        public string Receiver { get; private set; }
        public string Body { get; private set; }
        public bool Valid { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string InvalidReason { get; private set; }
        public IEnumerable<IExportSource> ExportSources { get; private set; }

        public void ParseFLAP()
        {
            try
            {
                var controlByte = this._reader.ReadByte();
                if(controlByte != FLAPCheck)
                {
                    this.Valid = false;
                    this.InvalidReason = "Invalid start of message";
                    return;
                }

                var channelNum = this._reader.ReadByte();
                ICQSNACChannelStat channel;
                if(!Enum.TryParse(channelNum.ToString(), out channel)) { channel = ICQSNACChannelStat.Unknown; }
                var seqNum = this._reader.ReadUInt16();
                var dataSize = this._reader.ReadUInt16();

                if(channel == ICQSNACChannelStat.SNAC) { this.ParseSNAC(); }
            }
            catch(Exception)
            {
                this.Valid = false;
                this.InvalidReason = "Error in message parsing";
            }
        }

        public void ParseSNAC()
        {
            var familyIDNumber = this._reader.ReadUInt16();
            ICQSNAClistTable familyID;
            if(!Enum.TryParse(familyIDNumber.ToString(), out familyID)) { familyID = ICQSNAClistTable.Unknown; }
            var familySubIDNumber = this._reader.ReadUInt16();
            var flags = this._reader.ReadUInt16();
            var requestID = this._reader.ReadUInt16();

            if(familyID == ICQSNAClistTable.ICBMService)
            {
                //if(flags == 0x0000)
                //{
                //    this.Valid = false;
                //    this.InvalidReason = "Unexpected SNAC flags";
                //    return;
                //}

                if(familySubIDNumber == 0x0006 || familySubIDNumber == 0x0007)
                {
                    // rec message
                    var iDontGiveAShit = this._reader.ReadUInt16();
                    var msgIDCookie = this._reader.ReadUInt64();
                    var channel = this._reader.ReadUInt16();
                    var uinStringLen = this._reader.ReadByte();
                    var uinString = new String(this._reader.ReadChars(uinStringLen));

                    if(familySubIDNumber == 0x0007) // receive MSG
                    {
                        var senderWarningLvl = this._reader.ReadUInt16();
                        var numOfTLV = this._reader.ReadUInt16();
                        this.Sender = uinString;
                    }
                    else // send MSG
                    { this.Receiver = uinString; }

                    while(!this._reader.EndOfPDU)
                    {
                        var type = this._reader.ReadUInt16();
                        var len = this._reader.ReadUInt16();
                        var value = this._reader.ReadChars(len);

                        if(type == 0x02) // message
                        {
                            var readBytes = 0;

                            var fragmentID1 = (byte) value[readBytes++];
                            var fragmentVer1 = (byte) value[readBytes++];
                            var lenOfRest1 = (UInt16) value[readBytes++] << 8|value[readBytes++];
                            // pass capabilities
                            readBytes += lenOfRest1;
                            var fragmentID2 = (byte)value[readBytes++];
                            var fragmentVer2 = (byte)value[readBytes++];
                            var lenOfRest2 = (UInt16)value[readBytes++] << 8 | value[readBytes++];
                            var msgCharset = (UInt16)value[readBytes++] << 8 | value[readBytes++];
                            var msgLang = (UInt16)value[readBytes++] << 8 | value[readBytes++];

                            this.Body = new String(value, readBytes, value.Length - readBytes);

                            this.Type = ICQMsgType.Msg;

                        }
                    }
                }
            }
        }
    }
}