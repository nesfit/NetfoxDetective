using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Netfox.Core.Database;
using Netfox.Core.Enums;

namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary> Represents part of the IMF MIME document.</summary>
    public class MIMEpart : IEntity
    {
        #region Implementation of IEntity

        [Key] public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; set; }

        #endregion

        /// <summary> Encoding type of the part content.</summary>
        public enum ContentTransferEncodingType
        {
            /// <summary> An enum constant representing the bit 7 option.</summary>
            Bit7,

            /// <summary> An enum constant representing the bit 8 option.</summary>
            Bit8,

            /// <summary> An enum constant representing the binary option.</summary>
            Binary,

            /// <summary> An enum constant representing the quotedprintable option.</summary>
            Quotedprintable,

            /// <summary> An enum constant representing the base 64 option.</summary>
            Base64,

            /// <summary> An enum constant representing the ietftoken option.</summary>
            Ietftoken,

            /// <summary> An enum constant representing the xtoken option.</summary>
            Xtoken
        }

        /// <summary> The encoding.</summary>
        public Encoding Encoding = new ASCIIEncoding();

        //private XElement _errorLogElement;
        /// <summary> The email.</summary>
        private MIMEemail _email;

        /// <summary> The boundary.</summary>
        private String _boundary;

        /// <summary> The headers.</summary>
        private List<MIMEheader> _headers;

        private FileInfo _storedHeaders;
        private FileInfo _storedContent;

        #region Constructors

        private MIMEpart()
        {
        } //EF

        /// <summary> Constructor.</summary>
        /// <param name="email"> The email. </param>
        public MIMEpart(MIMEemail email)
        {
            this._headers = new List<MIMEheader>();
            this.ContainedParts = new List<MIMEpart>();
            this.IsMIMEpart = false;
            this.ContentTransferEncoding = ContentTransferEncodingType.Bit7;
            this._email = email;
            //_errorLogElement = errorLogElement;
        }

        #endregion

        #region debug

        /// <summary> Print statistics.</summary>
        /// <param name="indent"> (Optional) the indent. </param>
        public void PrintStats(int indent = 0)
        {
            var indentString = string.Concat(Enumerable.Repeat("   ", indent));

            Console.WriteLine(indentString + "-------------------");
            Console.WriteLine(indentString + "IsMIMEpart: " + this.IsMIMEpart);
            Console.WriteLine(indentString + "IsMultiPart: " + this.IsMultiPart);
            Console.WriteLine(indentString + "ContentType: " + this.ContentType + "/" + this.ContentSubtype);
            Console.WriteLine(indentString + "ContentBoundary: " + this.ContentBoundary);
            Console.WriteLine(indentString + "ContentTypeName: " + this.ContentTypeName);
            if (this.RawContent != null)
            {
                Console.WriteLine(indentString + "ContentSize: " + this.RawContent.Length);
            }

            Console.WriteLine(indentString + "ContainedParts: ");


            foreach (var part in this.ContainedParts)
            {
                part.PrintStats(indent + 1);
            }

            Console.WriteLine(indentString + "-------------------");
        }

        #endregion

        #region MIME Part properties

        /// <summary> MIME parts included in this part.</summary>
        /// <value> The contained parts.</value>
        public List<MIMEpart> ContainedParts { get; }

        /// <summary> Type of content encoding.</summary>
        /// <value> The content transfer encoding.</value>
        public ContentTransferEncodingType ContentTransferEncoding { get; set; }

        /// <summary> Gets or sets a value indicating whether this object is mim epart.</summary>
        /// <value> true if this object is mim epart, false if not.</value>
        public bool IsMIMEpart { get; set; }

        /// <summary> Gets the part boundary.</summary>
        /// <value> The part boundary.</value>
        public String PartBoundary
        {
            get
            {
                if (this._boundary == null)
                {
                    return null;
                }

                return "--" + this._boundary;
            }
        }

        /// <summary> Gets the content boundary.</summary>
        /// <value> The content boundary.</value>
        public String ContentBoundary
        {
            get
            {
                if (this._boundary == null)
                {
                    return null;
                }

                return "--" + this._boundary + "--";
            }
        }

        /// <summary> Gets or sets a value indicating whether this object is multi part.</summary>
        /// <value> true if this object is multi part, false if not.</value>
        public bool IsMultiPart { get; set; }

        /// <summary> Gets or sets the type of the content.</summary>
        /// <value> The type of the content.</value>
        public string ContentType { get; set; }

        /// <summary> Gets or sets the content subtype.</summary>
        /// <value> The content subtype.</value>
        public string ContentSubtype { get; set; }

        /// <summary> Gets or sets the name of the content type.</summary>
        /// <value> The name of the content type.</value>
        public string ContentTypeName { get; set; }

        /// <summary> Gets or sets the content type charset.</summary>
        /// <value> The content type charset.</value>
        public string ContentTypeCharset { get; set; }

        /// <summary> Gets or sets the content disposition.</summary>
        /// <value> The content disposition.</value>
        public string ContentDisposition { get; set; }

        /// <summary> Gets or sets the filename of the content disposition file.</summary>
        /// <value> The filename of the content disposition file.</value>
        public string ContentDispositionFileName { get; set; }

        /// <summary> Gets or sets the identifier of the content.</summary>
        /// <value> The identifier of the content.</value>
        public string ContentID { get; set; }

        /// <summary> Gets or sets the identifier of the message.</summary>
        /// <value> The identifier of the message.</value>
        public string MessageID { get; set; }

        /// <summary> Gets or sets the in reply to.</summary>
        /// <value> The in reply to.</value>
        public string InReplyTo { get; set; }

        /// <summary> Gets or sets the subject.</summary>
        /// <value> The subject.</value>
        public string Subject { get; set; }

        /// <summary> Gets or sets the source for the.</summary>
        /// <value> from.</value>
        public string From { get; set; }

        /// <summary> Gets or sets to.</summary>
        /// <value> to.</value>
        public string To { get; set; }

        /// <summary> Gets or sets the Cc.</summary>
        /// <value> The Cc.</value>
        public string Cc { get; set; }

        /// <summary> Gets or sets the Bcc.</summary>
        /// <value> The Bcc.</value>
        public string Bcc { get; set; }

        /// <summary> Gets or sets the date.</summary>
        /// <value> The date.</value>
        public string Date { get; set; }

        /// <summary> Gets or sets the raw content.</summary>
        /// <value> The raw content.</value>
        public string RawContent { get; set; }

        /// <summary> Gets or sets the raw headers.</summary>
        /// <value> The raw headers.</value>
        public string RawHeaders { get; set; }

        /// <summary> Gets or sets a value indicating whether this object has empty content.</summary>
        /// <value> true if this object has empty content, false if not.</value>
        public bool HasEmptyContent { get; set; }

        /// <summary> Gets the filename of the suggested file.</summary>
        /// <value> The filename of the suggested file.</value>
        public string SuggestedFilename
        {
            get
            {
                if (!(String.IsNullOrEmpty(this.ContentDispositionFileName)))
                {
                    return this.ContentDispositionFileName;
                }

                if (!(String.IsNullOrEmpty(this.ContentTypeName)))
                {
                    return this.ContentTypeName;
                }

                return String.Empty;
            }
        }

        /// <summary> Path to the RAW headers file. If empty, headers were not stored.</summary>
        /// <value> The full pathname of the stored headers file.</value>
        [NotMapped]
        public FileInfo StoredHeaders
        {
            get { return this._storedHeaders ?? (this._storedHeaders = new FileInfo(this.StoredHeadersFilePath)); }
            private set
            {
                this._storedHeaders = value;
                this.StoredHeadersFilePath = value.FullName;
            }
        }

        public string StoredHeadersFilePath { get; set; }
        public string StoredContentFilePath { get; set; }

        /// <summary> Path to the part content file. If empty, content was not stored.</summary>
        /// <value> The full pathname of the stored content file.</value>
        [NotMapped]
        public FileInfo StoredContent
        {
            get { return this._storedContent ?? (this._storedContent = new FileInfo(this.StoredContentFilePath)); }
            private set
            {
                this._storedContent = value;
                this.StoredContentFilePath = value.FullName;
            }
        }

        /// <summary>
        ///     Determines extension for data content file. Extension is determined from headers fields.
        /// </summary>
        /// <value> The extension.</value>
        public string Extension
        {
            get
            {
                // try to determine from Content-Type : name
                if (this.ContentTypeName != String.Empty)
                {
                    try
                    {
                        if (this.ContentTypeName != null)
                        {
                            var replaced = this.ContentTypeName.Replace("\"", "");
                            if (replaced != null)
                            {
                                var ext = Path.GetExtension(replaced);
                                if (ext != null)
                                {
                                    return ext.Substring(1);
                                }
                            }
                        }
                    }
                    catch (Exception ex) //todo
                    {
                        //_log.Error("Sleuth General exception", ex);
                        this._email.AddErrorReport(FcLogLevel.Warn, "MIME Part", "Unable to determine part extension",
                            this.ContentTypeName, ex);
                    }
                }

                // Try to determine from content type
                var extC = MIMEContentType.ContentTypeToExt(this.ContentType, this.ContentSubtype);

                if (extC != String.Empty)
                {
                    return extC;
                }

                // Otherwise take default extension for text or binary data
                if (this.ContentTransferEncoding == ContentTransferEncodingType.Bit7 ||
                    this.ContentTransferEncoding == ContentTransferEncodingType.Bit8)
                {
                    return "txt";
                }

                return "bin";
            }
        }

        #endregion

        #region MIME Part modifiers

        /// <summary> Add children part to list of childrens parts.</summary>
        /// <param name="part"> The part. </param>
        public void AddPart(MIMEpart part)
        {
            this.ContainedParts.Add(part);
        }

        /// <summary> Store part data to files.</summary>
        /// <param name="basePath"> Target directory. </param>
        public void StorePartData(DirectoryInfo exportDirectoryInfo)
        {
            this.StorePartHeaders(exportDirectoryInfo);
            this.StorePartContent(exportDirectoryInfo);

            foreach (var part in this.ContainedParts)
            {
                part.StorePartData(exportDirectoryInfo);
            }
        }

        /// <summary> Clear content and headers data from memory.</summary>
        public void FreeDataFromMemory()
        {
            this.RawContent = String.Empty;
            this.RawHeaders = String.Empty;

            foreach (var part in this.ContainedParts)
            {
                part.FreeDataFromMemory();
            }
        }

        #endregion

        #region Private store and convert methods

        /// <summary> Stores part headers.</summary>
        /// <param name="path"> Full pathname of the file. </param>
        private void StorePartHeaders(DirectoryInfo exportDirectoryInfo)
        {
            this.StoredHeaders =
                new FileInfo(Path.Combine(exportDirectoryInfo.FullName, Guid.NewGuid() + "-HEADERS.txt"));
            File.WriteAllText(this.StoredHeaders.FullName, this.RawHeaders);
        }

        /// <summary> Stores part content.</summary>
        /// <param name="path"> Full pathname of the file. </param>
        private void StorePartContent(DirectoryInfo exportDirectoryInfo)
        {
            byte[] binaryData = null;

            if (this.RawContent == null)
            {
                return;
            }

            this.StoredContent = new FileInfo(Path.Combine(exportDirectoryInfo.FullName,
                Guid.NewGuid() + "-CONTENT." + this.Extension));

            // default content encoding is ASCII
            //Encoding enc = Encoding.ASCII;

            /*   try
               {   // try to determine content enconding according to headers
                   if (!String.IsNullOrEmpty(ContentTypeCharset))
                       enc = Encoding.GetEncoding(ContentTypeCharset);
               }
               catch (Exception ex)//todo
               {
                   //TODO
                   _log.Error("Sleuth General exception", ex);
               }

               */

            if (this.ContentTransferEncoding == ContentTransferEncodingType.Bit7 ||
                this.ContentTransferEncoding == ContentTransferEncodingType.Bit8)
            {
                // plaintext data
                binaryData = this.Encoding.GetBytes(this.RawContent);
            }
            else if (this.ContentTransferEncoding == ContentTransferEncodingType.Base64)
            {
                // base64 encoding
                try
                {
                    //Console.WriteLine("RAWBASE64:\r\n" + RawContent);

                    // try to convert to binary data 
                    binaryData = Convert.FromBase64String(this.RawContent);
                }
                catch (Exception ex) //todo
                {
                    // in case of error write plaintext data
                    //_errorLogElement.WriteMIMEParseError(path,ex.Message);
                    //  _log.Error("Sleuth General exception", ex);

                    this._email.AddErrorReport(FcLogLevel.Error, "MIME Part", "Unable to decode Base64 data",
                        this.StoredContent.Name, ex);

                    /*  _errorLogElement.Add(new XElement("MIMEPartError",
                          new XAttribute("partPath", path),
                          new XAttribute("exceptionMessage", ex.Message)));
                      */
                    binaryData = Encoding.ASCII.GetBytes(this.RawContent);
                }
            }
            else if (this.ContentTransferEncoding == ContentTransferEncodingType.Quotedprintable)
            {
                // quotedprintable encoding 
                // TODO check if works correct
                var contentDecoted = this.RawContent;

                var occurences = new Regex(@"(=[0-9A-Z]{2}){1,}", RegexOptions.Multiline);
                var matches = occurences.Matches(contentDecoted);

                foreach (Match match in matches)
                {
                    try
                    {
                        var b = new byte[match.Groups[0].Value.Length / 3];
                        for (var i = 0; i < match.Groups[0].Value.Length / 3; i++)
                        {
                            b[i] = byte.Parse(match.Groups[0].Value.Substring(i * 3 + 1, 2),
                                NumberStyles.AllowHexSpecifier);
                        }

                        var hexChar = this.Encoding.GetChars(b);
                        contentDecoted = contentDecoted.Replace(match.Groups[0].Value, hexChar[0].ToString());
                    }
                    catch (Exception ex) //todo
                    {
                        //_log.Error("Sleuth General exception", ex);
                        // TODO
                        this._email.AddErrorReport(FcLogLevel.Warn, "MIME Part",
                            "Unable to decode Quoted printable content", this.StoredContent.Name, ex);
                    }
                }

                contentDecoted = contentDecoted.Replace("?=", "").Replace("=\r\n", "");

                binaryData = this.Encoding.GetBytes(contentDecoted);
            }
            else
            {
                // Unknown conent encoding - write plainttext data
                //_errorLogElement.WriteMIMEParseError(path, "Unknown Content-encoding");
                /*_errorLogElement.Add(new XElement("MIMEPartError",
                    new XAttribute("partPath", path),
                    new XAttribute("exceptionMessage", "Unknown Content-encoding")));*/

                this._email.AddErrorReport(FcLogLevel.Warn, "MIME Part", "Unknown Content-encoding assuming ASCII",
                    this.StoredContent.Name);

                binaryData = Encoding.ASCII.GetBytes(this.RawContent);
            }

            // write content data to target file
            FileStream outFile;
            outFile = new FileStream(this.StoredContent.FullName, FileMode.Create, FileAccess.Write);
            outFile.Write(binaryData, 0, binaryData.Length);
            outFile.Close();
        }

        #endregion

        #region IMF headers processing

        /// <summary>
        ///     Add preparsed header to the part. Important headers are analyzed and mirrored in part
        ///     properties.
        /// </summary>
        /// <param name="header"> The header. </param>
        public void AddHeader(MIMEheader header)
        {
            if (header == null)
            {
                return;
            }

            this._headers.Add(header);

            if (header.Type.Equals("MIME-Version") && header.Value.Trim().Equals("1.0"))
            {
                this.IsMIMEpart = true;
            }
            else if (header.Type.Equals("Content-Type"))
            {
                this.ParseContentType(header);
            }
            else if (header.Type.Equals("Content-ID"))
            {
                this.ParseContentId(header);
            }
            else if (header.Type.Equals("Content-Transfer-Encoding"))
            {
                this.ParseContentTransferEncoding(header);
            }
            else if (header.Type.Equals("Message-ID"))
            {
                this.ParseMessageID(header);
            }
            else if (header.Type.Equals("In-Reply-To"))
            {
                this.ParseInReplyTo(header);
            }
            else if (header.Type.Equals("From"))
            {
                this.ParseFrom(header);
            }
            else if (header.Type.Equals("To"))
            {
                this.ParseTo(header);
            }
            else if (header.Type.Equals("Date"))
            {
                this.ParseDate(header);
            }
            else if (header.Type.Equals("Content-Disposition"))
            {
                this.ParseContentDisposition(header);
            }
            else if (header.Type.Equals("Cc"))
            {
                this.ParseCc(header);
            }
            else if (header.Type.Equals("Bcc"))
            {
                this.ParseBcc(header);
            }
            else if (header.Type.Equals("Subject"))
            {
                this.ParseSubject(header);
            }
        }

        /// <summary> Parse date.</summary>
        /// <param name="header"> The header. </param>
        private void ParseDate(MIMEheader header)
        {
            var dateString = header.Value.Trim();


            var parts = dateString.Split(' ');
            if (parts.Count() > 6)
            {
                var partsList = new List<string>(parts);
                partsList.RemoveRange(6, partsList.Count - 6);
                dateString = string.Join(" ", partsList);
            }

            var myCultureInfo = new CultureInfo("en-US");

            try
            {
                var partDateTime = DateTime.ParseExact(dateString, "ddd, dd MMM yyyy HH:mm:ss zzzz",
                    CultureInfo.InvariantCulture);
                this.Date = partDateTime.ToUniversalTime().ToString("yyyy-MM-dd hh.mm.ss.fffffff");
            }
            catch (FormatException ex)
            {
                // _log.Error("Sleuth FormatException", ex);

                this._email.AddErrorReport(FcLogLevel.Warn, "MIME Part", "Unable to parse date", dateString, ex);

                //Console.WriteLine("Unable to parse '{0}' '{1}'", dateString,ex.Message);
            }
        }

        /// <summary> Parse from.</summary>
        /// <param name="header"> The header. </param>
        private void ParseFrom(MIMEheader header)
        {
            this.From = header.Value.Trim();
        }

        /// <summary> Parse subject.</summary>
        /// <param name="header"> The header. </param>
        private void ParseSubject(MIMEheader header)
        {
            this.Subject = header.Value.Trim();
        }

        /// <summary> Parse to.</summary>
        /// <param name="header"> The header. </param>
        private void ParseTo(MIMEheader header)
        {
            this.To = header.Value.Trim();
        }

        /// <summary> Parse Cc.</summary>
        /// <param name="header"> The header. </param>
        private void ParseCc(MIMEheader header)
        {
            this.Cc = header.Value.Trim();
        }

        /// <summary> Parse Bcc.</summary>
        /// <param name="header"> The header. </param>
        private void ParseBcc(MIMEheader header)
        {
            this.Bcc = header.Value.Trim();
        }

        /// <summary> Parse message identifier.</summary>
        /// <param name="header"> The header. </param>
        private void ParseMessageID(MIMEheader header)
        {
            var messageID = header.Value.Trim();

            this.MessageID = messageID;
        }

        /// <summary> Parse in reply to.</summary>
        /// <param name="header"> The header. </param>
        private void ParseInReplyTo(MIMEheader header)
        {
            this.InReplyTo = header.Value.Trim();
        }

        /// <summary> Parse content disposition.</summary>
        /// <param name="header"> The header. </param>
        private void ParseContentDisposition(MIMEheader header)
        {
            //From = header.Value.Trim();
            var semSplit = new Regex("(?:^|;)(\"(?:[^\"]+|\"\")*\"|[^;]*)", RegexOptions.Compiled);

            var first = true;
            foreach (Match match in semSplit.Matches(header.Value))
            {
                if (first)
                {
                    this.ContentDisposition = match.Value.Trim();
                    first = false;
                }
                else
                {
                    var trimPart = match.Value.TrimStart(';').Trim();
                    var equalSeparator = new[]
                    {
                        '='
                    };
                    var partParts = trimPart.Split(equalSeparator, 2);
                    if (partParts.Count() == 2)
                    {
                        partParts[1] = partParts[1].Trim(' ', '\t', '\n', '\v', '\f', '\r', '"');
                        if (partParts[0].Equals("filename"))
                        {
                            this.ContentDispositionFileName = partParts[1];
                        }
                    }
                }
            }
        }

        /// <summary> Parse content transfer encoding.</summary>
        /// <param name="header"> The header. </param>
        private void ParseContentTransferEncoding(MIMEheader header)
        {
            switch (header.Value.Trim())
            {
                case "7bit":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Bit7;
                    break;
                case "8bit":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Bit8;
                    break;
                case "binary":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Binary;
                    break;
                case "quoted-printable":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Quotedprintable;
                    break;
                case "base64":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Base64;
                    break;
                case "ietf-token":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Ietftoken;
                    break;
                case "x-token":
                    this.ContentTransferEncoding = ContentTransferEncodingType.Xtoken;
                    break;
            }
        }

        /// <summary> Parse content identifier.</summary>
        /// <param name="header"> The header. </param>
        private void ParseContentId(MIMEheader header)
        {
            var contentID = header.Value.Trim();

            /* contentID = contentID.Remove(0, 1);
             contentID = contentID.Remove(contentID.Length-1, 1);
             */
            this.ContentID = contentID;
        }

        /// <summary> Parse content type.</summary>
        /// <param name="header"> The header. </param>
        private void ParseContentType(MIMEheader header)
        {
            var semSplit = new Regex("(?:^|;)(\"(?:[^\"]+|\"\")*\"|[^;]*)", RegexOptions.Compiled);

            var first = true;
            foreach (Match match in semSplit.Matches(header.Value))
            {
                if (first)
                {
                    var slashSeparator = new[]
                    {
                        '/'
                    };
                    var typeParts = match.Value.Split(slashSeparator);
                    if (typeParts.Count() != 2)
                    {
                        return;
                    }

                    this.ContentType = typeParts[0].Trim();
                    this.ContentSubtype = typeParts[1].Trim();

                    if (this.ContentType.Equals("multipart"))
                    {
                        this.IsMultiPart = true;
                    }

                    first = false;
                }
                else
                {
                    var trimPart = match.Value.TrimStart(';').Trim();
                    var equalSeparator = new[]
                    {
                        '='
                    };
                    var partParts = trimPart.Split(equalSeparator, 2);
                    if (partParts.Count() == 2)
                    {
                        partParts[1] = partParts[1].Trim(' ', '\t', '\n', '\v', '\f', '\r', '"');
                        if (partParts[0].Equals("boundary"))
                        {
                            this._boundary = partParts[1];
                        }
                        else if (partParts[0].Equals("name"))
                        {
                            this.ContentTypeName = partParts[1];
                        }
                        else if (partParts[0].Equals("charset"))
                        {
                            this.ContentTypeCharset = partParts[1];
                            try
                            {
                                this.Encoding = Encoding.GetEncoding(this.ContentTypeCharset);
                                ;
                            }
                            catch (Exception ex)
                            {
                                this._email.AddErrorReport(FcLogLevel.Warn, "MIME Part",
                                    "Unable to determine ConentType-Chartset", this.ContentTypeCharset, ex);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}