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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using Netfox.Core.Database;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Snoopers.SnooperHTTP.Models
{
    
    public class HTTPMsg:IEntity
    {
        private DirectoryInfo _exportDirectory;

        private HTTPContent _HTTPContent;
        private HTTPHeaderBase _httpHeader;
        private HttpRequestHeader _httpRequestHeader = new HttpRequestHeader();
        private HttpResponseHeader _httpResponseHeader = new HttpResponseHeader();
        private PersistableJsonSerializableGuid _framesGuids;

        protected HTTPMsg() { }

        public HTTPMsg(PDUStreamReader reader, DirectoryInfo exportBaseDirectory)
        {
            this._exportDirectory = exportBaseDirectory;
            this.Valid = true;
            this.Parse(reader);
        }
        [NotMapped]
        public virtual List<IExportSource> ExportSources { get; set; } = new List<IExportSource>();

        [NotMapped]
        public HTTPHeaderBase HTTPHeader
        {
            get
            {
                if (this._httpHeader != null) return this._httpHeader;
                if (this.HttpRequestHeader.IsPresent) return (this._httpHeader = this.HttpRequestHeader);
                if (this.HttpResponseHeader.IsPresent) return (this._httpHeader = this.HttpResponseHeader);
                return this._httpHeader;
            }
            protected set
            {
                if (this._httpHeader == value) return;
                this._httpHeader = value;
                if(value is HttpRequestHeader) this.HttpRequestHeader = (HttpRequestHeader) value;
                if(value is HttpResponseHeader) this.HttpResponseHeader = (HttpResponseHeader) value;

            }
        }
        [DataMember(IsRequired = false)]
        public virtual HttpRequestHeader HttpRequestHeader
        {
            get { return this._httpRequestHeader; }
            set
            {
                if(this._httpRequestHeader == value) return;
                this._httpRequestHeader = value;
                if(value.IsPresent)
                    this.HTTPHeader = value;
            }
        }
        [DataMember(IsRequired = false)]
        public virtual HttpResponseHeader HttpResponseHeader
        {
            get { return this._httpResponseHeader; }
            set
            {
                if (this._httpResponseHeader == value) return;
                this._httpResponseHeader = value;
                if (value.IsPresent)
                    this.HTTPHeader = value;
            }
        }

        [DataMember(IsRequired = false)]
        public virtual HTTPContent HTTPContent
        {
            get
            {
                if(this._HTTPContent != null) { return this._HTTPContent; }

                if(!this.HTTPHeader.Fields.ContainsKey("Content-Type") || !this.HTTPHeader.Fields.ContainsKey("Content-Length")
                   || this.HTTPHeader.Fields["Content-Length"].Contains("0")) {
                       return new HTTPContent();
                   }

                var prepare = this.TimeStamp.Millisecond + this.HTTPHeader.StatusLine + this.HTTPHeader.Fields["Content-Type"].First();

                this._HTTPContent = new HTTPContent
                {
                    Content = File.ReadAllBytes(this._exportDirectory + "\\" + GetMD5Hash(prepare))
                };
                return this._HTTPContent ?? (this._HTTPContent = new HTTPContent());
            }
            set { this._HTTPContent = value; }
        }

        [NotMapped]
        public List<PmFrameBase> Frames { get; set; } = new List<PmFrameBase>();

        public PersistableJsonSerializableGuid FrameGuids
        {
            get { return this._framesGuids ?? new PersistableJsonSerializableGuid(this.Frames.Select(f => f.Id)); }
            set { this._framesGuids = value; }
        }

        [Column(TypeName = "datetime2")]
        public DateTime TimeStamp { get; set; }

        public bool Valid { get; private set; }

        public string InvalidReason { get; private set; }

        //[ForeignKey(nameof(PairMessage))]
        //public Guid? PairMessageId { get; set; }
        [NotMapped]//todo fix
        public virtual List<HTTPMsg> PairMessages { get; set; } = new List<HTTPMsg>();
        [NotMapped]
        public MessageType MessageType => this.HTTPHeader.Type;

        public HTTPMsg Clone()
        {
            var newMsg = new HTTPMsg();
            newMsg.HTTPHeader = this.HTTPHeader;
            newMsg.HTTPContent = this.HTTPContent;
            newMsg.PairMessages.AddRange(this.PairMessages);

            return newMsg;
        }

        public static string GetMD5Hash(string str)
        {
            var bytes = Encoding.Unicode.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            var hashed = md5.ComputeHash(bytes);

            var sb = new StringBuilder();
            for(var i = 0; i < hashed.Length; i++) { sb.Append(hashed[i].ToString("x2")); }

            return sb.ToString();
        }

        private void Parse(PDUStreamReader reader)
        {
            var stream = reader.BaseStream as PDUStreamBasedProvider;

            //char[] buffer = new char[1024];

            //_reader.Read(buffer, 0, 1024);
            try
            {
                var line = reader.ReadLine();

                if (stream.GetCurrentPDU() != null)
                {
                    this.ExportSources.Add(stream.GetCurrentPDU());
                    this.TimeStamp = stream.GetCurrentPDU().FirstSeen;
                    this.Frames.AddRange(stream.GetCurrentPDU().FrameList);
                }
                else this.ExportSources.Add(stream.Conversation);


                if (line == null)
                {
                    this.Valid = false;
                    this.InvalidReason = "Nothing to read.";
                    return;
                }
                if(line.StartsWith("HTTP"))
                {
                    // HTTP Response
                    this.HTTPHeader = new HttpResponseHeader(reader, line);
                }
                else if(line.StartsWith("OPTIONS") || line.StartsWith("GET") || line.StartsWith("HEAD") || line.StartsWith("POST") || line.StartsWith("PUT")
                        || line.StartsWith("DELETE") || line.StartsWith("TRACE") || line.StartsWith("CONNECT"))
                {
                    // HTTP Request
                    this.HTTPHeader = new HttpRequestHeader(reader, line);
                }
                else
                {
                    this.Valid = false;
                    this.InvalidReason = "Not a HTTP message.";
                    return;
                }

                // Check for Message content
                if(this.HTTPHeader.Fields.ContainsKey("Transfer-Encoding") && this.HTTPHeader.Fields["Transfer-Encoding"].Contains("chunked")) {
                    this.HTTPContent = new HTTPContent(reader, HTTPContent.TransferCoding.CHUNKED);
                }
                else if(this.HTTPHeader.Fields.ContainsKey("Content-Length") && !this.HTTPHeader.Fields["Content-Length"].Contains("0")) {
                    this.HTTPContent = new HTTPContent(reader, HTTPContent.TransferCoding.IDENTITY, this.HTTPHeader.Fields["Content-Length"][0]);
                }
                else
                {
                    this.HTTPContent = new HTTPContent();
                }

                // If encoded try to decode
                if(this.HTTPHeader.Fields.ContainsKey("Content-Encoding"))
                {
                    var encoding = this.HTTPHeader.Fields["Content-Encoding"].Last();
                    var gzippedBytes = this.HTTPContent.Content;
                    var si = new MemoryStream(gzippedBytes);
                    var so = new MemoryStream();
                    var db = new byte[512];
                    int i;

                    switch(encoding)
                    {
                        case "gzip":
                            var gzs = new GZipInputStream(si);
                            while((i = gzs.Read(db, 0, db.Length)) != 0) { so.Write(db, 0, i); }
                            this.HTTPContent.Content = so.ToArray();
                            break;
                        case "deflate":
                            var ds = new DeflateStream(si, CompressionMode.Decompress);
                            while((i = ds.Read(db, 0, db.Length)) != 0) { so.Write(db, 0, i); }
                            this.HTTPContent.Content = so.ToArray();
                            break;
                        default:
                            break;
                    }
                }

                if(this.HTTPContent?.Content != null ) { this.SaveContent(); }
            }
            catch(Exception ex)
            {
                this.Valid = false;
                this.InvalidReason = $"Message parsing failed. {ex}";
            }
        }

        private void SaveContent()
        {
            var prepare = this.TimeStamp.Ticks + string.Join(" ", this.Frames) + this.HTTPHeader.StatusLine;
            var bw = new BinaryWriter(new FileStream(this._exportDirectory + "\\" + GetMD5Hash(prepare), FileMode.Create));
            bw.Write(this.HTTPContent.Content);
            bw.Close();
        }

        #region Implementation of IEntity
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; }
        #endregion
    }
}