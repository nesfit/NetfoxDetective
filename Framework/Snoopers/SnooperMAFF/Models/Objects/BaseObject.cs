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
using System.IO;
using System.IO.Compression;
using System.Text;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.SnooperHTTP.Models;
using Netfox.SnooperMAFF.Models.Common;
using Netfox.SnooperMAFF.Models.Parsers;

namespace Netfox.SnooperMAFF.Models.Objects
{
    /// <summary>
    /// Class of Base Objects desribes every MAFF object appended to archive.
    /// Class public attributes used for view detail visualizaton
    /// </summary>
    [ComplexType]
    public class BaseObject 
    {
        public byte[] Content { get; set; }
        public string OriginalUrl { get; set; }
        public string PathToFileName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public string HostAddress { get; set; }
        public string Referrer { get; set; }
        public List<string> CookieInformation { get; set; }
        public DateTime TimeStamp { get; set; }
        public IExportSource ExportSource;
        public long FileSize { get; set; }
        public string ArchiveBaseFolder { get; set; }
        public string ExportDirectory { get; set; }
        public List<string> ListOfNewReferences { get; set; }

        public string UniqueHash { get; set; }

        protected HTTPMsg RequestMessage;

        /// <summary>
        /// Returns a string that represents the current object content.
        /// </summary>
        /// <returns>
        /// A string that represents the current object content.
        /// </returns>
        public override String ToString() { return this.Content == null ? "" : Encoding.GetEncoding(437).GetString(this.Content); }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual BaseObject Clone()
        {
            return new BaseObject(this.OriginalUrl, this.PathToFileName, this.FileName, this.FileExtension, this.ContentType, this.RequestMessage, this.CookieInformation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseObject"/> class.
        /// </summary>
        /// <param name="sOrigUrl">The original URL.</param>
        /// <param name="sPathToFile">The path to file.</param>
        /// <param name="sFileName">Name of the file.</param>
        /// <param name="sFileExtension">The file extension.</param>
        /// <param name="sContentType">Type of the content.</param>
        /// <param name="oRequestMsg">The request message.</param>
        /// <param name="listCookieInformation">The list cookie information.</param>
        public BaseObject(
            string sOrigUrl,
            string sPathToFile,
            string sFileName,
            string sFileExtension,
            string sContentType,
            HTTPMsg oRequestMsg,
            List<string> listCookieInformation)
        {
            this.RequestMessage     = oRequestMsg;
            this.OriginalUrl        = sOrigUrl;
            this.PathToFileName     = sPathToFile;
            this.FileName           = sFileName;
            this.FileExtension      = sFileExtension;
            this.ContentType        = sContentType;

            this.Referrer           = ObjectParser.ParseReferer((HttpRequestHeader)oRequestMsg.HTTPHeader);
            this.HostAddress        = ObjectParser.ParseHostAddress((HttpRequestHeader)oRequestMsg.HTTPHeader);

            this.Content            = oRequestMsg.PairMessages[0].HTTPContent.Content;
            this.TimeStamp          = oRequestMsg.TimeStamp;
            this.ExportSource       = oRequestMsg.ExportSources[0];

            this.CookieInformation  = listCookieInformation;
            this.UniqueHash         = ComputeHash.GetMd5Hash(this.Content);


            this.FileSize            = this.Content.LongLength;
            this.ListOfNewReferences = new List<string>();
        }


        /// <summary>
        /// Gets the byte content of object.
        /// </summary>
        /// <returns>Return byte content of object</returns>
        public virtual byte[] GetContent() { return this.Content; }

        /// <summary>
        /// Rewrites the current byte content.
        /// </summary>
        /// <param name="byteNewContent">New content of the byte.</param>
        public virtual void RewriteContent(byte[] byteNewContent) { this.Content = byteNewContent; }

        /// <summary>
        /// Gets the object referent if was founded.
        /// </summary>
        /// <returns>Returns founded referent</returns>
        public string GetObjectReferent() { return this.HostAddress + this.OriginalUrl; }

        /// <summary>
        /// Gets the object filename path.
        /// </summary>
        /// <returns>Return object filename path</returns>
        public string GetObjectFilenamePath() { return this.HostAddress + this.PathToFileName; }

        /// <summary>
        /// Saves the content of the object to archive.
        /// </summary>
        /// <param name="newArchive">The new archive.</param>
        /// <param name="sExportDirectory">The export directory.</param>
        /// <param name="sPathInArchive">The path in archive.</param>
        public void SaveObjectContent(ZipArchive newArchive, string sExportDirectory, string sPathInArchive)
        {
            var newEntry = newArchive.CreateEntry(sPathInArchive + "/" + this.FileName);
            var newEntryWriteStream = new BinaryWriter(newEntry.Open());
            newEntryWriteStream.Write(this.GetContent());
            newEntryWriteStream.Dispose();

            this.ExportDirectory = sExportDirectory;
            this.ArchiveBaseFolder = sPathInArchive;
        }
    }
}
