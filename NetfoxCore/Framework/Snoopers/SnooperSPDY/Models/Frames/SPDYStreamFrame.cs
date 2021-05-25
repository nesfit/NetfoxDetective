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
using System.Text;

namespace Netfox.Snoopers.SnooperSPDY.Models.Frames
{
    public abstract class SPDYStreamFrame : SPDYControlFrame
    {
        public readonly int StreamID;
        public readonly Dictionary<string, string> Fields;

        protected abstract int HeaderBlockOffset { get; }

        protected SPDYStreamFrame(Byte[] header, Byte[] data) : base(header)
        {
            this.Fields = new Dictionary<string, string>();

            this.StreamID = SPDYFrameBase.ReadInt(data, 0, 4) & 0x7fffffff;

            this.ParseHeaderBlock(data, this.HeaderBlockOffset);
        }
    
        protected void ParseHeaderBlock(Byte[] compressedData, int headerBlockOffset)
        {
            // Decompress data
            var data = SPDYFrameDecompressor.Instance.Decompress(compressedData, headerBlockOffset);

            // zlib.net library still contains bug which can result in invalid decompressed data
                var fieldCount = SPDYFrameBase.ReadInt(data, 0, 4);
                var readedBytes = 4;
                for (var i = 0; i < fieldCount && readedBytes < data.Length; i++)
                {
                    // Read name
                    var nameLen = SPDYFrameBase.ReadInt(data, readedBytes, 4);
                    readedBytes += 4;
                    var name = Encoding.ASCII.GetString(data, readedBytes, nameLen);
                    readedBytes += nameLen;

                    //Read value
                    var valueLen = SPDYFrameBase.ReadInt(data, readedBytes, 4);
                    readedBytes += 4;
                    var value = Encoding.ASCII.GetString(data, readedBytes, valueLen);
                    readedBytes += valueLen;

                    this.Fields[name] = value;
                }
             
        }
    }
}