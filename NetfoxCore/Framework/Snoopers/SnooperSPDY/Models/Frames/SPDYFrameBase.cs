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
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Snoopers.SnooperSPDY.Models.Frames
{
    public abstract class SPDYFrameBase
    {
        public readonly Byte Flags;
        public readonly bool Fin;

        public List<PmFrameBase> Frames; 

        protected SPDYFrameBase(Byte[] header)
        {
            // https://www.chromium.org/spdy/spdy-protocol/spdy-protocol-draft2

            this.Flags = header[4];
            this.Fin = (this.Flags & 0x01) == 1;
        }


        // Read big endian int from buffer
        public static int ReadInt(Byte[] data, int offset, int bytesCount)
        {
            if(bytesCount <= 0 || offset < 0 || offset >= data.Length) throw new Exception("Invalid ReadInt parameters");

            var num = 0;
            for(var i = 0; i < bytesCount; i++)
            {
                num += data[offset + i] << ((bytesCount - 1 - i) * 8);     
            }

            return num;
        }
    }
}