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
using System.Runtime.Serialization;
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.SnooperSPDY.Models.Frames;

namespace Netfox.SnooperSPDY.Models
{
    public class PersistableJsonSerializableDictionaryIntToListSPDYFrameBase : PersistableJsonSerializableDictionary<int, List<SPDYFrameBase>>
    {
        public PersistableJsonSerializableDictionaryIntToListSPDYFrameBase() { }
    }
    public enum MessageType
    {
        Request,
        Response,
        Other
    }
    [ComplexType]
    public class SPDYMsg
    {
        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FramesGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f=>f.Id)); }
            set { this._framesGuids = value; }
        }

        public bool Valid { get; set; }
        public string InvalidReason { get; set; }
        protected PDUStreamReader Reader { get; }
        public DateTime TimeStamp { get; private set; }
        public List<IExportSource> ExportSources { get; private set; }
        public bool NothingToRead { get; set; }

        // Stores list of spdy frames (SYN_STREAM, SYN_REPLY and DATA) for given stream with id
        [NotMapped]
        protected static Dictionary<int, List<SPDYFrameBase>> StreamsFrames { get; private set; } = new Dictionary<int, List<SPDYFrameBase>>();
        //protected static PersistableJsonSerializableDictionaryIntToListSPDYFrameBase StreamsFrames { get; private set; } = new PersistableJsonSerializableDictionaryIntToListSPDYFrameBase();

        [NotMapped]
        public SPDYHeaderBase Header
        {
            get
            {
                if (this._header != null) return this._header;
                if (this.SPDYResponseHeader.IsPresent) return (this._header = this.SPDYResponseHeader);
                if (this.SPDYRequestHeader.IsPresent) return (this._header = this.SPDYRequestHeader);
                return this._header;
            }
            set
            {
                if (this._header == value) return;
                this._header = value;
                if (value is SPDYRequestHeader) this.SPDYRequestHeader = (SPDYRequestHeader)value;
                if (value is SPDYResponseHeader) this.SPDYResponseHeader = (SPDYResponseHeader)value;
            }
        }

        [DataMember(IsRequired = false)]
        public virtual SPDYRequestHeader SPDYRequestHeader
        {
            get
            {
                return this._spdyRequestHeader;
            }
            set
            {
                this._spdyRequestHeader = value;
                if (this._spdyRequestHeader == value) return;
                this._spdyRequestHeader = value;
                if (value.IsPresent)
                    this.Header = value;
            }
        }

        [DataMember(IsRequired = false)]
        public virtual SPDYResponseHeader SPDYResponseHeader
        {
            get
            {
                return this._spdyResponseHeader;
            }
            set
            {
                this._spdyResponseHeader = value;
                if (this._spdyResponseHeader == value) return;
                this._spdyResponseHeader = value;
                if (value.IsPresent)
                    this.Header = value;
            }
        }

        //[DataMember(IsRequired = false)]
        private SPDYContent _content;
        private SPDYHeaderBase _header;
        private SPDYRequestHeader _spdyRequestHeader  = new SPDYRequestHeader();
        private SPDYResponseHeader _spdyResponseHeader = new SPDYResponseHeader();
        private PersistableJsonSerializableGuid _framesGuids;

        public SPDYContent Content {
            get {
                if (this._content == null) { this._content = new SPDYContent();}
                return this._content;
            }
            set { this._content = value; }
        }


        public MessageType Type
        {
            get
            {
                if (this.Header == null)
                    return MessageType.Other;
                else if (this.Header is SPDYRequestHeader)
                    return MessageType.Request;
                else if (this.Header is SPDYResponseHeader)
                    return MessageType.Response;
                else
                    return MessageType.Other;
            }
        }

        protected SPDYMsg() { } //EF

        public SPDYMsg(PDUStreamReader reader)
        {
            this.Reader = reader;
            this.ExportSources = new List<IExportSource>();
            //this.Frames = new List<uint>();
            this.NothingToRead = false;

            this.Parse();
        }

        protected void Parse()
        {
            var streamProvider = this.Reader.PDUStreamBasedProvider;

            do
            {
                try
                {
                    var spdyFrame = this.GetSPDYFrame();
                    if(spdyFrame == null) { return; }

                    // Get stream id for SYN_STREAM, SYN_REPLY and DATA frames
                    var streamId = -1;
                    if(spdyFrame.GetType().IsSubclassOf(typeof(SPDYStreamFrame)))
                    {
                        var streamFrame = spdyFrame as SPDYStreamFrame;
                        streamId = streamFrame.StreamID;
                    }
                    else if(spdyFrame is SPDYFrameData)
                    {
                        var dataFrame = spdyFrame as SPDYFrameData;
                        streamId = dataFrame.StreamID;
                    }

                    // Save frame to given stream
                    if(streamId >= 0)
                    {
                        List<SPDYFrameBase> frames = null;
                        if(StreamsFrames.TryGetValue(streamId, out frames))
                        {
                            // Add frame to existing list
                            frames.Add(spdyFrame);
                        }
                        else
                        {
                            // Create new list of frames with given new frame
                            StreamsFrames.Add(streamId, new List<SPDYFrameBase>()
                            {
                                spdyFrame
                            });
                        }

                        // Reconstruct given stream if last frame was readed
                        if(spdyFrame.Fin)
                        {
                            this.ParseSPDYStream(StreamsFrames[streamId]);
                            StreamsFrames.Remove(streamId);
                            break;
                        }
                    }
                }
                catch(Exception e)
                {
                    this.InvalidReason = e.ToString();
                    return;
                }
                finally
                {
                    this.NothingToRead = !this.Reader.NewMessage();
                }
            } while(this.NothingToRead == false);
           
            this.ExportSources.Add(streamProvider.GetCurrentPDU());
            
        }

        protected SPDYFrameBase GetSPDYFrame()
        {
            var frameHeader = new Byte[8];
            if(this.Reader.Read(frameHeader, 0, frameHeader.Length) != frameHeader.Length)
            {
                this.Frames.AddRange(this.Reader.PDUStreamBasedProvider.GetCurrentPDU().FrameList);
                this.InvalidReason = "Not enough data for header";
                return null;
            }

            var headerPdu = this.Reader.PDUStreamBasedProvider.GetCurrentPDU();
            var headerFrames = headerPdu.FrameList;
            this.TimeStamp = headerPdu.FirstSeen;

            // First bit
            var isControl = (frameHeader[0] & 0x80) != 0;

            // Read payload
            // Payload len is in the last 3 bytes of 8 byte header
            var framePayloadLen = SPDYFrameBase.ReadInt(frameHeader, 5, 3);
            var framePayload = new Byte[framePayloadLen];
            var framePayloadReadBytes = this.Reader.Read(framePayload, 0, framePayloadLen);

            // Save frame numbers
            var payloadFrames = this.Reader.PDUStreamBasedProvider.GetCurrentPDU().FrameList;
            this.Frames.AddRange(headerFrames.Union(payloadFrames));

            if (framePayloadReadBytes != framePayloadLen)
            {
                this.InvalidReason = "Not enough data for payload";
                return null;
            }

            SPDYFrameBase spdyFrame;
            if(isControl)
            {
                // Get control frame type
                var controlFrameType = (SPDYControlFrameType)(SPDYFrameBase.ReadInt(frameHeader, 2, 2));
                switch(controlFrameType) {
                    case SPDYControlFrameType.SynStream:
                        spdyFrame = new SPDYFrameSynStream(frameHeader, framePayload);
                        break;
                    case SPDYControlFrameType.SynReply:
                        spdyFrame = new SPDYFrameSynReply(frameHeader, framePayload);
                        break;
                    case SPDYControlFrameType.Settings:
                        spdyFrame = new SPDYFrameSettings(frameHeader, framePayload);
                        break;
                    case SPDYControlFrameType.WindowUpdate:
                        spdyFrame = new SPDYFrameWindowUpdate(frameHeader, framePayload);
                        break;
                    case SPDYControlFrameType.Ping:
                        spdyFrame = new SPDYFramePing(frameHeader, framePayload);
                        break;
                    default:
                        throw new Exception("Unsupported SPDY control frame type: " + controlFrameType);
                }
            }
            else
            {
                spdyFrame = new SPDYFrameData(frameHeader, framePayload);
            }

            // Copy of a list is needed, not just assign
            spdyFrame.Frames = new List<PmFrameBase>(this.Frames);

            return spdyFrame;
        }

        // Reconstruct SPDY message from stream frames
        public void ParseSPDYStream(List<SPDYFrameBase> frames)
        {
            this.Frames.Clear();

            foreach(var spdyFrame in frames) {
                this.Frames.AddRange(spdyFrame.Frames);;

                if (spdyFrame is SPDYFrameSynStream)
                {
                    this.Header = new SPDYRequestHeader(spdyFrame as SPDYFrameSynStream);
                }
                else if(spdyFrame is SPDYFrameSynReply)
                {
                    this.Header = new SPDYResponseHeader(spdyFrame as SPDYFrameSynReply);
                }
                else if(spdyFrame is SPDYFrameData)
                {
                    if(this.Header == null)
                    {
                        this.InvalidReason = "Missing SPDY header for content";
                        return;
                    }

                    var contentEncoding = "";
                    this.Header.Fields.TryGetValue("content-encoding", out contentEncoding);
                    this.Content = new SPDYContent(spdyFrame as SPDYFrameData, contentEncoding);
                }
            }
            if(this.Header == null)
            {
                this.InvalidReason = "Missing SPDY header";
                return;
            }
        }
    }
}