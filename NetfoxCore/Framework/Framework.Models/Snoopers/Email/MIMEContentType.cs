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