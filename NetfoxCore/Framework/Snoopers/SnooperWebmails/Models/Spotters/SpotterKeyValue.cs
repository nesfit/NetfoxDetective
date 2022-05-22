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
using System.Text;
using System.Text.RegularExpressions;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Snoopers.SnooperHTTP.Models;

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    public class SpotterKeyValue : SpotterBase
    {
        private PersistableJsonSerializableDictionaryStringListString _headerFields;

        //private string[] _content;

        private Dictionary<string, List<string>> _contentFields;

        private Dictionary<string, List<string>> _requestUriParameters;

        private string _requestUri;

        #region Overrides of SpotterBase
        public override object Accept(ISpotterVisitor visitor) { return visitor.applyOn(this); }

        public override void Init(HTTPMsg message)
        {
            this._headerFields = message.HTTPHeader.Fields;
            if(message.MessageType == MessageType.REQUEST)
            {
                var reqHeader = message.HTTPHeader as HttpRequestHeader;
                var uri = reqHeader.RequestURI.Split('?');
                this._requestUri = uri[0];
                this._requestUriParameters = new Dictionary<string, List<string>>();
                if (uri.Length == 2)
                    foreach(var p in uri[1].Split('&').Select(pair => pair.Split('='))) {
                        if (!this._requestUriParameters.ContainsKey(p[0])) this._requestUriParameters[p[0]] = new List<string>();
                        if (p.Length == 2) this._requestUriParameters[p[0]].Add(p[1]);
                    }
            }

            if(message.HTTPContent == null) return;

            this._contentFields = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            var encoder = this.GetEncoder(message.HTTPHeader.Fields["Content-Type"].First());

            foreach (var pair in encoder.GetString(message.HTTPContent.Content).Split('&'))
            {
                var p = pair.Split('=');
                if(p.Length < 2) continue;

                if(!this._contentFields.ContainsKey(p[0]))
                {
                    this._contentFields.Add(p[0],new List<string>());
                }
                this._contentFields[p[0]].Add(p[1]);
            }
        }

        public override bool IsSpottable()
        {
            return (this._contentFields != null && this._contentFields.Any()) || (this._headerFields != null && this._headerFields.Any()) || (string.IsNullOrEmpty(this._requestUri));
        }

        private bool ContentContains(string str, SpotterContext context) {

            if (this._contentFields == null) return false;

            if (context.HasFlag(SpotterContext.ContentKey))
                if (this._contentFields.ContainsKey(str)) return true;

            if (context.HasFlag(SpotterContext.ContentValue))
                return this._contentFields.Keys.Any(key => this._contentFields[key].Contains(str));

            return false;
        }

        public override bool Contains(string str, SpotterContext context)
        {
            if(context.HasFlag(SpotterContext.ContentKey)
                || context.HasFlag(SpotterContext.ContentValue)) return this.ContentContains(str,context);

            if(context.HasFlag(SpotterContext.AllKey))
            {
                if(this._requestUriParameters.ContainsKey(str)) { return true;}
                if(this._headerFields.ContainsKey(str)) { return true;}
                if(this._contentFields == null) { return false; }
                if(this._contentFields.ContainsKey(str)) { return true;}
            }

            if(context.HasFlag(SpotterContext.AllValue))
            {
                if(this._headerFields.Keys.Any(key => this._headerFields[key].Contains(str))) { return true; }
                if(this._requestUriParameters != null && this._requestUriParameters.Keys.Any(key => this._requestUriParameters[key].Contains(str))) { return true; }

                if(this._contentFields == null) { return false; }
                if (this._contentFields.Keys.Any(key => this._contentFields[key].Contains(str))) { return true; }
            }

            return this._requestUri.Contains(str);
        }

        public override bool Contains(string[] strArr, SpotterContext context)
        {
            var cnt = strArr.Count(key => this.Contains(key, context));

            return cnt == strArr.Length;
        }

        public override bool ContainsOneOf(string[] strArr, SpotterContext context)
        { return strArr.Any(key => this.Contains(key, context)); }

        public override bool ContainsKeyValuePair(string key, string value, SpotterContext context)
        {
            if(context.HasFlag(SpotterContext.AllPair))
            {
                if(this._headerFields.ContainsKey(key) && this._headerFields[key].Contains(value)) return true;
                if(this._requestUriParameters != null && this._requestUriParameters.ContainsKey(key) && this._requestUriParameters[key].Contains(value)) return true;
            }
            if(context.HasFlag(SpotterContext.ContentPair) || context.HasFlag(SpotterContext.AllPair))
            {
                if(this._contentFields == null) return false;
                if(this._contentFields.ContainsKey(key) && this._contentFields[key].Contains(value)) return true;
            }
            return false;
        }

        public override object GetContent() { return this._contentFields; }

        public override string GetStringContent()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var key in this._contentFields.Keys)
            {
                sb.Append(key);
                sb.Append(" = [");
                for(int i = 0; i < this._contentFields[key].Count; i++)
                {
                    sb.Append(this._contentFields[key][i]);
                    if(i < this._contentFields[key].Count - 1) sb.Append(", ");
                }
                sb.Append("]\n");
            }
            return sb.ToString();

        }

        public override string GetContentPart(string partPattern, string expRetPattern)
        {
            var pattern = new Regex(partPattern);
            // When key is match return whatever the vaue is
            //var expect = new Regex(expRetPattern);

            var sb = new StringBuilder();

            foreach (var key in this._contentFields.Keys)
            {
                if(pattern.IsMatch(key))
                {
                    foreach(var value in this._contentFields[key])
                    {
                        sb.Append(value);
                        sb.Append("; ");
                    }
                }
            }

            return sb.ToString();
        }

        public override void Clean() {

            this._headerFields = null;
            this._requestUri = string.Empty;
            this._requestUriParameters = null;
            this._contentFields = null;

        }
        #endregion
    }
}
