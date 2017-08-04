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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Netfox.SnooperHTTP.Models;

namespace Netfox.SnooperMAFF.Models.Objects
{
    /// <summary>
    ///  Derived class from Text class desribes all parent objects of each archive.
    /// </summary>
    /// <seealso cref="TextObject" />
    [ComplexType]
    public class ParentObject : TextObject
    {
        private readonly string _sCharSet;
        private readonly string _sTitle;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override BaseObject Clone()
        {
            return new ParentObject(this.OriginalUrl, this.PathToFileName, this.FileName, this.FileExtension, this.ContentType, this.RequestMessage, this.CookieInformation, this._sCharSet);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentObject"/> class.
        /// </summary>
        /// <param name="sOrigUrl">The original URL.</param>
        /// <param name="sPathToFile">The path to file.</param>
        /// <param name="sFileName">Name of the file.</param>
        /// <param name="sFileExtension">The file extension.</param>
        /// <param name="sContentType">Type of the content.</param>
        /// <param name="oRequestMsg">The request message.</param>
        /// <param name="listCookieInformation">The list cookie information.</param>
        /// <param name="sCharSet">The character set.</param>
        public ParentObject(
            string sOrigUrl,
            string sPathToFile,
            string sFileName,
            string sFileExtension,
            string sContentType,
            HTTPMsg oRequestMsg,
            List<string> listCookieInformation,
            string sCharSet) : base(sOrigUrl, sPathToFile, sFileName, sFileExtension, sContentType, oRequestMsg, listCookieInformation)
        {
            this._sCharSet = sCharSet;

            var regMatch = Regex.Match(this.StringContent, @"<title>(.*)</title>");
            if (regMatch.Success)
            {
                this._sTitle = regMatch.Groups[1].ToString();
            }
        }

        /// <summary>
        /// Gets the character set of parent object (for whole archive).
        /// </summary>
        /// <returns>Return object character set</returns>
        public string GetCharSet() { return this._sCharSet; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <returns>Return web page title</returns>
        public string GetTitle() { return this._sTitle; }

        /// <summary>
        /// Gets the references amount of parent objects.
        /// </summary>
        /// <returns>Return number of references ammount</returns>
        public int GetReferencesAmount() { return this.ListOfReferences.Count; }

        /// <summary>
        /// Processes and replace the references obtained from list of references by special new referenes with index_files folder.
        /// </summary>
        public override void ProcessReferences()
        {
            foreach (var oReference in this.ListOfReferences)
            {
                //Replace References in String Content
                this.Replace(oReference.Item1, "index_files/" + oReference.Item2);

                //Fill NewReferenceList For Export
                this.ListOfNewReferences.Add("index_files/" + oReference.Item2);
            }
        }
    }
}
