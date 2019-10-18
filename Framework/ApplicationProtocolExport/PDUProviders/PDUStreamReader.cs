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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public class PDUStreamReader : StreamReader
    {
        private readonly Boolean _baseStreamRewinded;
        private readonly byte[] _binaryBuffer;
        private readonly bool _m2BytesPerChar;
        private readonly Decoder _mDecoder;
        private byte[] _mCharBytes;
        private Char[] _msgBuffer;
        private Int32 _msgLen;
        private Int32 _msgPos;
        private char[] _mSingleChar;
        private Int64 _readBytes;

        public PDUStreamReader(PDUStreamBasedProvider stream, Encoding encoding, Boolean rewindStream = false) : base(stream, encoding)
        {
            this._mDecoder = encoding.GetDecoder();
            this._m2BytesPerChar = encoding is UnicodeEncoding;
            var length = encoding.GetMaxByteCount(1);
            if(length < 16) { length = 16; }
            this._binaryBuffer = new byte[length];
            this.ReadBigEndian = false;

            this.PDUStreamBasedProvider = stream;
            this._baseStreamRewinded = rewindStream;
            if(rewindStream) { this.PDUStreamBasedProvider.Seek(0, SeekOrigin.Begin); }
        }

        public bool ReadBigEndian { get; set; }
        public PDUStreamBasedProvider PDUStreamBasedProvider { get; }
        public Boolean EndOfPDU => this.PDUStreamBasedProvider.EndOfPDU;
        public new Boolean EndOfStream => this.PDUStreamBasedProvider.EndOfStream && base.EndOfStream;

        public Boolean NewMessage()
        {
            this.PDUStreamBasedProvider.Seek(this._readBytes, SeekOrigin.Begin);
            this.DiscardBufferedData();
            this._readBytes = 0;
            return this.PDUStreamBasedProvider.NewMessage();
        }

        public override int Read()
        {
            if(this.PDUStreamBasedProvider == null) { throw new ObjectDisposedException("The stream is closed."); }
            return this.InternalReadOneChar();
        }

        /// <summary>
        ///     char represents a unicode character, and thus is two bytes (i.e. 16 bits) wide.
        ///     char[] (or better yet string) if you're dealing with strings
        ///     http://stackoverflow.com/questions/6711070/c-sharp-what-is-the-difference-between-byte-and-char
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override Int32 Read(Char[] buffer, Int32 index, Int32 count)
        {
            if(this._baseStreamRewinded) { throw new InvalidOperationException("Read(char[], ...) is valid only WITHOUT rewinded base stream"); }
            var len = base.Read(buffer, index, count);
            this._readBytes += len;
            return len;
        }

        /// <summary>
        ///     byte represents a byte. It is always 8-bits wide.
        ///     Use byte[] if you're dealing with raw bytes
        ///     http://stackoverflow.com/questions/6711070/c-sharp-what-is-the-difference-between-byte-and-char
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes
        /// requested if that many bytes are not currently available, or zero (0) if the end of the
        /// stream has been reached.</returns>
        public Int32 Read(Byte[] buffer, Int32 index, Int32 count)
        {
            if(!this._baseStreamRewinded) { throw new InvalidOperationException("Read(byte[], ...) is valid only WITH rewinded base stream"); }

            var len = this.PDUStreamBasedProvider.Read(buffer, index, count);
            this._readBytes += len;
            return len;
        }

        public override Task<Int32> ReadAsync(Char[] buffer, Int32 index, Int32 count) { throw new NotImplementedException(); }
        public override Int32 ReadBlock(Char[] buffer, Int32 index, Int32 count) { throw new NotImplementedException(); }
        public override Task<Int32> ReadBlockAsync(Char[] buffer, Int32 index, Int32 count) { throw new NotImplementedException(); }

        /// <exception cref="ObjectDisposedException">Thrown when the stream is closed.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the end of the stream is reached.</exception>
        public byte ReadByte()
        {
            if(this.PDUStreamBasedProvider == null) { throw new ObjectDisposedException("The stream is closed."); }
            var num1 = this.PDUStreamBasedProvider.ReadByte();
            var num2 = -1;
            if(num1 == num2) { throw new EndOfStreamException("The end of the stream is reached."); }
            this._readBytes += 1;
            return (byte) num1;
        }

        public char ReadChar()
        {
            var num1 = this.Read();
            const int num2 = -1;
            if(num1 == num2) { throw new EndOfStreamException("The end of the stream is reached."); }
            return (char) num1;
        }

        //TODO: not very safe, use with caution or rewrite
        public char[] ReadChars(int numOfChars)
        {
            var buffer = new char[numOfChars];
            for(var i = 0; i < numOfChars; i++) { buffer[i] = this.ReadChar(); }
            return buffer;
        }

        public short ReadInt16()
        {
            this.FillBuffer(2);

            if(this.ReadBigEndian) { return (short) (this._binaryBuffer[1]|this._binaryBuffer[0] << 8); }
            return (short) (this._binaryBuffer[0]|this._binaryBuffer[1] << 8);
        }

        public int ReadInt32()
        {
            this.FillBuffer(4);
            if(this.ReadBigEndian) { return this._binaryBuffer[3]|this._binaryBuffer[2] << 8|this._binaryBuffer[1] << 16|this._binaryBuffer[0] << 24; }
            return this._binaryBuffer[0]|this._binaryBuffer[1] << 8|this._binaryBuffer[2] << 16|this._binaryBuffer[3] << 24;
        }

        public override String ReadLine()
        {
            if(this._baseStreamRewinded) { throw new InvalidOperationException("ReadLine() is valid only WITHOUT rewinded base stream"); }
            var clField = typeof(StreamReader).GetField("charLen", BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance);
            var clValue = clField.GetValue(this);

            var cpField = typeof(StreamReader).GetField("charPos", BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance);
            var cpValue = cpField.GetValue(this);

            var cbField = typeof(StreamReader).GetField("charBuffer", BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance);
            var cbValue = cbField.GetValue(this);

            this._msgLen = (Int32) clValue;

            this._msgPos = (Int32) cpValue;

            if(this._msgBuffer == null || !this._msgBuffer.SequenceEqual((Char[]) cbValue))
            {
                this._msgBuffer = new Char[((Char[]) cbValue).Length];
                Array.Copy((Char[]) cbValue, this._msgBuffer, this._msgBuffer.Length);
            }

            var line = base.ReadLine();
            if(line != null) { this.UpdateReadBytes(line); }
            return line;
        }

        public override Task<String> ReadLineAsync() { throw new NotImplementedException(); }

        public sbyte ReadSByte()
        {
            this.FillBuffer(1);
            return (sbyte) this._binaryBuffer[0];
        }

        public string ReadToDelimiter(byte[] delimiter)
        {
            if(this._m2BytesPerChar) { throw new InvalidOperationException("ReadToDelimiter() is valid only on ASCII encoding"); }

            var delimiterLen = delimiter.Length;
            var buffer = new char[delimiterLen - 1];
            var delimiterPos = 0;
            var readedString = string.Empty;

            while(!this.EndOfPDU)
            {
                var readedChar = this.ReadChar();

                if(this._mCharBytes[0] == delimiter[delimiterPos])
                {
                    if(delimiterPos < delimiterLen - 1)
                    {
                        buffer[delimiterPos] = readedChar;
                        delimiterPos++;
                    }
                    else
                    {
                        return readedString;
                    }
                }
                else
                {
                    if(delimiterPos > 0)
                    {
                        for(var i = 0; i < delimiterPos; i++) { readedString += buffer[i]; }
                        delimiterPos = 0;
                    }
                    readedString += readedChar;
                }
            }
            return readedString;
        }

        public override String ReadToEnd()
        {
            if(this._baseStreamRewinded) { throw new InvalidOperationException("ReadToEnd() is valid only WITHOUT rewinded base stream"); }
            var sb = new StringBuilder();
            while(!this.EndOfStream)
            {
                var line = this.ReadLine();
                if(!this.EndOfStream) {
                    sb.Append(line + "\r\n");
                }
                else
                {
                    sb.Append(line);
                }
            }
            return sb.ToString();
        }

        public override Task<String> ReadToEndAsync() { throw new NotImplementedException(); }

        public ushort ReadUInt16()
        {
            this.FillBuffer(2);

            if(this.ReadBigEndian) { return (ushort) (this._binaryBuffer[1]|(uint) this._binaryBuffer[0] << 8); }
            return (ushort) (this._binaryBuffer[0]|(uint) this._binaryBuffer[1] << 8);
        }

        public uint ReadUInt32()
        {
            this.FillBuffer(4);
            if(this.ReadBigEndian) { return (uint) (this._binaryBuffer[3]|this._binaryBuffer[2] << 8|this._binaryBuffer[1] << 16|this._binaryBuffer[0] << 24); }
            return (uint) (this._binaryBuffer[0]|this._binaryBuffer[1] << 8|this._binaryBuffer[2] << 16|this._binaryBuffer[3] << 24);
        }

        public UInt64 ReadUInt64()
        {
            this.FillBuffer(8);
            if(this.ReadBigEndian)
            {
                return
                    (UInt64)
                    (this._binaryBuffer[7]|this._binaryBuffer[6] << 8|this._binaryBuffer[5] << 16|this._binaryBuffer[4] << 24|this._binaryBuffer[3] << 32
                     |this._binaryBuffer[2] << 40|this._binaryBuffer[1] << 48|this._binaryBuffer[0] << 56);
            }
            return
                (UInt64)
                (this._binaryBuffer[0]|this._binaryBuffer[1] << 8|this._binaryBuffer[2] << 16|this._binaryBuffer[3] << 24|this._binaryBuffer[4] << 32|this._binaryBuffer[5] << 40
                 |this._binaryBuffer[6] << 48|this._binaryBuffer[7] << 56);
        }

        private void FillBuffer(int numBytes)
        {
            if(this._binaryBuffer != null && (numBytes < 0 || numBytes > this._binaryBuffer.Length)) { throw new ArgumentOutOfRangeException(nameof(numBytes)); }
            var offset = 0;
            if(this.PDUStreamBasedProvider == null) { throw new ObjectDisposedException("The stream is closed."); }
            if(numBytes == 1)
            {
                var num = this.PDUStreamBasedProvider.ReadByte();
                if(num == -1) { throw new EndOfStreamException("The end of the stream is reached."); }
                this._binaryBuffer[0] = (byte) num;
                this._readBytes += 1;
            }
            else
            {
                do
                {
                    var num = this.PDUStreamBasedProvider.Read(this._binaryBuffer, offset, numBytes - offset);
                    if(num == 0) { throw new EndOfStreamException("The end of the stream is reached."); }
                    this._readBytes += num;
                    offset += num;
                } while(offset < numBytes);
            }
        }

        private int InternalReadOneChar()
        {
            var num1 = 0;
            var num3 = 0L;
            if(this.PDUStreamBasedProvider.CanSeek) { num3 = this.PDUStreamBasedProvider.Position; }
            if(this._mCharBytes == null) { this._mCharBytes = new byte[128]; }
            if(this._mSingleChar == null) { this._mSingleChar = new char[1]; }
            while(num1 == 0)
            {
                var byteCount = this._m2BytesPerChar? 2 : 1;
                var num4 = this.PDUStreamBasedProvider.ReadByte();
                this._mCharBytes[0] = (byte) num4;
                if(num4 == -1) { byteCount = 0; }
                if(byteCount == 2)
                {
                    var num5 = this.PDUStreamBasedProvider.ReadByte();
                    this._mCharBytes[1] = (byte) num5;
                    if(num5 == -1) { byteCount = 1; }
                }
                if(byteCount == 0) { return -1; }
                this._readBytes += byteCount;
                try {
                    num1 = this._mDecoder.GetChars(this._mCharBytes, 0, byteCount, this._mSingleChar, 0);
                }
                catch
                {
                    if(this.PDUStreamBasedProvider.CanSeek) { this.PDUStreamBasedProvider.Seek(num3 - this.PDUStreamBasedProvider.Position, SeekOrigin.Current); }
                    throw;
                }
            }
            if(num1 == 0) { return -1; }
            return this._mSingleChar[0];
        }

        private void UpdateReadBytes(String line)
        {
            // In readLine could be read in new buffer without us knowing it
            // we must check that, so we can calculate right value of bytes read

            var cpField = typeof(StreamReader).GetField("charPos", BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance);
            var cpValue = cpField.GetValue(this);

            var cbField = typeof(StreamReader).GetField("charBuffer", BindingFlags.NonPublic|BindingFlags.GetField|BindingFlags.Instance);
            var cbValue = cbField.GetValue(this);

            var len = 0;

            if(this._msgBuffer.SequenceEqual((Char[]) cbValue))
            {
                // still the same part of message
                len = (Int32) cpValue - this._msgPos;
            }
            else
            {
                // next part of message was read

                // len to end of the previous part
                len = this._msgLen - this._msgPos;
                // len of the read bytes from current part
                len += ((Int32) cpValue);
            }

            // this can happen when on the stream start one line is bigger than buffer length
            while(len < line.Length) { len += ((Char[]) cbValue).Length; }

            this._readBytes += len;
        }
    }
}