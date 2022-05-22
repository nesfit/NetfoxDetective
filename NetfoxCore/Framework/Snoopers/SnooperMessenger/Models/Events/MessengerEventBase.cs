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
using System.IO;
using System.IO.Compression;
using System.Linq;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.Snoopers;
using Newtonsoft.Json.Linq;
using Thrift.Protocol;
using Thrift.Transport;

namespace Netfox.Snoopers.SnooperMessenger.Models.Events
{
    public enum StringExtractorState
    {
        Start,
        Length,
        String
    };

    public abstract class MessengerEventBase : SnooperExportedObjectBase
    {
        private PersistableJsonSerializableGuid _framesGuids;
        protected MessengerEventBase() { }
        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FrameGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }

        protected MessengerEventBase(SnooperExportBase exportBase) : base(exportBase) {}

        public static Byte[] DecompressPayload(Byte[] data)
        {
            // Decompress data using deflate algorithm

            using (var compressedStream = new MemoryStream(data, 2, data.Length - 2))       // Skip first two bytes as they are part of the zlib, not deflate
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using(var decompressedStream = new MemoryStream())
            {
                deflateStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }

        public static JObject DecompressJSONPayload(Byte[] data)
        {
            var decompressedPayload = DecompressPayload(data);
            var stringPayload = System.Text.Encoding.UTF8.GetString(decompressedPayload);
            return JObject.Parse(stringPayload);
        }

        public static TCompactProtocol CompactProtocolForPayload(Byte[] payload, int offset=1)
        {
            var inStream = new MemoryStream(payload, offset, payload.Length - offset);
            var tProto = new TCompactProtocol(new TStreamTransport(inStream, inStream));
            return tProto;
        }
    }
}
