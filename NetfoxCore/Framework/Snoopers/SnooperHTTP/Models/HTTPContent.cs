// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Vit Janecek
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;

namespace Netfox.Snoopers.SnooperHTTP.Models
{
    [ComplexType]
    public class HTTPContent
    {
        public enum TransferCoding
        {
            CHUNKED,
            IDENTITY
        }

        public HTTPContent() { }

        public HTTPContent(PDUStreamReader reader, TransferCoding coding, string contentLen = "0")
        {
            var sb = new StringBuilder();
            switch(coding)
            {
                case TransferCoding.CHUNKED:

                    ParseChunk(reader, sb);
                    break;

                default:
                    var readed = 0;
                    var dLen = Convert.ToInt32(contentLen);
                    var dt = new char[dLen];
                    while(true)
                    {
                        var len = reader.Read(dt, readed, dLen - readed);
                        readed += len;
                        if(readed == dLen) { break; }
                        if(len == 0) { break; }
                    }
                    sb.Append(dt);
                    break;
            }

            this.Content = Encoding.ASCII.GetBytes(sb.ToString());
        }

        public byte[] Content { get; set; }

        public HTTPContent Clone()
        {
            var newContent = new HTTPContent
            {
                Content = this.Content
            };
            return newContent;
        }

        public override String ToString() { return this.Content == null? "" : Encoding.ASCII.GetString(this.Content); }

        private static void ParseChunk(PDUStreamReader reader, StringBuilder sb)
        {
            var chunkStart = true;
            var chunkSize = 0;
            while(true)
            {
                if(chunkStart)
                {
                    var line = reader.ReadLine();
                    if(line == null)
                    {
                        return;
                    }
                    // TODO check if correct chunk coding or else unread
                    chunkSize = Convert.ToInt32(line, 16);
                    chunkStart = false;
                }
                else
                {
                    var data = new char[chunkSize];
                    var read = 0;
                    do
                    {
                        var len = reader.Read(data, read, data.Length - read);
                        if(len == 0)
                        {
                            return;
                        }
                        read += len;
                    } while(read < chunkSize);


                    sb.Append(data);
                    // chunk boundary
                    if(reader.ReadLine() == null)
                    {
                        return;
                    }

                    chunkStart = true;
                }

                if(chunkSize == 0)
                {
                    /*
                    // read last line - chunk boundary
                    if(reader.ReadLine() == null)
                    {
                        reader.NewMessage();
                        reader.ReadLine(); // TODO unread line if not empty string
                    }
                    break;
                    */
                    reader.ReadLine();
                    return;
                }
            }
        }
    }
}