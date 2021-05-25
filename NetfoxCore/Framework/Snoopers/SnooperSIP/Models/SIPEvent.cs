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
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Netfox.Snoopers.SnooperSIP.Enums;
using Netfox.Snoopers.SnooperSIP.Models.Message;

namespace Netfox.Snoopers.SnooperSIP.Models
{
    public enum SipMethods
    {
        INVITE,
        ACK,
        BYE,
        REFER,
        REGISTER,
        trying,
        ringing,
        ok,
        Unknown_Request,
        Unknown_Status
    }

    public abstract class SIPEvent : SnooperExportedObjectBase
    {
        //public List<IExportSource> ExportSources = new List<IExportSource>();
        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FrameGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }
        protected SIPEvent() { }//EF
        protected SIPEvent(SnooperExportBase exportBase) : base(exportBase) { this.TimeStamp = DateTime.MinValue; }

        public string From { get; protected set; } = string.Empty;
        public string To { get; protected set; } = string.Empty;
        //public new DateTime TimeStamp { get; set; }
        public SIPEventType Type { get; protected set; }

        public List<SipMethods> SipMethods { get; } = new List<SipMethods>();
        public List<SIPMsg> SipMessages { get; } = new List<SIPMsg>();

        //public List<IExportSource> ExportSources = new List<IExportSource>();

		public static List<string> CallMethods = new List<string>() { "INVITE", "ACK", "BYE", "REFER" }; //must be uppercase
		public static List<string> CallStatuses = new List<string>() { "trying", "ringing" };
		public static List<string> AuthentizationMethods = new List<string>() { "REGISTER" };
        private PersistableJsonSerializableGuid _framesGuids;

        //public new abstract string ToString();
        public virtual void Update(SIPMsg message)
        {
            this.SipMessages.Add(message);
            switch (message.Type)
            {
                case SIPMsg.SIPMsgType.Request:
                    switch(message.RequestLine.Method.ToUpper())
                    {
                        case "INVITE":
                            this.SipMethods.Add(Models.SipMethods.INVITE);
                            break;
                        case "ACK":
                            this.SipMethods.Add(Models.SipMethods.ACK);
                            break;
                        case "BYE":
                            this.SipMethods.Add(Models.SipMethods.BYE);
                            break;
                        case "REFER":
                            this.SipMethods.Add(Models.SipMethods.REFER);
                            break;
                        case "REGISTER":
                            this.SipMethods.Add(Models.SipMethods.REGISTER);
                            break;
                        default:
                            this.SipMethods.Add(Models.SipMethods.Unknown_Request);
                            break;
                    }
                    break;
                case SIPMsg.SIPMsgType.Status:
                    if(message.StatusLine.StatusInfo.ToLower().Contains("trying")) {
                        this.SipMethods.Add(Models.SipMethods.trying);
                    }
                    else if(message.StatusLine.StatusInfo.ToLower().Contains("ringing")) {
                        this.SipMethods.Add(Models.SipMethods.ringing);
                    }
                    else if(message.StatusLine.StatusInfo.ToLower().Contains("ok")) {
                        this.SipMethods.Add(Models.SipMethods.ok);
                    }
                    else
                    {
                        this.SipMethods.Add(Models.SipMethods.Unknown_Status);
                    }
                    break;
            }
        }

        public void UpdateFromUnkownEvent(SIPUnknownEvent unknownEvent)
        {
            //this.From = unknownEvent.From;
            //this.To = unknownEvent.To;
            //this.SipMethods.AddRange(unknownEvent.SipMethods);
            foreach(var message in unknownEvent.SipMessages) { this.Update(message); }
        }

        protected void ProcessExportSources(SIPMsg message)
        {
            foreach(var exportSource in message.ExportSources) { if(!this.ExportSources.Contains(exportSource)) { this.ExportSources.Add(exportSource); } }
        }

        protected void ProcessFromTo(SIPMsg message)
        {
            this.Frames = this.Frames.Concat(message.Frames).ToList();
            //Console.WriteLine("SIPEvent: ProcessFromTo()");
            if(this.From == string.Empty) {
                this.From = message.Headers.From;
            }
            else
            {
                if(message.Headers.From.Length > this.From.Length && message.Headers.From.Contains(this.From)) { this.From = message.Headers.From; }
            }

            //Get or update "To" value
            if(this.To == string.Empty) {
                this.To = message.Headers.To;
            }
            else
            {
                if(message.Headers.To.Length > this.To.Length && message.Headers.To.Contains(this.To)) { this.To = message.Headers.To; }
            }
        }
    }
}