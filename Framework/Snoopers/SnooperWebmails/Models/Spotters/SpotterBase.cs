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
using System.Text;
using Netfox.SnooperHTTP.Models;

namespace Netfox.SnooperWebmails.Models.Spotters
{
    /// <summary>
    /// This is base class for content spotters. All new spotters must extends this class.
    /// The purpose of content spotters is to watch over some type of content and tell if it has
    /// some wanted parts. It can also try to return some specified parts of the content.
    /// Every spotter should be implemented as singleton.
    /// </summary>
    public abstract class SpotterBase
    {
        /// <summary>
        /// Tells spotter what should consider in its operations.
        /// Every spotter can spot more parts of HTTP message.
        /// </summary>
        [Flags]
        public enum SpotterContext
        {
            ContentKey = 1,
            ContentValue = 2,
            ContentPair = 4,
            AllKey = 8,
            AllValue = 16,
            AllPair = 32,
        }

        /// <summary>
        /// This is helper method that gets part of a string bounded by two another strings.
        /// </summary>
        /// <param name="from">Left boundary.</param>
        /// <param name="to">Right boundary.</param>
        /// <param name="field">Part that I want get subprat from.</param>
        /// <returns>wanted string part</returns>
        protected string GetStringPart(string from, string to,string field)
        {
            var start = field.IndexOf(from, StringComparison.Ordinal);
            start = start == -1? 0 : start;
            var len = field.IndexOf(to, start, StringComparison.Ordinal);
            len = len == -1 ? field.Length : len;
            len = len - start - from.Length;
            return field.Substring(start + from.Length, len);
        }

        /// <summary>
        /// Helper method that returns encoder based on charset in content type in HTTP header.
        /// </summary>
        /// <param name="contentType">HTTP header value of Content-Type field</param>
        /// <returns>encoding instance</returns>
        protected Encoding GetEncoder(string contentType)
        {
            var coding = this.GetStringPart("charset=", ";",contentType);

            var encoder = Encoding.ASCII;

            if (coding.Equals("UTF-8",StringComparison.OrdinalIgnoreCase))
                    encoder = Encoding.UTF8;

            return encoder;
        }

        /// <summary>
        /// Every spotter have to define this method the same way. 
        /// Purpose of this method is to extend functionality of content spotter so that it can do some specific work.
        /// This specific work is implemented by Spotter Visitors.
        /// </summary>
        /// <param name="visitor">SpotterVisitor that implements some speccific job that should be done.</param>
        /// <returns>object - whatever spotter visitor returns</returns>
        public abstract object Accept(ISpotterVisitor visitor);

        /// <summary>
        /// Initialize spotter with HTTP message.
        /// </summary>
        /// <param name="message">HTTP message which content should be watch.</param>
        public abstract void Init(HTTPMsg message);

        /// <summary>
        /// Return true if there is something to spot.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsSpottable();

        /// <summary>
        /// Checks if there is specified string in spottable content, specified by SpotterContext flag.
        /// </summary>
        /// <param name="str">String that should be look for.</param>
        /// <param name="context">Context that should be look in.</param>
        /// <returns>true if string was found, false otherwise</returns>
        public abstract bool Contains(string str, SpotterContext context);

        /// <summary>
        /// Checks if there are all strings in specified array.
        /// </summary>
        /// <param name="strArr">strings that should be look for</param>
        /// <param name="context">context that should be look in</param>
        /// <returns>true if all strings were found, false otherwise</returns>
        public abstract bool Contains(string[] strArr, SpotterContext context);

        /// <summary>
        /// Checks if there is at least one of the specified strings.
        /// </summary>
        /// <param name="strArr">strings that should be look for</param>
        /// <param name="context">context htat should be look in</param>
        /// <returns>true if at least one of the string was found, false otherwise</returns>
        public abstract bool ContainsOneOf(string[] strArr, SpotterContext context);

        /// <summary>
        /// Checks if content contains specified key/value pair. 
        /// If content can't be separated on keys and values and no other part of HTTP message is under spoting context,
        /// then this method should return false.
        /// </summary>
        /// <param name="key">key string that should be look for</param>
        /// <param name="value">value string that should be look for as pair of specified key</param>
        /// <param name="context">context that should be look in</param>
        /// <returns>true if specified key/value pair was found</returns>
        public abstract bool ContainsKeyValuePair(string key, string value, SpotterContext context);

        /// <summary>
        /// Returns spotting content in some kind of object. the best choice should be Dictionary or List of KeyValuePair-s.
        /// </summary>
        /// <returns></returns>
        public abstract object GetContent();

        /// <summary>
        /// Gets content transformed to string.
        /// </summary>
        /// <returns>string content</returns>
        public abstract string GetStringContent();

        /// <summary>
        /// Tryies to get some part of content based on regular expressions.
        /// </summary>
        /// <param name="partPattern">This regex patern specifies part of the content that should be key if it was key value spotter.</param>
        /// <param name="expRetPattern">This regex specifies value part. This is what user of this method expects to be returned.</param>
        /// <returns>returns string part of content that matches expRetPattern and is after partPattern in content</returns>
        public abstract string GetContentPart(string partPattern, string expRetPattern);

        public abstract void Clean();
    }
}
