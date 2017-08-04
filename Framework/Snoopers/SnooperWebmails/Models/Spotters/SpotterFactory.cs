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

namespace Netfox.SnooperWebmails.Models.Spotters
{
    /// <summary>
    /// Factory for conetnt spotters. Return Spotter based on given Content-Type. 
    /// If no match for spotter is found, returns text/string based Spotter.
    /// </summary>
    public class SpotterFactory
    {
        private SpotterFactory() { }

        public static List<string> SupportedContent = new List<string>()
        {
            "application/x-www-form-urlencoded",
            "application/json",
            "multipart/form-data",
            "application/x-base64-frpc",
            "text/*"
        };

        public static SpotterPool Pool = new SpotterPool(20);

        /// <summary>
        /// Factory for conetnt spotters. Return Spotter based on given Content-Type. 
        /// </summary>
        /// <param name="contentType">Content-Type value from HTTPMsg header</param>
        /// <returns></returns>
        public static SpotterBase GetSpotter(string contentType)
        {
            if (contentType.StartsWith("application/x-www-form-urlencoded")) { return new SpotterKeyValue(); }
            else if(contentType.StartsWith("application/json")) { return new SpotterJson(); }
            else if(contentType.StartsWith("multipart/form-data")) { return new SpotterMultipart(Pool); }
            else if(contentType.StartsWith("application/x-base64-frpc")) { return new SpotterFRPC(); }
            return new SpotterText();
        }

        public static Type GetSpotterType(string contentType)
        {
            if (contentType.StartsWith("application/x-www-form-urlencoded")) { return typeof(SpotterKeyValue); }
            else if (contentType.StartsWith("application/json")) { return typeof(SpotterJson); }
            else if (contentType.StartsWith("multipart/form-data")) { return typeof(SpotterMultipart); }
            else if (contentType.StartsWith("application/x-base64-frpc")) { return typeof(SpotterFRPC); }
            return typeof(SpotterText);
        }

    }
}
