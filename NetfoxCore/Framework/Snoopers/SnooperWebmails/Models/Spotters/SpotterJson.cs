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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Snoopers.SnooperHTTP.Models;

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    public class SpotterJson : SpotterBase
    {

        private PersistableJsonSerializableDictionaryStringListString _headerFields;

        private Dictionary<string, List<string>> _requestUriParameters;

        private string _requestUri;

        private XElement _root;

        private List<string> Flatten(XElement element)
        {
            var list = new List<string>();
            foreach(var child in element.Elements())
            {
                var sb = new StringBuilder();
                sb.Append(child.Name);
                if(child.HasAttributes)
                {
                    sb.Append("(");
                    foreach(var attr in child.Attributes()) { sb.Append(attr.Name + ":" + attr.Value + ","); }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(")");
                }
                if(!child.IsEmpty)
                {
                    sb.Append("=");
                    sb.Append(child.Value);
                }
                list.Add(sb.ToString());

                if (element.HasElements) list.AddRange(this.Flatten(child));
            }

            return list;
        }

        /// <summary>
        /// Returns root element of JSON content.
        /// </summary>
        /// <returns></returns>
        public XElement GetRoot() { return this._root; }

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
            
            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(message.HTTPContent.Content, new XmlDictionaryReaderQuotas());

            this._root = XElement.Load(jsonReader);


        }

        public override bool IsSpottable() { return this._root != null; }

        private bool ContentContains(string str, SpotterContext context)
        {
            if (this._root == null) return false;

            if (context.HasFlag(SpotterContext.ContentKey) || context.HasFlag(SpotterContext.AllKey))
                if (this._root.XPathSelectElement("//" + str) != null) return true;

            if (context.HasFlag(SpotterContext.ContentValue) || context.HasFlag(SpotterContext.AllValue))
                return this._root.XPathSelectElements("//*").Any(element => element.Value.Equals(str));

            return false;
        }

        public override bool Contains(string str, SpotterContext context)
        {
            if (context.HasFlag(SpotterContext.ContentKey)
                || context.HasFlag(SpotterContext.ContentValue))
                return this.ContentContains(str, context);

            if (context.HasFlag(SpotterContext.AllKey))
            {
                if (this._requestUriParameters?.ContainsKey(str) ?? false) { return true; }
                if (this._headerFields.ContainsKey(str)) { return true; }
                if (this._root == null) { return false; }
                if (this._root.XPathSelectElement("//" + str) != null) return true;
            }

            if (context.HasFlag(SpotterContext.AllValue))
            {
                if (this._headerFields.Keys.Any(key => this._headerFields[key].Contains(str))) { return true; }
                if (this._requestUriParameters != null && this._requestUriParameters.Keys.Any(key => this._requestUriParameters[key].Contains(str))) { return true; }

                if (this._root == null) { return false; }
                if(this._root.XPathSelectElements("//*").Any(element => element.Value.Equals(str))) { return true; }
            }

            return this._requestUri?.Contains(str) ?? false;
        }

        public override bool Contains(string[] strArr, SpotterContext context)
        {
            var cnt = strArr.Count(str => this.Contains(str, context));
            return cnt == strArr.Length;
        }

        public override bool ContainsOneOf(string[] strArr, SpotterContext context)
        { return strArr.Any(str => this.Contains(str, context)); }

        public override bool ContainsKeyValuePair(string key, string value, SpotterContext context)
        {
            if(context.HasFlag(SpotterContext.AllPair))
            {
                if (this._headerFields.ContainsKey(key) && this._headerFields[key].Contains(value)) return true;
                if (this._requestUriParameters != null && this._requestUriParameters.ContainsKey(key) && this._requestUriParameters[key].Contains(value)) return true;
            }

            if (context.HasFlag(SpotterContext.ContentPair) || context.HasFlag(SpotterContext.AllPair)) {
                return this._root != null && this._root.XPathSelectElements("//"+key).Any(element => element.Value.Equals(value));
            }
            return false;
        }

        public override object GetContent() { return this._root; }

        //public Dictionary<string, List<string>> GetDictionaryContent()
        //{
        //    var dict = new Dictionary<string, List<string>>();
        //    foreach(var el in this.Flatten(this._root))
        //    {
        //        var pair = el.Split('=');
        //        if(!dict.ContainsKey(pair[0])) dict[pair[0]] = new List<string>();
        //        if (pair.Length == 2) dict[pair[0]].Add(pair[1]);
        //    }
        //    return dict;
        //}

        public override string GetStringContent() { return string.Join("; ",this.Flatten(this._root)); }

        public override string GetContentPart(string partPattern, string expRetPattern)
        {
            var pattern = new Regex(partPattern);
            var expect = new Regex(expRetPattern);

            var sb = new StringBuilder();

            foreach(var element in this._root.XPathSelectElements("//*"))
            {
                if(pattern.IsMatch(element.Name.LocalName))
                {
                    // Match in name - get value
                    if(expect.IsMatch(element.Value)) {
                        sb.Append(expect.Match(element.Value));
                        sb.Append("; ");
                    }
                    else if(element.HasElements)
                    {
                        // check for inner elements if value was not match
                        foreach(var child in element.Descendants().Where(child => expect.IsMatch(child.Value)))
                        {
                            sb.Append(expect.Match(child.Value));
                            sb.Append("; ");
                        }
                    }
                    
                }
                else
                {
                    // check its attributes
                    foreach(var attr in element.Attributes().Where(attr => pattern.IsMatch(attr.Name.LocalName))) {
                        // attribute match
                        sb.Append(expect.Match(attr.Value));
                        sb.Append("; ");
                    }
                }
            }

            return sb.ToString();

        }

        public override void Clean()
        {

            this._root = null;
            this._headerFields = null;
            this._requestUri = string.Empty;
            this._requestUriParameters = null;
        }
        #endregion
    }
}
