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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Netfox.Core.Database;

namespace Netfox.SnooperMQTT.Models.Commands
{
    // http://public.dhe.ibm.com/software/dw/webservices/ws-mqtt/mqtt-v3r1.html#msg-format
    public enum MQTTCommandType
    {
        Connect = 1,
        Connack,
        Publish,
        Puback,
        Pubrec,
        Pubrel,
        Pubcomp,
        Subscribe,
        Suback,
        Unsubcribe,
        Unsuback,
        Pingreq,
        Pingresp,
        Disconnect
    }
    [Persistent]
    public abstract class MQTTCommandBase: IEntity
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        protected MQTTCommandBase() {} //EF

        [NotMapped]
        public abstract MQTTCommandType Type { get;  } 

        public string ReadUtf8StringFromBuffer(Byte[] buffer, int offset)
        {
            // http://public.dhe.ibm.com/software/dw/webservices/ws-mqtt/mqtt-v3r1.html#utf-8

            // First two bytes are length of the string (Big endian)
            var stringLen = (buffer[offset] << 8) + buffer[offset + 1];

            if ((stringLen + 2) > buffer.Length)
                throw new Exception("String too long");

            // Read string
            return System.Text.Encoding.UTF8.GetString(buffer, offset + 2, stringLen);
        }

        #region Implementation of IEntity
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; set; }
        #endregion
    }
}