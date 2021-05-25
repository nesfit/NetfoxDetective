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
using Netfox.Snoopers.SnooperHTTP.Models;
using Netfox.Snoopers.SnooperWebmails.Models.Spotters;

namespace Netfox.Snoopers.SnooperWebmails.Models.Analyzers
{
    /// <summary>
    /// Try to recognize most of webmails. It depends on named fields of email in HTTPMsg content.
    /// </summary>
    class GenericWebmailAnalyzer : WebmailAnalyzerBase
    {
        private static readonly GenericWebmailAnalyzer _instance = new GenericWebmailAnalyzer();

        private GenericWebmailAnalyzer() { }

        /// <summary>
        /// Returns instance of GenericWebmailAnalyzer.
        /// </summary>
        /// <returns></returns>
        public static GenericWebmailAnalyzer GetInstance(SpotterPool pool)
        {
            _instance.ContentSpotter = null;
            _instance.SpotterPool = pool;
            return _instance;
        }

        #region Overrides of WebmailAnalyzerBase

        public override bool IsMsgWebmailEvent(HTTPMsg message, ref List<EventSuggest> detectedEvents, out SpotterBase spotter)
        {
            this.Init(message);
            spotter = this.ContentSpotter;

            if(this.ContentSpotter == null) return false;

            /* webmail application detection for better content parsing purposes */
            if (this.ContentSpotter.ContainsKeyValuePair("appid", "YahooMailNeo", SpotterBase.SpotterContext.AllPair)) { this.WebmailApp = WebmailSuggest.YahooMailNeo; }
            else if (this.ContentSpotter.ContainsKeyValuePair("Host", "email.seznam.cz", SpotterBase.SpotterContext.AllPair)) { this.WebmailApp = WebmailSuggest.Seznam; }

            /* event detection */
            if(this.ContentSpotter.Contains("from", SpotterBase.SpotterContext.ContentKey) && this.ContentSpotter.Contains("to", SpotterBase.SpotterContext.ContentKey)
               && this.ContentSpotter.Contains("subject", SpotterBase.SpotterContext.ContentKey)
               && this.ContentSpotter.ContainsOneOf(this.MailContentFields, SpotterBase.SpotterContext.ContentKey)) {
                   detectedEvents.Add(EventSuggest.MailNewMessage);
               }
            else
            {

                foreach(var pair in this.ListFolderPairs)
                {
                    var key = pair.Key;
                    if((pair.Value.Equals("*") && this.ContentSpotter.Contains(pair.Key, SpotterBase.SpotterContext.AllKey))
                       || this.ContentSpotter.ContainsKeyValuePair(key, pair.Value, SpotterBase.SpotterContext.AllPair)) {
                           detectedEvents.Add(EventSuggest.MailListFolder);
                       }
                }

                foreach(var pair in this.DisplayMessagePairs)
                {
                    var key = pair.Key;
                    if((pair.Value.Equals("*") && this.ContentSpotter.Contains(pair.Key, SpotterBase.SpotterContext.AllKey))
                       || this.ContentSpotter.ContainsKeyValuePair(key, pair.Value, SpotterBase.SpotterContext.AllPair)) {
                           detectedEvents.Add(EventSuggest.MailDisplayMessage);
                       }
                }

                // other events detection comes here
            }
            return detectedEvents.Any();
        }
        #endregion

        
    }
}
