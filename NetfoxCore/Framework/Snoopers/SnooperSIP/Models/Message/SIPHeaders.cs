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
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;

namespace Netfox.Snoopers.SnooperSIP.Models.Message
{
    [ComplexType]
    public class SIPHeaders
    {
        public string CallID { get; private set; } = string.Empty;
        public string Contact { get; private set; } = string.Empty;
        public string From { get; private set; } = string.Empty;

        public string To { get; private set; } = string.Empty;
        public bool IsPresent => this.CallID != String.Empty;
        internal SIPHeaders() { } //EF
        public SIPHeaders(PDUStreamReader reader, SIPMsg msg)
        {
            this.Parse(reader,msg);
        }

        private void Parse(PDUStreamReader reader,SIPMsg msg)
        {
            var tag = string.Empty;
            var value = string.Empty;
            var parameters = new Dictionary<string, string>();
            var line = reader.ReadLine();

            if (line == null)
            {
                msg.Valid = false;
                msg.InvalidReason = "End of stream reached sooner than expected";
                return;
            }
            //Console.WriteLine("++ HEADER ++");
            while (line != null && line != "\n" && line != string.Empty)
                //while (_line != null && _line != "\n" && _line != string.Empty)
            {
                parameters.Clear();
                this.ParseSIPHeaderLine(line, ref tag, ref value, ref parameters);
                switch(tag)
                {
                    case "To":
                    case "t":
                        this.To += value;
                        break;
                    case "From":
                    case "f":
                        this.From += value;
                        break;
                    case "Contact":
                        this.Contact += value;
                        break;
                    case "Call-ID":
                    case "CallID":
                    case "i":
                        this.CallID += value;
                        //Console.WriteLine(_line);
                        break;
                    default:
                        break;
                }
                line = reader.ReadLine();
            }
        }

        private void ParseSIPHeaderLine(string line, ref string tag, ref string value, ref Dictionary<string, string> parameters)
        {
            var index = 0;

            index = line.IndexOf(':');
            if(index < 0)
            {
                // there's no tag
                value = line.Trim();
            }
            else
            {
                tag = line.Substring(0, index).Trim();
                value = line.Substring(index + 1).Trim();
            }

            var _parameters = value.Split(';');
            value = _parameters[0];
            //Console.WriteLine(tag + ": " + value);
            for(index = 1; index < _parameters.Length; ++index)
            {
                var parameter = _parameters[index].Split('=');
                var parTag = parameter[0].Trim();
                var parValue = string.Empty;
                if(parameter.Length == 2) { parValue = parameter[1].Trim(); }
                //Console.WriteLine("  " + _par_tag + ": " + _par_value);
                parameters[parTag] = parValue;
            }
        }

        public override string ToString()
        {
            var converted = string.Empty;
            if(this.From != string.Empty) { converted += "\nFrom: " + this.From; }
            if(this.To != string.Empty) { converted += "\nTo: " + this.To; }
            if(this.Contact != string.Empty) { converted += "\nContact: " + this.CallID; }
            if(this.CallID != string.Empty) { converted += "\nCall-ID: " + this.CallID; }
            return converted;
        }
    }
}