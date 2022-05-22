// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka
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
using System.Linq;
using Netfox.Snoopers.SnooperHTTP.Models;

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    public class SpotterFRPC : SpotterBase
    {
        /// <summary>
        /// List of FRPC items this content spotter operates on.
        /// </summary>
        public List<FRPCparser.IFRPCItem> Items;
        
        private IDictionary<string, List<string>> _headerFields;

        //private string[] _content;

        private Dictionary<string, List<string>> _requestUriParameters;

        private string _requestUri;

        private bool ItemContainsPair(FRPCparser.IFRPCItem item, string key, string value, SpotterContext context)
        {
            switch (item.Type)
            {
                case FRPCparser.ItemType.MethodCall:
                    var methodCall = item as FRPCparser.FRPCMethodCall;
                    if (methodCall == null) return false;

                    return (methodCall.Name.Contains(key) && methodCall.Parameters.Any(param => this.ItemContains(param, value, context))) 
                        || methodCall.Parameters.Any(param => this.ItemContainsPair(param, key, value, context));
                case FRPCparser.ItemType.Array:
                    var array = item as FRPCparser.FRPCArray;
                    return array != null && array.Items.Any(x=> this.ItemContainsPair(x, key, value, context));
                case FRPCparser.ItemType.Struct:
                    var fStruct = item as FRPCparser.FRPCStruct;
                    if (fStruct != null) {
                        return fStruct.Items.Any(x => x.Key.Equals(key) && this.ItemContains(x.Value, value, context))
                            || fStruct.Items.Any(x => this.ItemContainsPair(x.Value, key, value, context)); }
                    break;
            }

            return false;
        }

        private bool ItemContains(FRPCparser.IFRPCItem item, string str, SpotterContext context)
        {
            switch(item.Type) {
                case FRPCparser.ItemType.MethodCall:
                    var methodCall = item as FRPCparser.FRPCMethodCall;
                    if(methodCall == null) return false;

                    return ((context.HasFlag(SpotterContext.AllKey) || context.HasFlag(SpotterContext.ContentKey)) && methodCall.Name.Contains(str)) 
                        || methodCall.Parameters.Any(param => this.ItemContains(param, str, context));
                case FRPCparser.ItemType.MethodReponse:
                    var methodResoponse = item as FRPCparser.FRPCMethodRespone;
                    return methodResoponse != null && methodResoponse.Data.Any(data => this.ItemContains(data, str, context));
                case FRPCparser.ItemType.Array:
                    var array = item as FRPCparser.FRPCArray;
                    return array != null && array.Items.Any(x => this.ItemContains(x, str, context));
                case FRPCparser.ItemType.Struct:
                    var fStruct = item as FRPCparser.FRPCStruct;

                    if((context.HasFlag(SpotterContext.AllKey)
                        || context.HasFlag(SpotterContext.ContentKey))
                        && fStruct != null 
                        && fStruct.Items.Any(x => x.Key.Equals(str)))
                    {
                        return true;
                    }

                    return fStruct != null && fStruct.Items.Any(x => this.ItemContains(x.Value, str, context));
                case FRPCparser.ItemType.String:
                    var stri = item as FRPCparser.FRPCString;
                    return (stri != null && stri.Value.Equals(str));

            }

            return false;
        }

        #region Overrides of SpotterBase
        public override object Accept(ISpotterVisitor visitor) { return visitor.applyOn(this); }

        public override void Init(HTTPMsg message)
        {
            this._headerFields = message.HTTPHeader.Fields;
            if (message.MessageType == MessageType.REQUEST)
            {
                var reqHeader = message.HTTPHeader as HttpRequestHeader;
                var uri = reqHeader.RequestURI.Split('?');
                this._requestUri = uri[0];
                this._requestUriParameters = new Dictionary<string, List<string>>();
                if (uri.Length == 2)
                    foreach (var p in uri[1].Split('&').Select(pair => pair.Split('=')))
                    {
                        if (!this._requestUriParameters.ContainsKey(p[0])) this._requestUriParameters[p[0]] = new List<string>();
                        if (p.Length == 2) this._requestUriParameters[p[0]].Add(p[1]);
                    }
            }

            if (message.HTTPContent == null) return;

            var decodedBytes = Convert.FromBase64String(message.HTTPContent.ToString());

            this.Items = new List<FRPCparser.IFRPCItem>();

            if (!FRPCparser.Parse(decodedBytes, out this.Items)) { return; }
        }
        public override bool IsSpottable() { return this.Items.Count > 0; }
        public override bool Contains(string str, SpotterContext context)
        {

            if (context.HasFlag(SpotterContext.AllKey))
            {
                if (this._requestUriParameters.ContainsKey(str)) { return true; }
                if (this._headerFields.ContainsKey(str)) { return true; }
            }

            if (context.HasFlag(SpotterContext.AllValue))
            {
                if (this._headerFields.Keys.Any(key => this._headerFields[key].Contains(str))) { return true; }
                if (this._requestUriParameters != null && this._requestUriParameters.Keys.Any(key => this._requestUriParameters[key].Contains(str))) { return true; }
            }

            return this.Items.Any(item => this.ItemContains(item, str, context));
        }

        public override bool Contains(string[] strArr, SpotterContext context)
        {
            var cnt = strArr.Count(key => this.Contains(key, context));

            return cnt == strArr.Length;

        }

        public override bool ContainsOneOf(string[] strArr, SpotterContext context)
        {
            return strArr.Any(key => this.Contains(key, context));
        }

        public override bool ContainsKeyValuePair(string key, string value, SpotterContext context)
        {
            if (context.HasFlag(SpotterContext.AllPair))
            {
                if (this._headerFields.ContainsKey(key) && this._headerFields[key].Contains(value)) return true;
                if (this._requestUriParameters != null && this._requestUriParameters.ContainsKey(key) && (this._requestUriParameters[key].Contains(value) || value.Equals("*"))) return true;
            }

            return this.Items.Any(item => this.ItemContainsPair(item, key, value, context));
        }
        public override object GetContent() { return this.Items.ToString(); }
        public override string GetStringContent() { return this.Items.ToString(); }
        public override string GetContentPart(string partPattern, string expRetPattern) { return ""; }
        public override void Clean()
        {
            this.Items = null;
            this._headerFields = null;
            this._requestUri = string.Empty;
            this._requestUriParameters = null;

        }
        #endregion
    }
}
