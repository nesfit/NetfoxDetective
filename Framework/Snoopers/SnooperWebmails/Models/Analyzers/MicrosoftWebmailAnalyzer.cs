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
    /// Recognize only microsoft live webmail app. 
    /// </summary>
    class MicrosoftWebmailAnalyzer : WebmailAnalyzerBase
    {
        private static readonly MicrosoftWebmailAnalyzer _instance = new MicrosoftWebmailAnalyzer();

        private MicrosoftWebmailAnalyzer() { }

        /// <summary>
        /// Returns instance of GenericWebmailAnalyzer.
        /// </summary>
        /// <returns></returns>
        public static MicrosoftWebmailAnalyzer GetInstance(SpotterPool pool)
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

            if (this.ContentSpotter == null) return false;
            if(this.ContentSpotter.ContainsKeyValuePair("cn", "Microsoft.Msn.Hotmail.Ui.Fpp.MailBox", SpotterBase.SpotterContext.ContentPair))
            {
                if (this.ContentSpotter.ContainsKeyValuePair("mn","SendMessage_ec",SpotterBase.SpotterContext.ContentPair)) detectedEvents.Add(EventSuggest.MailNewMessage);
                if (this.ContentSpotter.ContainsKeyValuePair("mn","GetInboxData",SpotterBase.SpotterContext.ContentPair)
                    && !detectedEvents.Contains(EventSuggest.MailNewMessage)) detectedEvents.Add(EventSuggest.MailListFolder);
                // TODO other events
            }
            return detectedEvents.Any();
        }
        #endregion
    }
}
