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

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public class BigEndianPDUBinaryReader : BinaryReader
    {
        private byte[] _a16 = new byte[2];
        private byte[] _a32 = new byte[4];
        private byte[] _a64 = new byte[8];
        public BigEndianPDUBinaryReader(PDUStreamBasedProvider stream) : base(stream) { }
        public PDUStreamBasedProvider Stream => this.BaseStream as PDUStreamBasedProvider;

        public Boolean EndOfPDU => this.Stream.EndOfPDU;

        public override Int16 ReadInt16()
        {
            this._a16 = this.ReadBytes(2);
            Array.Reverse(this._a16);
            return BitConverter.ToInt16(this._a16, 0);
        }

        public override Int32 ReadInt32()
        {
            this._a32 = this.ReadBytes(4);
            Array.Reverse(this._a32);
            return BitConverter.ToInt32(this._a32, 0);
        }

        public override Int64 ReadInt64()
        {
            this._a64 = this.ReadBytes(8);
            Array.Reverse(this._a64);
            return BitConverter.ToInt64(this._a64, 0);
        }

        public override UInt16 ReadUInt16()
        {
            this._a16 = this.ReadBytes(2);
            Array.Reverse(this._a16);
            return BitConverter.ToUInt16(this._a16, 0);
        }

        public override UInt32 ReadUInt32()
        {
            this._a32 = this.ReadBytes(4);
            Array.Reverse(this._a32);
            return BitConverter.ToUInt32(this._a32, 0);
        }

        public override UInt64 ReadUInt64()
        {
            this._a64 = this.ReadBytes(8);
            Array.Reverse(this._a64);
            return BitConverter.ToUInt64(this._a64, 0);
        }
    }
}