// Copyright (c) 2017 Jan Pluskal, Viliam Letavay
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

namespace Netfox.Snoopers.SnooperMQTT.Models.Commands
{
    public class MQTTCommandConnect : MQTTCommandBase
    {
        public override MQTTCommandType Type => MQTTCommandType.Connect;
        public string ProtocolName { get; private set; }
        public byte ProtocolVersionNumber { get; private set; }
        public byte Flags { get; private set; }
        public byte[] Payload { get; private set; }

        public MQTTCommandConnect()
        {
        } //EF

        public MQTTCommandConnect(Byte[] commandData)
        {
            this.ProtocolName = this.ReadUtf8StringFromBuffer(commandData, 0);
            var readedBytes = 2 + this.ProtocolName.Length;

            this.ProtocolVersionNumber = commandData[readedBytes];
            readedBytes++;

            this.Flags = commandData[readedBytes];
            readedBytes++;

            // Skip Keepalive
            readedBytes += 2;

            var payloadLen = commandData.Length - readedBytes;
            this.Payload = new Byte[payloadLen];
            Array.Copy(commandData, readedBytes, this.Payload, 0, payloadLen);
        }
    }
}