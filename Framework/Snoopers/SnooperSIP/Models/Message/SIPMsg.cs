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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Netfox.Core.Database;
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.SnooperSIP.Models.Message
{

    public class SIPMsg: IEntity
    {
        #region Implementation of IEntity
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        public int OrderKey { get; set; }
        #endregion
        public enum SIPMsgType
        {
            Request,
            Status
        }
        [NotMapped]
        public SIPBody Body
        {
            get { return this.BodyDb.IsPresent ? this.BodyDb : null; }
            private set { this.BodyDb = value; }
        }

        public SIPBody BodyDb { get; private set; } = new SIPBody();
        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FrameGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }

        [NotMapped]
        public SIPHeaders Headers
        {
            get { return this.HeadersDb.IsPresent ? this.HeadersDb : null; }
            private set { this.HeadersDb = value; }
        }

        public SIPHeaders HeadersDb { get; private set; } = new SIPHeaders();
        public string InvalidReason;
        private bool _valid;
        private PersistableJsonSerializableGuid _framesGuids;

        [NotMapped]
        public SIPRequestLine RequestLine
        {
            get { return this.RequestLineDb.IsPresent? this.RequestLineDb:null; }
            private set { this.RequestLineDb = value; }
        }
        internal SIPRequestLine RequestLineDb { get; private set; } = new SIPRequestLine();

        [NotMapped]
        public SIPStatusLine StatusLine
        {
            get { return this.StatusLineDb.IsPresent ? this.StatusLineDb : null; }
            private set { this.StatusLineDb = value; }
        }

        internal SIPStatusLine StatusLineDb { get; private set; } = new SIPStatusLine();
        //public string ID;
        //public string CallID;
        public DateTime Timestamp => this.FirstSeen;
        public SIPMsgType Type { get; private set; }

        public bool Valid
        {
            get { return this._valid; }
            set { this._valid = value; }
        }

        public List<IExportSource> ExportSources { get; }

        public SIPMsg(PDUStreamReader reader)
        {
            this.Valid = true;
            this.InvalidReason = string.Empty;
            this.ExportSources = new List<IExportSource>();
            this.Parse(reader);
        }

        public void Parse(PDUStreamReader reader)
        {
            //Console.WriteLine("parse");
            var stream = reader.PDUStreamBasedProvider;
            //if(stream == null)
            //    Console.WriteLine("stream null");

            //Console.WriteLine("before stream.GetCurrentPDU().FirstSeen");
            if(stream.GetCurrentPDU() != null)
            {
                this.FirstSeen = stream.GetCurrentPDU().FirstSeen;
                this.ExportSources.Add(stream.GetCurrentPDU());
            }
            else this.ExportSources.Add(stream.Conversation);
            //Console.WriteLine("after stream.GetCurrentPDU().FirstSeen");
            this.Frames.AddRange(stream.ProcessedFrames); 
            //Console.WriteLine("parsing of SIP message, frame numbers: " + string.Join(",", this.FrameGuids.ToArray()));

            var startLine = reader.ReadLine();
            var startLineSplit = new string[] {""};
            if (startLine != null)
                startLineSplit = startLine.Split(' ');
            if(startLineSplit[0].IndexOf("SIP", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // status
                this.Type = SIPMsgType.Status;
                this.StatusLine = new SIPStatusLine(startLineSplit, out this._valid, out this.InvalidReason);
            }
            else
            {
                // request
                this.Type = SIPMsgType.Request;
                this.RequestLine = new SIPRequestLine(startLineSplit, out this._valid, out this.InvalidReason);
            }
            if(!this.Valid) { return; }

            // message header
            this.Headers = new SIPHeaders(reader, this);
            if(this.Headers == null) this.Valid = false;
            if(this.Valid) { this.Body = new SIPBody(reader, this); }
        }

        public override string ToString()
        {
            var converted = string.Empty;
            switch(this.Type)
            {
                case SIPMsgType.Request:
                    converted += "REQUEST: ";
                    converted += this.RequestLine.ToString();
                    break;
                case SIPMsgType.Status:
                    converted += "STATUS: ";
                    converted += this.StatusLine.ToString();
                    break;
                default:
                    converted += "unset message type:";
                    break;
            }
            converted += this.Headers?.ToString();
            converted += this.Body?.ToString();

            return converted + "\n = = = = = = = =";
        }

        #region Implementation of IEntity
        public DateTime FirstSeen { get; private set; }
        #endregion
    }
    
}