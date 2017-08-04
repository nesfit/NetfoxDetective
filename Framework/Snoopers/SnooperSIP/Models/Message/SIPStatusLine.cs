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
using System.Diagnostics;

namespace Netfox.SnooperSIP.Models.Message
{
    [ComplexType]
    public class SIPStatusLine
    {
        public string SIPVersion { get; private set; }
        public string StatusCode { get; private set; }
        public string StatusInfo { get; private set; }
        public bool IsPresent => this.SIPVersion != null;
        internal SIPStatusLine() { } //EF
        public SIPStatusLine(string[] startline, out bool valid, out string invalidReason)
        {
            valid = true;
            invalidReason = string.Empty;
            if(startline.Length < 3)
            {
                var line = string.Join(" ", startline);
                valid = false;
                invalidReason = "SIPStatusLine has unexpected length (less than 3 space-separated words): \"" + line + "\"";
                return;
            }
            this.SIPVersion = startline[0];
            this.StatusCode = startline[1];
            this.StatusInfo = string.Join(" ", startline, 2, startline.Length - 2);
            Debug.Print("StatusInfo: "+this.StatusInfo);
        }

        public override string ToString() { return this.StatusCode + ' ' + this.StatusInfo + ' ' + this.SIPVersion; }
    }
}