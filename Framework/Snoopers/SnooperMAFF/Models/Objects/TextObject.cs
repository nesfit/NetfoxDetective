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
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Netfox.SnooperHTTP.Models;

namespace Netfox.SnooperMAFF.Models.Objects
{
    /// <summary>
    /// Derived class from BaseObject desribes all objects in text form. 
    /// </summary>
    /// <seealso cref="BaseObject" />
    [ComplexType]
    public class TextObject : BaseObject
    {
        protected string StringContent = "";
        protected List<Tuple<string, string>> ListOfReferences;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override BaseObject Clone()
        {
            return new TextObject(this.OriginalUrl, this.PathToFileName, this.FileName, this.FileExtension, this.ContentType, this.RequestMessage, this.CookieInformation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextObject"/> class.
        /// </summary>
        /// <param name="sOrigUrl">The original URL.</param>
        /// <param name="sPathToFile">The path to file.</param>
        /// <param name="sFileName">Name of the file.</param>
        /// <param name="sFileExtension">The file extension.</param>
        /// <param name="sContentType">Type of the content.</param>
        /// <param name="oRequestMsg">The request MSG.</param>
        /// <param name="listCookieInformation">The list cookie information.</param>
        public TextObject(
            string sOrigUrl,
            string sPathToFile,
            string sFileName,
            string sFileExtension,
            string sContentType,
             HTTPMsg oRequestMsg,
            List<string> listCookieInformation) : base(sOrigUrl, sPathToFile, sFileName, sFileExtension, sContentType, oRequestMsg, listCookieInformation)
        {
            this.ListOfReferences = new List<Tuple<string, string>>();
            this.ConvertByteArrayToString();
        }

        /// <summary>
        /// Checks if the reference link is in object content
        /// </summary>
        /// <param name="sLink">The link to check.</param>
        /// <returns>Return true if link was founded</returns>
        public virtual bool CheckLink(string sLink) { return (this.StringContent.Contains(sLink)); }

        /// <summary>
        /// Adds the founded link to objects list of references.
        /// </summary>
        /// <param name="sPathToFilename">The path to filename.</param>
        /// <param name="sFilename">The filename.</param>
        public void AddLink(string sPathToFilename, string sFilename)
        {
            if (!this.StringContent.Contains(sPathToFilename)) { return; }
            this.ListOfReferences.Add(new Tuple<string, string>(sPathToFilename, sFilename));
        }

        /// <summary>
        /// Processes and replace the references obtained from list of references.
        /// </summary>
        public virtual void ProcessReferences()
        {
            foreach (var oReference in this.ListOfReferences)
            {
                //Replace References in String Content
                this.Replace(oReference.Item1, oReference.Item2);

                //Fill NewReferenceList For Export
                this.ListOfNewReferences.Add(oReference.Item2);
            }
        }

        /// <summary>
        /// Gets the byte content of object.
        /// </summary>
        /// <returns>Return byte content of object</returns>
        public override byte[] GetContent()
        {
            this.ConvertStringToByteArray();
            return this.Content;
        }

        /// <summary>
        /// Rewrites the current byte content.
        /// </summary>
        /// <param name="byteNewContent">New content of the byte.</param>
        public override void RewriteContent(byte[] byteNewContent)
        {
            this.Content = byteNewContent;
            this.ConvertByteArrayToString();
        }

        /// <summary>
        /// Converts the byte array content to string content.
        /// </summary>
        private void ConvertByteArrayToString()
        {
            this.StringContent = this.Content == null ? "" : Encoding.GetEncoding(437).GetString(this.Content);
        }
        /// <summary>
        /// Converts the string content to byte array content.
        /// </summary>
        private void ConvertStringToByteArray()
        {
            this.Content = Encoding.GetEncoding(437).GetBytes(this.StringContent);
            this.FileSize = this.Content.LongLength;
        }

        /// <summary>
        /// Replaces the specified old string sequence by new string sequences in string content.
        /// </summary>
        /// <param name="sOldStringSequence">The s old string sequence.</param>
        /// <param name="sNewStringSequence">The s new string sequence.</param>
        protected void Replace(string sOldStringSequence, string sNewStringSequence)
        {
            for (var pos = 0; pos < this.StringContent.Length; pos++)
            {
                pos = this.StringContent.IndexOf(sOldStringSequence, pos, StringComparison.Ordinal);
                if (pos == -1) { return; }

                var lBeginSubstringIndex = 0;
                var lEndSubstringIndex = 0;
                for (var lIndex = pos; lIndex > 0; lIndex--)
                {
                    if ((this.StringContent[lIndex] == '\'') || (this.StringContent[lIndex] == '\"') ||
                        (this.StringContent[lIndex] == ' ') || (this.StringContent[lIndex] == '('))
                    {
                        lBeginSubstringIndex = lIndex + 1;
                        break;
                    }
                }

                for (var lIndex = pos + sOldStringSequence.Length; lIndex < this.StringContent.Length; lIndex++)
                {
                    if ((this.StringContent[lIndex] == '\'') || (this.StringContent[lIndex] == '\"') ||
                        (this.StringContent[lIndex] == ' ') || (this.StringContent[lIndex] == ')'))
                    {
                        lEndSubstringIndex = lIndex;
                        break;
                    }
                }
                if ((lEndSubstringIndex - lBeginSubstringIndex) > 0)
                {
                    sOldStringSequence = this.StringContent.Substring(lBeginSubstringIndex, lEndSubstringIndex - lBeginSubstringIndex);
                    this.StringContent = this.StringContent.Replace(sOldStringSequence, sNewStringSequence);
                }
            }
        }
    }
}
