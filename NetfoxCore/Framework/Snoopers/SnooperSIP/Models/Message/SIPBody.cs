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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;

namespace Netfox.Snoopers.SnooperSIP.Models.Message
{
    [ComplexType]
    public class SIPBody
    {
        public string RTPAddress { get; private set; } = String.Empty;
        public string RTPPort { get; private set; } = String.Empty;
        public PersistableJsonSerializableString PossibleCodecs { get; private set; } = new PersistableJsonSerializableString();
        public bool IsPresent => this.RTPAddress != String.Empty;
        internal SIPBody() { } //EF
        public SIPBody(PDUStreamReader reader, SIPMsg msg)
        {
            this.Parse(reader, msg);
        }

        private void Parse(PDUStreamReader reader, SIPMsg msg)
        {
            var tag = string.Empty;
            var value = string.Empty;

            // message body
            if(reader.Peek() > 0)
            {
                var line = reader.ReadLine();
                //Console.WriteLine("++ BODY ++");
                while(line != null)
                {
                    this.ParseSIPBodyLine(line, ref tag, ref value);
                    //Console.WriteLine("\"" + _tag + "\" : \"" + _value + "\"");
                    //Console.WriteLine(_line.Length.ToString() + ": \"" + _line.ToString() + "\"");
                    string[] values;
                    switch(tag)
                    {
                        case "c":
                            values = value.Split(' ');
                            if(values.Length < 2)
                            {
                                msg.Valid = false;
                                msg.InvalidReason = "wrong format in message body: '" + line + "'";
                                return;
                            }
                            if(values[1] == "IP4") { this.RTPAddress = values[2]; }
                            break;
                        case "m":
                            values = value.Split(' ');
                            if(values.Length < 2)
                            {
                                msg.Valid = false;
                                msg.InvalidReason = "wrong format in message body: '" + line + "'";
                                return;
                            }
                            this.RTPPort = values[1];
                            break;
                        case "a":
                            values = value.Split(' ');
                            if(values.Count() == 2 && values[0].Contains("rtpmap:")) // codecs' descriptions
                            {
                                this.PossibleCodecs.Add(values[1]+" ("+values[0].Split(':')[1]+")");
                            }
                            break;
                        default:
                            break;
                    }
                    line = reader.ReadLine();
                }
                //Console.WriteLine("---------");
            }
        }

        private void ParseSIPBodyLine(string line, ref string tag, ref string value)
        {
            var index = 0;

            index = line.IndexOf('=');
            if(index < 0)
            {
                // something's wrong
                tag = string.Empty;
                value = string.Empty;
            }
            else
            {
                tag = line.Substring(0, index).Trim();
                value = line.Substring(index + 1).Trim();
            }
        }

        public override string ToString()
        {
            var converted = string.Empty;
            if(this.RTPAddress != string.Empty && this.RTPPort != string.Empty) { converted += "\nRTP: " + this.RTPAddress + ":" + this.RTPPort; }
            return converted;
        }
    }
}