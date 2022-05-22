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
using Netfox.Snoopers.SnooperSIP.Interfaces;
using Netfox.Snoopers.SnooperSIP.Models.Message;

namespace Netfox.Snoopers.SnooperSIP.Models
{
    public class SIPAuthentization : SIPEvent, IExportSIPAuthentization
    {
        public SipAuthenticationState State { get; private set; }
        public string RejectReason { get; private set; }
        
        private SIPAuthentization() { }//EF
        public SIPAuthentization(SnooperExportBase exportBase) : base(exportBase)
        {
            this.Type = SIPEventType.Authentization;
            //TimeStamp = new DateTime(2014, 1, 31, 21, 15, 07);
            this.State = SipAuthenticationState.UNKNOWN;
            this.RejectReason = string.Empty;
        }

        public override string ToString()
        {
	        var converted = new StringBuilder();
            converted.AppendLine("authentization:");
            converted.AppendLine(" Date:     " + this.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            converted.AppendLine(" From:     " + this.From);
            converted.AppendLine(" To:       " + this.To);
            converted.Append(" state:    ");
            switch(this.State)
            {
                case SipAuthenticationState.SUCCESSFUL:
                    converted.Append("SUCCESSFUL");
                    break;
                case SipAuthenticationState.ESTABLISHING:
                    converted.Append("ESTABLISHING");
                    break;
                case SipAuthenticationState.REJECTED:
                    converted.Append("REJECTED (" + this.RejectReason + ")");
                    break;
                case SipAuthenticationState.UNKNOWN:
                    converted.Append("UNKNOWN");
                    break;
                default:
                    //TODO throw exception, this should never happen
                    break;
            }
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

            //Process state
            switch (message.Type)
            {
                case SIPMsg.SIPMsgType.Request:
                    switch(message.RequestLine.Method)
                    {
                        case "REGISTER":
                            this.State = SipAuthenticationState.ESTABLISHING;
                            break;
                        default:
                            break;
                    }
                    break;
                case SIPMsg.SIPMsgType.Status:
                    switch(this.State)
                    {
                        case SipAuthenticationState.ESTABLISHING:
                            if(message.StatusLine.StatusCode == "200") { this.State = SipAuthenticationState.SUCCESSFUL; }
                            if(message.StatusLine.StatusCode[0] == '4')
                            {
                                this.State = SipAuthenticationState.REJECTED;
                                this.RejectReason = message.StatusLine.StatusInfo;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
	}
}