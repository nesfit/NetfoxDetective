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
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;

namespace Netfox.SnooperHTTP.Models
{
    
    public class HttpResponseHeader : HTTPHeaderBase
    {
        public string StatusCode { get; private set; }

        public string ReasonPhrase { get; private set; }

        internal HttpResponseHeader() : base() { } //EF

        public HttpResponseHeader(PDUStreamReader reader, string firstLine)
        {
            this.Type = MessageType.RESPONSE;
            var statLine = firstLine.Split(' ');
            var phrase = new string[statLine.Length - 2];
            Array.Copy(statLine,2,phrase,0,phrase.Length);
            this.Version = statLine[0];
            this.StatusCode = statLine[1];
            this.ReasonPhrase = string.Join(" ", phrase);

            this.Parse(reader);
        }
        
        #region Overrides of HTTPHeaderBase
        public override string StatusLine => this.Version + " " + this.StatusCode + " " + this.ReasonPhrase;
        #endregion
    }
}