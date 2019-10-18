// Copyright (c) 2017 Jan Pluskal
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

using System.IO;
using System.Text;

namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary> A line reader.</summary>
    public class LineReader : BinaryReader
    {
        /// <summary> The encoding.</summary>
        private Encoding _encoding;
        /// <summary> The decoder.</summary>
        private Decoder _decoder;

        /// <summary> Size of the buffer.</summary>
        const int BufferSize = 1024;
        /// <summary> Buffer for line data.</summary>
        private byte[] _lineBuffer = new byte[BufferSize];

        /// <summary> Constructor.</summary>
        /// <param name="stream">     The stream. </param>
        /// <param name="bufferSize"> Size of the buffer. </param>
        /// <param name="encoding">   The encoding. </param>
        public LineReader(Stream stream, int bufferSize, Encoding encoding)
            : base(stream, encoding)
        {
            this._encoding = encoding;
            this._decoder = encoding.GetDecoder();
        }

        /// <summary> Reads a line.</summary>
        /// <param name="encoding"> The encoding. </param>
        /// <returns> The line.</returns>
        public string ReadLine(Encoding encoding)
        {
            var pos = 0;

            StringBuilder stringBuilder = null;
            var lineEndFound = false;
            var endOfFileFound = false;

            do
            {

                if (this.PeekChar() < 0)
                    endOfFileFound = true;
                else
                {
                    int c = this.ReadByte();
                    if (c == '\r')
                    {
                        var cc = this.PeekChar();
                        if (cc == '\n')
                            continue;
                    }
                    else if (c == '\n')
                    {
                        lineEndFound = true;
                    }
                    else
                    {
                        this._lineBuffer[pos++] = (byte)c;
                        if (pos >= BufferSize)
                        {
                            if (stringBuilder == null)
                                stringBuilder = new StringBuilder();

                            var linePart = encoding.GetString(this._lineBuffer, 0, pos);

                            stringBuilder.Append(linePart);
                            pos = 0;
                        }
                    }
                }

            } while (!lineEndFound && !endOfFileFound);

            if (endOfFileFound)
                return null;

            if (stringBuilder != null)
            {
                if (pos > 0)
                {
                    var linePart = encoding.GetString(this._lineBuffer, 0, pos);
                    stringBuilder.Append(linePart);
                }

                var line = stringBuilder.ToString();
                return line;
            }
            else
            {
                if (pos > 0)
                {
                    var line = encoding.GetString(this._lineBuffer, 0, pos);
                    return line;
                }
                else
                    return string.Empty;
            }

            /*
                        while (base.Read(buf, 0, 2) > 0)
                        {
                            if (buf[1] == '\r')
                            {
                                // grab buf[0]
                                this._LineBuffer[pos++] = buf[0];
                                // get the '\n'
                                char ch = base.ReadChar();
                                Debug.Assert(ch == '\n');

                                lineEndFound = true;
                            }
                            else if (buf[0] == '\r')
                            {
                                lineEndFound = true;
                            }
                            else
                            {
                                this._LineBuffer[pos] = buf[0];
                                this._LineBuffer[pos + 1] = buf[1];
                                pos += 2;

                                if (pos >= bufferSize)
                                {
                                    stringBuffer = new StringBuilder(bufferSize + 80);
                                    stringBuffer.Append(this._LineBuffer, 0, bufferSize);
                                    pos = 0;
                                }
                            }

                            if (lineEndFound)
                            {
                                if (stringBuffer == null)
                                {
                                    if (pos > 0)
                                        return new string(this._LineBuffer, 0, pos);
                                    else
                                        return string.Empty;
                                }
                                else
                                {
                                    if (pos > 0)
                                        stringBuffer.Append(this._LineBuffer, 0, pos);
                                    return stringBuffer.ToString();
                                }
                            }
                        }

                        if (stringBuffer != null)
                        {
                            if (pos > 0)
                                stringBuffer.Append(this._LineBuffer, 0, pos);
                            return stringBuffer.ToString();
                        }
                        else
                        {
                            if (pos > 0)
                                return new string(this._LineBuffer, 0, pos);
                            else
                                return null;
                        }*/
        }

    }
}