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
using Netfox.Core.Database.PersistableJsonSerializable;
using Netfox.Snoopers.SnooperHTTP.Models;

namespace Netfox.Snoopers.SnooperWebmails.Models.Spotters
{
    public class SpotterMultipart : SpotterBase
    {
        private HTTPMsg _clone;

        private string _content;

        private List<Int32> _boundariesIndices;

        private string _boundary;

        private PersistableJsonSerializableDictionaryStringListString _headerFields;

        private Dictionary<string, List<string>> _requestUriParameters;

        private string _requestUri;

        private int _partCount;

        private int _partPosition;

        private SpotterPool _spotterPool; 

        public SpotterMultipart(SpotterPool pool) { this._spotterPool = pool; }

        /// <summary>
        /// Checks if there is next part in multipart content.
        /// </summary>
        /// <returns>true if there isnext part, false otherwise</returns>
        public bool HasNextPart() { return this._partPosition/2 < this._partCount; }

        /// <summary>
        /// Return spotter for next part.
        /// </summary>
        /// <returns>SpotterBase instance.</returns>
        public SpotterBase GetNextPart()
        {
            var part = this._content.Substring(this._boundariesIndices[this._partPosition], this._boundariesIndices[this._partPosition + 1] - this._boundariesIndices[this._partPosition]);
            var contentType = this.GetStringPart("Content-Type: ", "\r\n", part);
            var spotter = this._spotterPool.GetSpotterOrWait(contentType);
            part = part.Substring(part.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
            this._clone.HTTPContent.Content = Encoding.ASCII.GetBytes(part);
            spotter.Init(this._clone);

            this._partPosition += 2;

            return spotter;

        }

        public void UnloadPart(SpotterBase spotter)
        {
            this._spotterPool.ReturningSpotter(spotter);
        }

        /// <summary>
        /// Resets part iteration.
        /// </summary>
        public void Reset() { this._partPosition = 0; }

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

            // Clone message is needed for later use of another soptters for every part
            this._clone = message.Clone();
            this._clone.HTTPContent = this._clone.HTTPContent.Clone();

            this._boundary = this.GetStringPart("boundary=", ";", message.HTTPHeader.Fields["Content-Type"].First());
            var encoder = this.GetEncoder(message.HTTPHeader.Fields["Content-Type"].First());
            this._content = encoder.GetString(message.HTTPContent.Content);
            this._boundariesIndices = new List<Int32>();

            // mark boundaries
            var start = this._content.IndexOf(this._boundary, StringComparison.Ordinal);
            start = this._content.IndexOf("\r\n", start, StringComparison.Ordinal) + 1;
            this._boundariesIndices.Add(start);
            do
            {
                var end = this._content.IndexOf(this._boundary, start, StringComparison.Ordinal);
                if (end == -1) break;
                end = this._content.LastIndexOf("\r\n", end, StringComparison.Ordinal);
                this._boundariesIndices.Add(end);

                this._partCount++;

                start = this._content.IndexOf(this._boundary, start, StringComparison.Ordinal);
                if (end == -1) break;
                start = this._content.IndexOf("\r\n", start, StringComparison.Ordinal) + 1;
                this._boundariesIndices.Add(start);
            } while(true);

        }
        public override bool IsSpottable() { return this._boundariesIndices.Count > 1; }

        public override bool Contains(string str, SpotterContext context)
        {

            this.Reset();
            while(this.HasNextPart())
            {
                var spotter = this.GetNextPart();
                if (spotter.Contains(str, context)) { this.UnloadPart(spotter); return true;}
                this.UnloadPart(spotter);
            }
            //for(var i = 0; i < this._boundariesIndices.Count-1; i += 2) {
            //    var part = this._content.Substring(this._boundariesIndices[i], this._boundariesIndices[i + 1] - this._boundariesIndices[i]);
            //    var contentType = this.GetStringPart("Content-Type: ", "\r\n", part);
            //    var spotter = SpotterFactory.GetSpotter(contentType);
            //    part = part.Substring(part.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
            //    this._clone.HTTPContent.Content = Encoding.ASCII.GetBytes(part);
            //    spotter.Init(this._clone);

                
            //}
            return false;
        }

        public override bool Contains(string[] strArr, SpotterContext context)
        {
            var cnt = strArr.Count(str => this.Contains(str, context));
            return cnt == strArr.Length;
        }

        public override bool ContainsOneOf(string[] strArr, SpotterContext context)
        { return strArr.Any(str => this.Contains(str, context)); }

        public override bool ContainsKeyValuePair(string key, string value, SpotterContext context) {

            this.Reset();
            while(this.HasNextPart())
            {
                var spotter = this.GetNextPart();
                if (spotter.ContainsKeyValuePair(key, value, context)) { this.UnloadPart(spotter); return true;}
                this.UnloadPart(spotter);
            }

            //for (var i = 0; i < this._boundariesIndices.Count-1; i += 2)
            //{
            //    var part = this._content.Substring(this._boundariesIndices[i], this._boundariesIndices[i + 1] - this._boundariesIndices[i]);
            //    var contentType = this.GetStringPart("Content-Type: ", "\r\n", part);
            //    var spotter = SpotterFactory.GetSpotter(contentType);
            //    part = part.Substring(part.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
            //    this._clone.HTTPContent.Content = Encoding.ASCII.GetBytes(part);
            //    spotter.Init(this._clone);

            //    if (spotter.ContainsKeyValuePair(key, value, context)) return true;
            //}
            return false;
        }
        public override object GetContent() { throw new NotImplementedException(); }

        public override string GetStringContent()
        {
            var sb = new StringBuilder();
            this.Reset();
            while(this.HasNextPart())
            {
                var spotter = this.GetNextPart();
                sb.Append(spotter.GetStringContent());
                this.UnloadPart(spotter);
            }

            //for(var i = 0; i < this._boundariesIndices.Count-1; i += 2)
            //{
            //    var part = this._content.Substring(this._boundariesIndices[i], this._boundariesIndices[i + 1] - this._boundariesIndices[i]);
            //    var contentType = this.GetStringPart("Content-Type: ", "\r\n", part);
            //    var spotter = SpotterFactory.GetSpotter(contentType);
            //    part = part.Substring(part.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4);
            //    this._clone.HTTPContent.Content = Encoding.ASCII.GetBytes(part);
            //    spotter.Init(this._clone);

            //    sb.Append(spotter.GetStringContent());
            //}

            return sb.ToString();
        }

        public override string GetContentPart(string partPattern, string expRetPattern)
        {
            var sb = new StringBuilder();

            this.Reset();
            while(this.HasNextPart())
            {
                var spotter = this.GetNextPart();
                sb.Append(spotter.GetContentPart(partPattern, expRetPattern));
                this.UnloadPart(spotter);
            }

            //for (var i = 0; i < this._boundariesIndices.Count-1; i += 2)
            //{
            //    var part = this._content.Substring(this._boundariesIndices[i], this._boundariesIndices[i + 1] - this._boundariesIndices[i]);
            //    var contentType = this.GetStringPart("Content-Type: ", "\r\n", part);
            //    var spotter = SpotterFactory.GetSpotter(contentType);
            //    this._clone.HTTPContent.Content = Encoding.ASCII.GetBytes(part);
            //    spotter.Init(this._clone);

            //    sb.Append(spotter.GetContentPart(partPattern, expRetPattern));
            //}

            return sb.ToString();
        }

        public override void Clean()
        {
            this._clone = null;
            this._content = string.Empty;
            this._boundariesIndices = null;
            this._boundary = null;
            this._headerFields = null;
            this._requestUri = string.Empty;
            this._requestUriParameters = null;
            this._partCount = 0;
            this._partPosition = 0;
        }
        #endregion
    }
}
