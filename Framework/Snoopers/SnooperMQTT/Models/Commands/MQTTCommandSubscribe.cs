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
using Netfox.Core.Database.PersistableJsonSeializable;

namespace Netfox.SnooperMQTT.Models.Commands
{
    public class MQTTCommandSubscribe : MQTTCommandBase
    {
        public override MQTTCommandType Type => MQTTCommandType.Subscribe;

        public PersistableJsonSerializableString Topics { get; private set; } = new PersistableJsonSerializableString();
        public MQTTCommandSubscribe() { } //EF

        public MQTTCommandSubscribe(Byte[] commandData)
        {
            var readedBytes = 0;
            while(readedBytes < commandData.Length)
            {
                var topicName = this.ReadUtf8StringFromBuffer(commandData, readedBytes);
                this.Topics.Add(topicName);

                // 2B topic len, NB topic, 1B QOS
                readedBytes += 2 + topicName.Length + 1;
            }
        }
        
    }
}