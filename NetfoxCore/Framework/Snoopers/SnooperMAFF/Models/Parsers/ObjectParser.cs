// Copyright (c) 2017 Jan Pluskal, Vit Janecek
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
using System.Text;
using Netfox.Snoopers.SnooperHTTP.Models;
using Netfox.Snoopers.SnooperMAFF.Models.Objects;

namespace Netfox.Snoopers.SnooperMAFF.Models.Parsers
{
    /// <summary>
    /// Static class for parsing and creating MAFF objects
    /// </summary>
    internal static class ObjectParser
    {
        private static long _generatedObjectIndex = -1;
        /// <summary>
        /// Creates the MAFF object method.
        /// </summary>
        /// <param name="oResponseHeader">The message response header.</param>
        /// <param name="oRequestMsg">The request message.</param>
        /// <returns></returns>
        public static BaseObject CreateDataObject(HttpResponseHeader oResponseHeader, HTTPMsg oRequestMsg)
        {
            var oRequestHeader =    ((HttpRequestHeader)oRequestMsg.HTTPHeader);
            var sOriginalUrl =      oRequestHeader.RequestURI;

            var sContentType =      ObjectParser.ParseContentType(oResponseHeader);
            var sCharSet =          ObjectParser.ParseCharSet(oResponseHeader);
            var sPathToFileName =   ObjectParser.ParsePathToFileName(sOriginalUrl);
            var sFileName =         ObjectParser.ParseFileName(sPathToFileName);
            var listCookiees =      ObjectParser.ParseCookieInformation(oResponseHeader);

            var bGeneratedObject = false;
            var sFileExtension = string.Empty;

            if (sFileName.Length != 0)
            {
                sFileExtension = ObjectParser.ParseFileExtension(sFileName);

                //If it's Text object without extension, fill with GET uri request
                if (ObjectParser.CheckIfTextObject(sContentType) && sFileExtension == string.Empty)
                {
                    sPathToFileName = sOriginalUrl;
                }
            }

            //Parse GetUrlRequest for Javasrcipt If isnt Root html object
            else if (!sOriginalUrl.Equals("/"))
            {
                _generatedObjectIndex++;
                bGeneratedObject = true;
            }

            if (sContentType.Contains("text/html") && !bGeneratedObject)
            {
                if (ObjectParser.CheckIsValidParentObject(oRequestMsg.PairMessages[0].HTTPContent.Content))
                {
                    return new ParentObject(sOriginalUrl, sPathToFileName, "index.html", "html", sContentType, oRequestMsg, listCookiees, sCharSet);
                }
                return new TextObject(sOriginalUrl, sPathToFileName, sFileName, sFileExtension, sContentType, oRequestMsg, listCookiees);
            }

            if (bGeneratedObject)
            {
                //TODO IMPOSSIBLE TO PARSE??? FOR NOW -> Discard packet 
                return ObjectParser.CheckIfScriptObject(sContentType) ?
                    new GeneratedObject(sOriginalUrl, sPathToFileName, sFileName, sFileExtension, sContentType, oRequestMsg, listCookiees, _generatedObjectIndex) : null;
            }

            if (ObjectParser.CheckIfScriptObject(sContentType))
            {
                return new ScriptObject(sOriginalUrl, sPathToFileName, sFileName, sFileExtension, sContentType, oRequestMsg, listCookiees);
            }

            if (ObjectParser.CheckIfTextObject(sContentType))
            {
                return new TextObject(sOriginalUrl, sPathToFileName, sFileName, sFileExtension, sContentType, oRequestMsg, listCookiees);
            }

            return new BaseObject(sOriginalUrl, sPathToFileName, sFileName, sFileExtension, sContentType, oRequestMsg, listCookiees);
        }

        //Parsers for Text section
        //////////////////////////////////
       
        /// <summary>
        /// Parses the host address.
        /// </summary>
        /// <param name="oRequestHeader">The message request header.</param>
        /// <returns>Return host adress</returns>

        public static string ParseHostAddress(HttpRequestHeader oRequestHeader)
        {
            if (oRequestHeader.Fields.ContainsKey("Host") && oRequestHeader.Fields["Host"].Count > 0)
            {
                return oRequestHeader.Fields["Host"][0];
            }
            return string.Empty;
        }

        /// <summary>
        /// Parses the referrer.
        /// </summary>
        /// <param name="oRequestHeader">The message request header.</param>
        /// <returns>Return parsed referrer</returns>
        public static string ParseReferer(HttpRequestHeader oRequestHeader)
        {
            if (oRequestHeader.Fields.ContainsKey("Referer") && oRequestHeader.Fields["Referer"].Count > 0)
            {
                var sReferer = oRequestHeader.Fields["Referer"][0];
                var iReplacingIndex = sReferer.IndexOf("http://", StringComparison.Ordinal);

                if (iReplacingIndex >= 0)
                {
                    iReplacingIndex += "http://".Length;
                    return sReferer.Substring(iReplacingIndex, sReferer.Length - iReplacingIndex);
                }

                iReplacingIndex = sReferer.IndexOf("https://", StringComparison.Ordinal);

                if (iReplacingIndex >= 0)
                {
                    iReplacingIndex += "https://".Length;
                    return sReferer.Substring(iReplacingIndex, sReferer.Length - iReplacingIndex);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Checks if text object.
        /// </summary>
        /// <param name="sContentType">Type of the s content.</param>
        /// <returns>Return true if object is text type</returns>
        private static bool CheckIfTextObject(string sContentType)
        {
            return sContentType.Contains("text/") || sContentType.Contains("application/x-pointplus") || CheckIfScriptObject(sContentType);
        }

        /// <summary>
        /// Checks if script object.
        /// </summary>
        /// <param name="sContentType">Type of the s content.</param>
        /// <returns>Return true if object is script type </returns>
        private static bool CheckIfScriptObject(string sContentType)
        {
            return sContentType.Contains("text/javascript") ||
                   sContentType.Contains("text/ecmascript") ||
                   sContentType.Contains("application/x-javascript") ||
                   sContentType.Contains("application/ecmascript") ||
                   sContentType.Contains("application/javascript");
        }

        /// <summary>
        /// Parses the type of the HTTP content.
        /// </summary>
        /// <param name="oResponseHeader">The message response header.</param>
        /// <returns>Return type of object</returns>
        private static string ParseContentType(HttpResponseHeader oResponseHeader)
        {
            if (oResponseHeader.Fields.ContainsKey("Content-Type") && oResponseHeader.Fields["Content-Type"].Count > 0)
            {
                var iReplacingIndex = oResponseHeader.Fields["Content-Type"][0].IndexOf(";", StringComparison.Ordinal);
                return iReplacingIndex > 0 ? oResponseHeader.Fields["Content-Type"][0].Substring(0, iReplacingIndex) : oResponseHeader.Fields["Content-Type"][0];
            }
            return string.Empty;
        }

        /// <summary>
        /// Parses the character set of text object.
        /// </summary>
        /// <param name="oResponseHeader">The message response header.</param>
        /// <returns>Return text object character set</returns>
        private static string ParseCharSet(HttpResponseHeader oResponseHeader)
        {
            if (oResponseHeader.Fields.ContainsKey("Content-Type") && oResponseHeader.Fields["Content-Type"].Count > 0)
            {
                var sCharSet = oResponseHeader.Fields["Content-Type"][0];
                var iReplacingIndex = sCharSet.IndexOf(";", StringComparison.Ordinal);

                if (iReplacingIndex > 0)
                {
                    iReplacingIndex = sCharSet.IndexOf("charset=", StringComparison.Ordinal) + "charset=".Length;
                    return sCharSet.Substring(iReplacingIndex, sCharSet.Length - iReplacingIndex);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Parses the cookie information.
        /// </summary>
        /// <param name="oResponseHeader">The response header.</param>
        /// <returns>Return list of Cookies</returns>
        private static List<string> ParseCookieInformation(HttpResponseHeader oResponseHeader)
        {
            if (oResponseHeader.Fields.ContainsKey("Set-Cookie") && oResponseHeader.Fields["Set-Cookie"].Count > 0)
            {
                return oResponseHeader.Fields["Set-Cookie"];
            }
            return new List<string>();
        }

        /// <summary>
        /// Parses the name of the path to file.
        /// </summary>
        /// <param name="sOriginalUrl">The string contained original URL.</param>
        /// <returns>Return path to file</returns>
        private static string ParsePathToFileName(string sOriginalUrl)
        {
            var iReplacingIndex = sOriginalUrl.IndexOf("?", StringComparison.Ordinal);
            return iReplacingIndex > 0 ? sOriginalUrl.Substring(0, iReplacingIndex) : sOriginalUrl;
        }

        /// <summary>
        /// Parses the name of the file.
        /// </summary>
        /// <param name="sPathToFileName">String contained the path to file.</param>
        /// <returns>Return name of file</returns>
        private static string ParseFileName(string sPathToFileName)
        {
            var iReplacingIndex = sPathToFileName.LastIndexOf("/", StringComparison.Ordinal);
            if (iReplacingIndex == -1) { iReplacingIndex = 0; }
            else { iReplacingIndex += "/".Length; }
            return sPathToFileName.Substring(iReplacingIndex, sPathToFileName.Length - iReplacingIndex);
        }

        /// <summary>
        /// Parses the file extension.
        /// </summary>
        /// <param name="sFileName">Name of the file.</param>
        /// <returns>Return file extension</returns>
        private static string ParseFileExtension(string sFileName)
        {
            var iReplacingIndex = sFileName.LastIndexOf(".", StringComparison.Ordinal);
            return iReplacingIndex > 0 ? sFileName.Substring(iReplacingIndex + ".".Length) : string.Empty;
        }

        /// <summary>
        /// Checks the is valid parent object.
        /// </summary>
        /// <param name="byteContent">Object content in byte form.</param>
        /// <returns></returns>
        private static bool CheckIsValidParentObject(byte[] byteContent)
        {
            var sHelperString = Encoding.GetEncoding(437).GetString(byteContent);

            return sHelperString.Contains("<meta") && sHelperString.Contains("<div") && sHelperString.Contains("<head") && sHelperString.Contains("<body");
        }
    }
}
