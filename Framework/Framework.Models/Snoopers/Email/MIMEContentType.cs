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

using System;

namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary> A mime content type.</summary>
    public class MIMEContentType
    {
        /// <summary> Content type to extent.</summary>
        /// <param name="type">    The type. </param>
        /// <param name="subtype"> The subtype. </param>
        /// <returns> A string.</returns>
        public static string ContentTypeToExt(string type, string subtype)
        {
            if (type == null) type = String.Empty;
            if (subtype == null) subtype = String.Empty;

            switch (type)
            {
                case "application":

                    switch (subtype)
                    {
                        case "octet-stream": return "exe";
                        case "zip": return "zip";
                        case "rar": return "rar";
                    }

                    break;

                case "image":

                    switch (subtype)
                    {
                        case "tiff": return "tiff";
                        case "jpeg": return "jpeg";
                    }

                    break;

                case "text":

                    switch (subtype)
                    {
                        case "plain": return "txt";
                        case "html": return "html";
                    }

                    break;
            }

            return String.Empty;
        }

    }
}