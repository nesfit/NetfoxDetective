// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Viliam Letavay
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
using System.IO;
using Netfox.Framework.Models;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public abstract class PDUDataProvider : Stream
    {
        public abstract override Int64 Length { get; }
        public abstract override Boolean CanRead { get; }
        public abstract override Boolean CanSeek { get; }
        public abstract override Boolean CanWrite { get; }
        public abstract override Int64 Position { get; set; }
        public abstract override void Flush();
        public abstract L7PDU GetCurrentPDU();
        public abstract Boolean NewMessage();
        public abstract Int32 Peek(Byte[] buffer, Int32 bufferOffset, Int32 streamPosition, Int32 requestedCount, SeekOrigin origin);
        public abstract override Int32 Read(Byte[] buffer, Int32 offset, Int32 requestedCount);
        public abstract Boolean Reset();
        public abstract override Int64 Seek(Int64 offset, SeekOrigin origin);
        public abstract override void SetLength(Int64 value);
        public abstract override void Write(Byte[] buffer, Int32 offset, Int32 count);
    }
}