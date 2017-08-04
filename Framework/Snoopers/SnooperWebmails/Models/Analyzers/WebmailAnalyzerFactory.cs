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

using System.Linq;
using System.Net;
using Netfox.SnooperWebmails.Models.Spotters;

namespace Netfox.SnooperWebmails.Models.Analyzers
{
    /// <summary>
    /// Factory for Webmail Analyzers. Returns analyzer based on some information from HTTPMsg that indicates
    /// webmail apps that have specific analyzers.
    /// </summary>
    class WebmailAnalyzerFactory
    {
        private WebmailAnalyzerFactory() { }

        /// <summary>
        /// Returns best analyzer for given HTTPMsg.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static WebmailAnalyzerBase GetAnalyzer(Netfox.SnooperHTTP.Models.HttpRequestHeader request)
        {
            if((request.Fields.ContainsKey("Host") && request.Fields["Host"].First().Contains("mail.live.com"))
                || request.RequestURI.Contains("/ol/mail.fpp"))
            {
                return MicrosoftWebmailAnalyzer.GetInstance(SpotterFactory.Pool);
            }
            else
            {
                return GenericWebmailAnalyzer.GetInstance(SpotterFactory.Pool);
            }
        }
    }
}
