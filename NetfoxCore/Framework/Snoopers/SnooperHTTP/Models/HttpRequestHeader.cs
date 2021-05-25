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

namespace Netfox.Snoopers.SnooperHTTP.Models
{

    public enum HTTPRequestMethod
    {
        OPTIONS,
        GET,
        HEAD,
        POST,
        PUT,
        DELETE,
        TRACE,
        CONNECT
    }

    
    public class HttpRequestHeader : HTTPHeaderBase
    {
        public HTTPRequestMethod Method { get; private set; }

        public string RequestURI { get; private set; }

        internal HttpRequestHeader() : base() { } //EF

        public HttpRequestHeader(PDUStreamReader reader, string firstLine)
        {
            this.Type = MessageType.REQUEST;
            var reqLine = firstLine.Split(' ');
            HTTPRequestMethod parsedMethod;
            if(Enum.TryParse(reqLine[0], false, out parsedMethod)) this.Method = parsedMethod;
            this.RequestURI = reqLine[1];
            this.Version = reqLine[2];

            this.Parse(reader);
        }
        
        

        #region Overrides of HTTPHeaderBase
        public override string StatusLine => this.Method.ToString() + " " + this.RequestURI + " " + this.Version;
        #endregion
    }
}