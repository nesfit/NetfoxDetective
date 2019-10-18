// Copyright (c) 2017 Jan Pluskal
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

using System.IO;
using Netfox.Core.Interfaces.Model.Exports;

namespace Netfox.Framework.Models.Snoopers.Email
{


    /// <summary>
    /// Basic email implementation - enables to store RAW email data and fundamental metadata.
    /// </summary>
    public class RawEMail : SnooperExportedObjectBase
    {
        /// <summary> The rawdata.</summary>
        public byte[] Rawdata { get; set; }


        public string StoredPath { get; private set; }

        /// <summary> Stores raw mail.</summary>
        /// <param name="path"> Full pathname of the file. </param>
        public void StoreRawMail(string path)
        {
            this.StoredPath = Path.Combine(Path.GetFileName(Path.GetDirectoryName(path)), Path.GetFileName(path));
            // File.WriteAllText(path, _rawdata);
            File.WriteAllBytes(path, this.Rawdata);
        }
        /// <summary> Type of email behind this interface.</summary>
        /// <value> The type.</value>
        public EMailType EMailType { get; set; } = EMailType.RawEmail;

        /// <summary> Gets or sets the type of the content.</summary>
        /// <value> The type of the content.</value>
        public EMailContentType ContentType { get; set; } = EMailContentType.Unknown;

        protected RawEMail() { }//EF

        /// <summary> Constructor.</summary>
        /// <param name="rawData"> Information describing the raw. </param>
        public RawEMail(SnooperExportBase snooperExportBase, byte[] rawData) : base(snooperExportBase) {
            this.Rawdata = rawData;
        }
    }
}