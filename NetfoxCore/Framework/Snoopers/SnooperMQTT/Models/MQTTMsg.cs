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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Snoopers.SnooperMQTT.Models.Commands;

namespace Netfox.Snoopers.SnooperMQTT.Models
{
    
    public class MQTTMsg : SnooperMQTTExportedObject
    {
        private PersistableJsonSerializableGuid _framesGuids;

        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FrameGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }
        public bool Valid { get; set; }
        public string InvalidReason { get; set; }
        protected PDUStreamReader Reader { get; }
        
        public virtual MQTTCommandBase Command { get; set; }

        public MQTTMsg() { }

        public MQTTMsg(PDUStreamReader reader)
        {
            this.Reader = reader;
            this.InvalidReason = null;

            this.Parse();
        }

        protected void Parse()
        {
            try
            {
                this.Command = this.GetCommand();
            }
            catch(Exception e)
            {
                this.InvalidReason = e.ToString();
                return;
            }
            
            this.ExportSources.Add(this.Reader.PDUStreamBasedProvider.GetCurrentPDU());
        }

        protected MQTTCommandBase GetCommand()
        {
            // Read fixed header
            // Read header flags
            var headerFlags = this.Reader.ReadByte();
            // High 4 bits of header flags
            var commandType = (MQTTCommandType)((headerFlags & 0xf0) >> 4);
            // Get command length using VLQ
            var commandLen = 0;
            var multiplier = 1;
            var digit = 0;
            do
            {
                digit = this.Reader.ReadByte();
                commandLen += (digit & 127) * multiplier;
                multiplier *= 128;
            } while ((digit & 128) != 0);

            var headerPdu = this.Reader.PDUStreamBasedProvider.GetCurrentPDU();
            var headerFrames = headerPdu.FrameList;
            this.TimeStamp = headerPdu.FirstSeen;

            // Read rest of the command
            var commandData = new Byte[commandLen];
            var totalReadedBytes = 0;
            var readedBytes = this.Reader.Read(commandData, totalReadedBytes, commandLen - totalReadedBytes);

            // Save frame numbers
            var payloadFrames = this.Reader.PDUStreamBasedProvider.GetCurrentPDU().FrameList;
            this.Frames.AddRange(headerFrames.Union(payloadFrames));

            if (readedBytes != commandLen)
            {
                this.InvalidReason = "Not enough data for command";
                return null;
            }

            // Create appropriate MQTT command object based on read type
            MQTTCommandBase command;
            switch (commandType)
            {
                case MQTTCommandType.Connect:
                    command = new MQTTCommandConnect(commandData);
                    break;
                case MQTTCommandType.Subscribe:
                    command = new MQTTCommandSubscribe(commandData);
                    break;
                case MQTTCommandType.Publish:
                    command = new MQTTCommandPublish(commandData);
                    break;
                case MQTTCommandType.Puback:
                    command = new MQTTCommandPublishAck();
                    break;
                case MQTTCommandType.Pubrel:
                    command = new MQTTCommandPublishRel();
                    break;
                case MQTTCommandType.Pubcomp:
                    command = new MQTTCommandPublishComp();
                    break;
                case MQTTCommandType.Pubrec:
                    command = new MQTTCommandPublishRec();
                    break;
                case MQTTCommandType.Pingreq:
                    command = new MQTTCommandPingRequest();
                    break;
                case MQTTCommandType.Pingresp:
                    command = new MQTTCommandPingResponse();
                    break;
                default:
                    this.InvalidReason = "Unsupported MQTT command: " + commandType;
                    return null;
            }
            return command;
        }


        public override string ToString()
        {
            var description = "";
            if(this.Command != null) { description += "MQTTCommand: " + this.Command.Type + " "; }
            if(this.InvalidReason != null) { description += "InvalidReason: " + this.InvalidReason + " "; }
            if(this.Frames != null) { description += "Frames: " + String.Join(",", this.Frames);}
            return description+base.ToString();
        }
        
    }
}