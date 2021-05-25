// Copyright (c) 2017 Jan Pluskal, Martin Kmet
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
using System.Text;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Core.Database.Wrappers;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Snoopers.SnooperRTP.Models
{
    public class SnooperExportedObjectBaseRtpRtcp : SnooperExportedObjectBase, ICall, ICallStream
    {
        private IPEndPointEF[] _callStreamAddresses;
        private PersistableJsonSerializableDictionaryStringString _codecsToDecodedFilesDictionary;
        private IList<ICallStream> _callStreams;

        private SnooperExportedObjectBaseRtpRtcp() { } //EF
        public SnooperExportedObjectBaseRtpRtcp(SnooperExportBase exportBase) : base(exportBase)
        {
        }

        public UInt32 SSRC { get; set; }
        public string PayloadType { get; set; }
        public string TimeDelta { get; set; }
        public string PayloadLen { get; set; }

        public string PossibleCodecsStr
        {
            get
            {
                var sb = new StringBuilder();
                var first = true;
                if(this.PossibleCodecs == null) { return "Undetected"; }
                foreach(var possibleCodec in this.PossibleCodecs)
                {
                    sb.Append(possibleCodec);
                    if(!first) { sb.Append(", "); }
                    else
                    { first = false; }
                }
                return sb.ToString();
            }
        }

        public uint? BytesCnt { get; set; }
        public uint? PacketsCnt { get; set; }
        public uint? LostPacketsCnt { get; set; }
        // could multiple lines string or empty string
        public string RTCPinfo { get; set; }

        [NotMapped]
        public string PayloadFileName => this.SSRC + ".raw";

        public string PayloadFilePath { get; set; }
        //public string DecodedPayloadFilePath { get { xxx; } }

        /// <summary>
        ///     Map possible codecs to decoded filePaths
        /// </summary>
        public PersistableJsonSerializableDictionaryStringString CodecsToDecodedFilesDictionary
        {
            get { return this._codecsToDecodedFilesDictionary ?? (this._codecsToDecodedFilesDictionary = new PersistableJsonSerializableDictionaryStringString()); }
            set { this._codecsToDecodedFilesDictionary = value; }
        }

        public virtual L7Conversation RTPConversation { get; set; }
        public virtual L7Conversation RTCPConversation { get; set; }
        internal byte PtNum { get; set; }
        [NotMapped]
        public IEnumerable<IPEndPointEF> CallStreamAddresses
        {
            get
            {
                if(this._callStreamAddresses != null) { return this._callStreamAddresses; }
                this._callStreamAddresses = new IPEndPointEF[2];
                this._callStreamAddresses[0] = new IPEndPointEF(this.SourceEndPoint);
                this._callStreamAddresses[1] = new IPEndPointEF(this.DestinationEndPoint);
                return this._callStreamAddresses;
            }
        }
        [NotMapped]
        public virtual IList<ICallStream> CallStreams
        {
            get { return this._callStreams ?? (this._callStreams = new List<ICallStream>{this});}
            private set { this._callStreams = value; }
        }

        public TimeSpan? Duration => this.End - this.Start;

        public DateTime? End { get; set; }
        [NotMapped]
        public string From => (this.SourceEndPoint?.ToString()) ?? this.SourceEndpointString;
        [NotMapped]
        public virtual IList<ICallStream> PossibleCallStreams { get; private set; } = new List<ICallStream>();

        //public string[] PossibleCodecs { get { return CodecsToDecodedFilesDictionary.Keys.ToArray(); }}
        public IEnumerable<string> PossibleCodecs { get; set; }
        public DateTime? Start { get; set; }
        [NotMapped]
        public string To => (this.DestinationEndPoint?.ToString()) ?? this.DestinationEndpointString;

        public string WavFilePath { get; set; }

        internal void UpdateValues(SnooperExportedObjectBaseRtpRtcp other)
        {
            foreach(var item in other.CodecsToDecodedFilesDictionary) { this.CodecsToDecodedFilesDictionary.Add(item.Key, item.Value); }
            this.Start = this.Start ?? other.Start;
            this.End = this.End ?? other.End;
            this.PayloadType = this.PayloadType ?? other.PayloadType;
            this.TimeDelta = this.TimeDelta ?? other.TimeDelta;
            this.PayloadLen = this.PayloadLen ?? other.PayloadLen;
            this.BytesCnt = this.BytesCnt ?? other.BytesCnt;
            this.PacketsCnt = this.PacketsCnt ?? other.PacketsCnt;
            this.LostPacketsCnt = this.LostPacketsCnt ?? other.LostPacketsCnt;
            this.RTCPinfo = this.RTCPinfo ?? other.RTCPinfo;
            this.PayloadFilePath = this.PayloadFilePath ?? other.PayloadFilePath;
            this.PossibleCodecs = this.PossibleCodecs ?? other.PossibleCodecs;
            this.RTPConversation = this.RTPConversation ?? other.RTPConversation;
            this.RTCPConversation = this.RTCPConversation ?? other.RTCPConversation;
        }
    }
}