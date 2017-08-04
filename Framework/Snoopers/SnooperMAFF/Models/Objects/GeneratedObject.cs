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
using Netfox.SnooperHTTP.Models;

namespace Netfox.SnooperMAFF.Models.Objects
{
    public class GeneratedObject : TextObject
    {
        private readonly long _lGeneratedObjectIndex;

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override BaseObject Clone()
        {
            return new GeneratedObject(this.OriginalUrl, this.PathToFileName, this.FileName, this.FileExtension, this.ContentType, this.RequestMessage, this.CookieInformation, this._lGeneratedObjectIndex);
        }

        public GeneratedObject(
            string sOrigUrl,
            string sPathToFile,
            string sFileName,
            string sFileExtension,
            string sContentType,
             HTTPMsg oRequestMsg,
            List<string> listCookieInformation,
            long lGeneratedObjectIndex) : base(sOrigUrl, sPathToFile, sFileName, sFileExtension, sContentType, oRequestMsg, listCookieInformation)
        {
            this._lGeneratedObjectIndex = lGeneratedObjectIndex;
            this.FileName = "ad" + this._lGeneratedObjectIndex + ".js";
            this.FileExtension = "js";
        }
    }
}
