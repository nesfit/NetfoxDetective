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
using System.Text;
using System.Text.RegularExpressions;
using Netfox.SnooperHTTP.Models;

namespace Netfox.SnooperWebmails.Models.Spotters
{
    public class SpotterText : SpotterBase
    {

        private string _content;       

        #region Overrides of SpotterBase
        public override object Accept(ISpotterVisitor visitor) { return visitor.applyOn(this); }

        public override void Init(HTTPMsg message) {
            if (message.HTTPContent == null) return;

            var encoder = this.GetEncoder(message.HTTPHeader.Fields["Content-Type"].First());

            this._content = encoder.GetString(message.HTTPContent.Content);

        }

        public override bool IsSpottable() { return !string.IsNullOrEmpty(this._content); }

        public override bool Contains(string str, SpotterContext context) { return this._content?.Contains(str) ?? false; }

        public override bool Contains(string[] strArr, SpotterContext context)
        {
            var i = strArr.Count(str => this.Contains(str, context));

            return i == strArr.Length;
        }

        public override bool ContainsOneOf(string[] strArr, SpotterContext context)
        { return strArr.Any(str => this.Contains(str, context)); }

        public override bool ContainsKeyValuePair(string key, string value, SpotterContext context) { return false; }

        public override object GetContent()
        {
            var dict = new Dictionary<string, List<string>>
            {
                ["all"] = new List<string>
                {
                    this._content
                }
            };
            return dict;
        }
        public override string GetStringContent() { return this._content; }

        public override string GetContentPart(string partPattern, string expRetPattern)
        {
            var pattern = new Regex(partPattern);
            var expect = new Regex(expRetPattern);

            var sb = new StringBuilder();

            var founds = pattern.Matches(this._content);
            for(int i = 0; i < founds.Count; i++)
            {
                var begin = founds[i].Index;
                var len = founds.Count > i + 1? founds[i + 1].Index - begin : this._content.Length - begin;
                sb.Append(expect.Match(this._content, begin, len));
                sb.Append("; ");
            }

            return sb.ToString();

        }

        public override void Clean() {
            this._content = string.Empty;
        }
        #endregion
    }
}
