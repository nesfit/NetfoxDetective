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
using System.Text;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperSIP.Enums;
using Netfox.Snoopers.SnooperSIP.Models.Message;

namespace Netfox.Snoopers.SnooperSIP.Models
{
    public class SIPUnknownEvent : SIPEvent
    {
        private SIPUnknownEvent() { }//EF
        public SIPUnknownEvent(SnooperExportBase exportBase) : base(exportBase)
        {
            this.Type = SIPEventType.Unknown;
        }

        public override string ToString()
        {
	        var converted = new StringBuilder();
            converted.AppendLine("unknown:");
            converted.AppendLine(" Date:     " + this.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            converted.AppendLine(" From:     " + this.From);
            converted.Append(" To:       " + this.To);
            return converted.ToString();
        }

        public override void Update(SIPMsg message)
        {
            base.Update(message);
            //Process From and To fields
            this.ProcessFromTo(message);
            this.ProcessExportSources(message);

            //Process timestamp
            if (this.TimeStamp == DateTime.MinValue || this.TimeStamp > message.Timestamp)
            {
                this.TimeStamp = message.Timestamp;
            }
        }
	}
}