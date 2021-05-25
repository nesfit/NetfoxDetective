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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Core.Database.Wrappers;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperSIP.Enums;
using Netfox.Snoopers.SnooperSIP.Interfaces;
using Netfox.Snoopers.SnooperSIP.Models.Message;

namespace Netfox.Snoopers.SnooperSIP.Models
{
    public class SIPCall : SIPEvent, ISIPCall
    {
        private SIPCall() { }//EF
        public SIPCall(SnooperExportBase exportBase) : base(exportBase)
        {
            this.Type = SIPEventType.Call;
            //TimeStamp = new DateTime(2014, 1, 31, 21, 15, 07);
            //_end = new DateTime(2015, 2, 1, 21, 16, 07);
            this.State = SipCallState.UNKNOWN;
            this.RejectReason = string.Empty;
            //this.ExportedPayloads = new RTPExportedPayload[0];
            this.CallId = string.Empty;
            this.RTPAddressesString = new PersistableJsonSerializableString();
            this.PossibleCodecsString = new PersistableJsonSerializableString();
        }

        public PersistableJsonSerializableString RTPAddressesString { get; set; }
        public PersistableJsonSerializableString PossibleCodecsString { get; set; }


        public List<IPEndPointEF> RTPAddresses { get; private set; } = new List<IPEndPointEF>();
        [NotMapped]
        public TimeSpan? Duration => this?.End - this?.Start;
        [Column(TypeName = "DateTime2")]
        public DateTime? End { get; private set; }
        public IEnumerable<string> PossibleCodecs { get; private set; } = new List<string>();
        
        public IEnumerable<IPEndPointEF> CallStreamAddresses => this.RTPAddresses.ToArray();
       
        public IList<ICallStream> CallStreams { get; private set; } = new List<ICallStream>();
       
        public IList<ICallStream> PossibleCallStreams { get; private set; } = new List<ICallStream>();
        [NotMapped]
        public DateTime? Start => this.TimeStamp;

        //#region Implementation of IExportBase
        //public List<IExportSource> ExportSources { get; private set; }
        //#endregion

        //public RTPExportedPayload[] ExportedPayloads { get; private set; } 
        public string CallId { get; private set; }
        public string RejectReason { get; private set; }
        //private string From;
        //private string To;
        public SipCallState State { get; private set; }
        //public void SetExportedPayloads(RTPExportedPayload[] exportedPayloads) { this.ExportedPayloads = exportedPayloads; }

        public override string ToString()
        {
            var converted = new StringBuilder();
            converted.AppendLine("call " + this.CallId + ":");
            converted.AppendLine(" Begin:    " + this.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            converted.AppendLine(" End:      " + this.End?.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            converted.AppendLine(" Duration: " + (this.End - this.TimeStamp)?.ToString(@"d' days, 'hh\:mm\:ss\.ffffff"));
            converted.AppendLine(" From:     " + this.From);
            converted.AppendLine(" To:       " + this.To);
            if(this.RTPAddresses.Count > 0)
            {
                converted.Append(" RTP:      ");
                foreach(var address in this.RTPAddresses) { converted.Append(address + ", "); }
                converted.AppendLine();
            }
            if(this.SipMethods.Count > 0)
            {
                converted.Append(" Methods:  ");
                foreach(var method in this.SipMethods) { converted.Append(method + ", "); }
                converted.AppendLine();
            }
            if (this.PossibleCodecs.Any())
            {
                converted.Append(" Codecs:   ");
                foreach (var codec in this.PossibleCodecs) { converted.Append(codec + ", "); }
                converted.AppendLine();
            }
            converted.Append(" state:    ");
            switch(this.State)
            {
                case SipCallState.CANCELLED:
                    converted.Append("CANCELLED");
                    break;
                case SipCallState.COMPLETE:
                    converted.Append("COMPLETE");
                    break;
                case SipCallState.ESTABLISHING:
                    converted.Append("ESTABLISHING");
                    break;
                case SipCallState.IN_PROGRESS:
                    converted.Append("IN PROGRESS");
                    break;
                case SipCallState.REJECTED:
                    converted.Append("REJECTED (" + this.RejectReason + ")");
                    break;
                case SipCallState.UNKNOWN:
                    converted.Append("UNKNOWN");
                    break;
                default:
                    //TODO throw exception, this should never happen
                    break;
            }
            //foreach(var exportedPayload in this.ExportedPayloads) { converted += "\n" + exportedPayload; }
            return converted.ToString();
        }

        public override void Update(SIPMsg message)
        {
            base.Update(message);
            //Process From and To fields
            this.ProcessFromTo(message);
            this.ProcessExportSources(message);

            //Process timestamps
            if(this.TimeStamp == DateTime.MinValue || this.TimeStamp > message.Timestamp) { this.TimeStamp = message.Timestamp; }
            this.End = message.Timestamp;

            //Process state
            switch(message.Type)
            {
                case SIPMsg.SIPMsgType.Request:
                    switch(message.RequestLine.Method)
                    {
                        case "INVITE":
                            this.State = SipCallState.ESTABLISHING;
                            break;
                        case "CANCEL":
                            if(this.State == SipCallState.ESTABLISHING || this.State == SipCallState.IN_PROGRESS) { this.State = SipCallState.CANCELLED; }
                            break;
                        case "BYE":
                            if(this.State == SipCallState.IN_PROGRESS) { this.State = SipCallState.COMPLETE; }
                            break;
                        default:
                            break;
                    }
                    break;
                case SIPMsg.SIPMsgType.Status:
                    switch(this.State)
                    {
                        case SipCallState.ESTABLISHING:
                            if(message.StatusLine.StatusCode == "200") { this.State = SipCallState.IN_PROGRESS; }
                            if(message.StatusLine.StatusCode[0] == '4')
                            {
                                this.State = SipCallState.REJECTED;
                                this.RejectReason = message.StatusLine.StatusInfo;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
            }

            if(message.Headers.CallID != string.Empty && this.CallId == string.Empty) { this.CallId = message.Headers.CallID; }

            //Get or update "RTPAddress" values
            if(message.Body != null && (message.Body.RTPAddress != string.Empty && message.Body?.RTPPort != string.Empty))
            {
                var found = false;
                foreach(var address in this.RTPAddresses)
                {
                    var addressString = message.Body.RTPAddress + ":" + message.Body.RTPPort; //address.ToString();
                    if(addressString == address.ToString())
                    {
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    try
                    {
                        this.RTPAddresses.Add(new IPEndPointEF(IPAddress.Parse(message.Body.RTPAddress), int.Parse(message.Body.RTPPort)));
                        this.RTPAddressesString.Add(new IPEndPointEF(IPAddress.Parse(message.Body.RTPAddress), int.Parse(message.Body.RTPPort)).ToString());
                    }
                    catch
                    {
                        //TODO throw some kind of "improper IP:port format" exception
                    }
                }
            }

            if(message.Body != null)
                foreach(var codec in message.Body.PossibleCodecs)
                {
                    if(!this.PossibleCodecs.Contains(codec))
                    {
                        var list = this.PossibleCodecs as List<string>;
                        list.Add(codec);
                        this.PossibleCodecsString.Add(codec);
                    }
                }
        }
    }
}