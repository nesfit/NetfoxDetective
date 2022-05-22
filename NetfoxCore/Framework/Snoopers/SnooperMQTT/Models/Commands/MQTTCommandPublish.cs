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
    public class MQTTCommandPublish : MQTTCommandBase
    {
        public override MQTTCommandType Type => MQTTCommandType.Publish;

        public string Topic { get; private set; }
        public byte[] Payload { get; private set; }

        public MQTTCommandPublish()
        {
        } //EF

        public MQTTCommandPublish(Byte[] commandData)
        {
            // Read topic
            this.Topic = this.ReadUtf8StringFromBuffer(commandData, 0);

            // Read payload
            // 2B topic len, NB topic, 2B MsgId
            var payloadStartIndex = 2 + this.Topic.Length + 2;
            var payloadLen = commandData.Length - payloadStartIndex;
            this.Payload = new Byte[payloadLen];
            Array.Copy(commandData, payloadStartIndex, this.Payload, 0, payloadLen);
        }
    }
}