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

using System.ComponentModel.DataAnnotations.Schema;

namespace Netfox.Snoopers.SnooperSIP.Models.Message
{
    [ComplexType]
    public class SIPRequestLine
    {
        public string Method { get; private set; }
        public string RequestURI { get; private set; }
        public string SIPVersion { get; private set; }
        public bool IsPresent => this.SIPVersion != null;
        /*public SIPRequestLine()
        {
        }*/
        internal  SIPRequestLine() { } //EF
        public SIPRequestLine(string[] requestLine, out bool valid, out string invalidReason)
        {
            valid = true;
            invalidReason = string.Empty;
            if(requestLine.Length != 3)
            {
                var line = string.Join(" ", requestLine);
                //throw new InvalidDataException("SIPRequestLine has unexpected length (other than 3 space-separated words): \""+line+"\"");
                valid = false;
                invalidReason = "SIPRequestLine has unexpected length (other than 3 space-separated words): \"" + line + "\"";
                return;
            }
            this.Method = requestLine[0];
            this.RequestURI = requestLine[1];
            this.SIPVersion = requestLine[2];
        }

        public override string ToString() { return this.Method + ' ' + this.RequestURI + ' ' + this.SIPVersion; }
    }
}