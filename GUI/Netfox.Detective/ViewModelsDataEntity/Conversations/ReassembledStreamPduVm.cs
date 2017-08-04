// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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

using System.Text;
using Castle.Windsor;
using Netfox.Core.Enums;
using Netfox.Framework.Models;

namespace Netfox.Detective.ViewModelsDataEntity.Conversations
{
    public class ReassembledStreamPduVm : DetectiveDataEntityViewModelBase
    {
        private readonly Encoding _encoding;
        private readonly L7PDU l7Pdu;

        public ReassembledStreamPduVm(IWindsorContainer applicationWindsorContainer, L7PDU l7Pdu, Encoding encoding) : base(applicationWindsorContainer)
        {
            this.l7Pdu = l7Pdu;
            this._encoding = encoding;
            this.EncapsulatedModel = l7Pdu;
        }

        public byte[] Bytes => this.l7Pdu.PDUByteArr;

        public DaRFlowDirection FlowDirection => this.l7Pdu.FlowDirection;

        public string PlainTextValue
        {
            get
            {
                var asciiString = this._encoding.GetString(this.l7Pdu.PDUByteArr);

                if(asciiString.EndsWith("\r\n")) { asciiString = asciiString.Substring(0, asciiString.Length - 2); }
                else if(asciiString.EndsWith("\n")) { asciiString = asciiString.Substring(0, asciiString.Length - 1); }

                return asciiString;
            }
        }

        public string HexValue
        {
            get
            {
                var hexString = new StringBuilder();

                var bytes = this.l7Pdu.PDUByteArr;

                var octetC = 0;
                var offset = 0;


                var ascciChars = new char[16];


                foreach(var b in bytes)
                {
                    if(octetC == 0)
                    {
                        hexString.Append(offset.ToString("X8"));
                        hexString.Append(' ');
                    }

                    hexString.Append(b.ToString("X2"));
                    hexString.Append(' ');

                    if(b > 0x20 && b < 0x7E) { ascciChars[octetC] = (char) b; }
                    else
                    { ascciChars[octetC] = '.'; }

                    offset++;
                    octetC++;
                    if(octetC == 8) { hexString.Append(' '); }
                    if(octetC == 16)
                    {
                        octetC = 0;
                        hexString.Append(' ');
                        hexString.Append(ascciChars);
                        hexString.Append("\n");
                    }
                }

                if(octetC < 16)
                {
                    for(var i = octetC; i < 16; i++) { hexString.Append("   "); }

                    if(octetC < 8) { hexString.Append(' '); }

                    hexString.Append(' ');
                    hexString.Append(ascciChars, 0, octetC);
                    hexString.Append("\n");
                }

                return hexString.ToString();
            }
        }
    }
}