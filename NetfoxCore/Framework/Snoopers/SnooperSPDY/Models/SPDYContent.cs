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
using System.Text;
using System.Text.RegularExpressions;
using Netfox.Snoopers.SnooperSPDY.Models.Frames;

namespace Netfox.Snoopers.SnooperSPDY.Models
{
    [ComplexType]
    public class SPDYContent
    {
        public byte[] Content { get; private set; }
        public string ContentEncoding { get; private set; }
        public SPDYContent() { } //EF

        public SPDYContent(SPDYFrameData dataFrame, string contentEncoding = "")
        {
            this.ContentEncoding = contentEncoding;
            this.Content = this.ParseData(dataFrame.Data);
        }

        protected Byte[] ParseData(Byte[] data)
        {
            // Content is not encoded
            if(string.IsNullOrEmpty(this.ContentEncoding))
            {
                return data;
            }
            // Content is encoded using gzip
            else if(this.ContentEncoding == "gzip")
            {
                // Decompress content
                using (var gzipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                using (var outMemoryStream = new MemoryStream())
                {
                    gzipStream.CopyTo(outMemoryStream);
                    return outMemoryStream.ToArray();
                }
            }
            else
            {
                throw new Exception("Unknown SPDY data frame encoding: " + this.ContentEncoding);
            }
        }

        public Dictionary<string, string> GetFormUrlEncodedData()
        {
            return ParseFormUrlEncodedData(Encoding.ASCII.GetString(this.Content));
        }

        // Parse x-www-form-urlencoded data
        public static Dictionary<string, string> ParseFormUrlEncodedData(string data)
        {
            // http://codereview.stackexchange.com/questions/1588/get-params-from-a-url
            var matches = Regex.Matches(data, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value)
            );
        } 

        public override string ToString() { return this.Content == null? "" : Encoding.UTF8.GetString(this.Content); }
    }
}