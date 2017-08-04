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
using Netfox.Core.Database.PersistableJsonSeializable;
using Netfox.SnooperSPDY.Models.Frames;

namespace Netfox.SnooperSPDY.Models
{
    public enum RequestMethod
    {
        HEAD,
        GET,
        POST,
        PUT,
        DELETE,
        TRACE,
        OPTIONS,
        CONNECT
    };

    public class SPDYRequestHeader : SPDYHeaderBase
    {
        public RequestMethod Method { get; private set; }
        public string Scheme { get; private set; }

        public string URI { get; private set; }
        public string Path { get; private set; }
        //public Dictionary<string, string> URIParameters { get; private set; }
        public virtual PersistableJsonSerializableDictionaryStringString URIParameters { get; private set; } = new PersistableJsonSerializableDictionaryStringString();

        public SPDYRequestHeader() { } //EF

        public SPDYRequestHeader(SPDYFrameSynStream synStreamFrame) : base(synStreamFrame as SPDYStreamFrame)
        {
            this.Method = this.ParseRequestMethod(synStreamFrame.Fields[":method"]);
            this.Scheme = synStreamFrame.Fields[":scheme"];

            this.URI = synStreamFrame.Fields[":path"];
            this.Path = this.URI.Split('?')[0];
            this.URIParameters = new PersistableJsonSerializableDictionaryStringString(SPDYContent.ParseFormUrlEncodedData(this.URI));
        }

        protected RequestMethod ParseRequestMethod(string method)
        {
            switch (method) {
                case "HEAD":
                    return RequestMethod.HEAD;
                case "GET":
                    return RequestMethod.GET;
                case "POST":
                    return RequestMethod.POST;
                case "PUT":
                    return RequestMethod.PUT;
                case "DELETE":
                    return RequestMethod.DELETE;
                case "TRACE":
                    return RequestMethod.TRACE;
                case "OPTIONS":
                    return RequestMethod.OPTIONS;
                case "CONNECT":
                    return RequestMethod.CONNECT;
                default:
                    throw new Exception("Unknown SPDY method: " + method);
            }
        }

        public override string ToString() { return this.Method + " " + this.URI; }
    }
}