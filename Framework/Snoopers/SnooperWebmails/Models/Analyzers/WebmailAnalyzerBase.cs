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
using Netfox.SnooperHTTP.Models;
using Netfox.SnooperWebmails.Models.Spotters;

namespace Netfox.SnooperWebmails.Models.Analyzers
{
    /// <summary>
    /// Base class of webmail analyzers. There will be one generic webmail analyzer that should be able to 
    /// recognize much of the webmail applications. But there might be webmail applications that does not
    /// behave as expected. In that purpose this permits to implement some non-generic analyzers.
    /// In order to reduce number of instances per conversation, every analyzer should be implemented as singleton.
    /// </summary>
    abstract class WebmailAnalyzerBase
    {
        /// <summary>
        /// This is enumeration of possible webmail events that may carry some usefull informations.
        /// </summary>
        public enum EventSuggest
        {
            MailNewMessage,
            MailListFolder,
            MailDisplayMessage
        }

        /// <summary>
        /// Enumeration of some webmail apps that can be recognized for better parsing purpose.
        /// </summary>
        public enum WebmailSuggest
        {
            YahooMailNeo,
            Gmail,
            Centrum,
            Seznam,
            MicrosoftLive,
            Unknown
        }

        /// <summary>
        /// Possible names of fields that determines content of mail message.
        /// </summary>
        protected readonly string[] MailContentFields = new string[] {
            "body",
            "content",
            "simpleBody"
        };

        /// <summary>
        /// Method keys and names that determines listing folder
        /// </summary>
        protected readonly List<KeyValuePair<string, string>> ListFolderPairs = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string,string>("search", "*" ), /* gmail */
            new KeyValuePair<string,string>( "m", "list" ), /* centrum/atlas */
            new KeyValuePair<string,string>("method", "ListMessagesInThread"), /* yahoo */
            new KeyValuePair<string,string>("method", "ListFolderThreads"),
            new KeyValuePair<string,string>("user.listMessages", "*") /* seznam */

        };

        /// <summary>
        /// Method keys and names that determines displaying message
        /// </summary>
        protected readonly List<KeyValuePair<string,string>>  DisplayMessagePairs = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string,string>("method", "GetDisplayMessage"),
            new KeyValuePair<string,string>("user.message.getAttributes", "*")
        };

        protected readonly string[] ListFolderMethods = new string[]
        {
            "search",
            "list",
            "folders",
            "ListMessagesInThread"

        };

        public SpotterPool SpotterPool { get; set; }

        /// <summary>
        /// Webmail application detected.
        /// </summary>
        public WebmailSuggest WebmailApp { get; protected set; }

        /// <summary>
        /// Content spotter for this analyzer.
        /// </summary>
        public SpotterBase ContentSpotter { get; protected set; }

        /// <summary>
        /// Initialization with HTTP message.
        /// </summary>
        /// <param name="message">HTTPMsg message</param>
        public void Init(HTTPMsg message)
        {
            if (message.HTTPHeader.Fields.ContainsKey("Content-Type"))
            {
                this.ContentSpotter = this.SpotterPool.GetSpotterOrWait(message.HTTPHeader.Fields["Content-Type"].First());
                this.ContentSpotter.Init(message);
            }
            this.WebmailApp = WebmailSuggest.Unknown;
        }

        /// <summary>
        /// Check if HTTP message is webmail event.
        /// </summary>
        /// <param name="message">HTTP message to check</param>
        /// <param name="detectedEvents">Webmail events found in message</param>
        /// <param name="spotter">Spotter used to handle message.</param>
        /// <returns></returns>
        public abstract bool IsMsgWebmailEvent(HTTPMsg message, ref List<EventSuggest> detectedEvents, out SpotterBase spotter);
    }
}
