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
using System.IO;
using System.Linq;
using System.Text;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Core.Models.Exports;

namespace Netfox.Framework.Models.Snoopers.Email
{
    /// <summary>
    /// Extended RAW email implementation. Enables to parse RAW email data to MIME document parts
    /// tree structure.
    /// </summary>
    public class MIMEemail : RawEMail, IEMail
    {
        //    public int OrderKey { get; set; }//Database order
        private MIMEemail() { }//EF
        /// <summary> Constructor.</summary>
        /// <param name="rawData">       Information describing the raw. </param>
        /// <param name="contentType">   Type of the content. </param>
        /// <param name="exportedEmail"> The exported email. </param>
        public MIMEemail(SnooperExportEmailBase snooperExportBase, byte[] rawData, EMailContentType contentType) : base(snooperExportBase,rawData)
        {
            this.ContentType = contentType;
            this.ParseRawData();
            this.StorePrasedData(snooperExportBase.DirectoryInfo);
        }

        /// <summary> Adds an error report.</summary>
        /// <param name="level">       The level. </param>
        /// <param name="source">      Source for the. </param>
        /// <param name="description"> The description. </param>
        /// <param name="detail">      (Optional) the detail. </param>
        /// <param name="ex">          (Optional) the ex. </param>
        public void AddErrorReport(FcLogLevel level, string source, string description, string detail = null, Exception ex = null)
        {
                //Debugger.Break();
                this.Reports.Add(new ExportReport()
                { //TODO commented to be compiled FIX Filip!!!
                    //Level = level,
                    //Source = source,
                    Description = description,
                    Detail = detail,
                    Exception = ex,
                    //FrameNumbers = this.ExportedEmail.FrameNumbers != null ? this.ExportedEmail.FrameNumbers.ToArray() : null
                    //FrameNumbers = new uint[] { 1084,1085}
                });
        }

        #region Class variables

        /// <summary> Root of the MIME document structure.</summary>
        /// <value> The document root.</value>
        public MIMEpart DocumentRoot { private set; get; }

        #endregion

        #region Data manipulation methods

        /// <summary> Parse raw data.</summary>
        public void ParseRawData()
        {
            using (var rawData = new MemoryStream(this.Rawdata))
            {
                using (var dataStream = new LineReader(rawData, 1024, Encoding.ASCII))
                {
                    bool isFinalPart;
                    this.DocumentRoot = this.ParseMIMEpart(this, dataStream, null, out isFinalPart);
                }
            }

            //DocumentRoot.PrintStats();
        }

        /// <summary> Stores prased data.</summary>
        /// <param name="basePath"> Full pathname of the base file. </param>
        public void StorePrasedData(DirectoryInfo exportDirectoryInfo)
        {
            this.DocumentRoot.StorePartData(exportDirectoryInfo);
        }


        /// <summary> Clear RAW data stored in memory.</summary>
        public void FreeDataFromMemory()
        {
            this.Rawdata = null;

            this.DocumentRoot.FreeDataFromMemory();
        }

        public string From => this.DocumentRoot.From;

        public string To => this.DocumentRoot.To;

        public string Bcc => this.DocumentRoot.Bcc;

        public string Cc => this.DocumentRoot.Cc;

        public string Subject => this.DocumentRoot.Subject;

        public string RawContent => this.DocumentRoot.RawContent;
        #endregion

        #region IMF MIME parsing

        /// <summary> Parse mim epart.</summary>
        /// <param name="mimEemail">  The mim eemail. </param>
        /// <param name="dataStream"> The data stream. </param>
        /// <param name="parentPart"> The parent part. </param>
        /// <param name="finalPart">  [out] The final part. </param>
        /// <returns> A MIMEpart.</returns>
        private MIMEpart ParseMIMEpart(MIMEemail mimEemail, LineReader dataStream, MIMEpart parentPart, out bool finalPart)
        {
            var currentPart = new MIMEpart(mimEemail);
            if (parentPart != null)
                currentPart.Encoding = parentPart.Encoding;

            finalPart = false;

            var boundaryFound = false;

            // parse IMF headers
            this.ParseHeaders(dataStream, currentPart, parentPart, Encoding.ASCII, out boundaryFound);

            if (boundaryFound) // boundary was found => empty content
            {
                currentPart.HasEmptyContent = true;
                return currentPart;
            }

            // if current part is also multipart parse all sub parts
            if (currentPart.IsMultiPart)
            {
                var isFinalPart = false;
                currentPart.RawContent = this.ReadData(dataStream, currentPart.PartBoundary, currentPart.ContentBoundary, currentPart.Encoding,
                    out isFinalPart);

                while (!isFinalPart)
                {
                    currentPart.AddPart(this.ParseMIMEpart(mimEemail, dataStream, currentPart, out isFinalPart));
                }
            }
            else
            {
                // this is not multipart part => read part content :

                if (parentPart == null || parentPart.ContentBoundary == null)
                {
                    // boundary was not set or specified - read till end of IMF data :
                    finalPart = true;
                    //currentPart.RawContent = dataStream.ReadToEnd();

                    //currentPart.RawContent = currentPart.encoding.GetString(dataStream.ReadBytes(int.MaxValue));
                    // currentPart.RawContent = dataStream.ReadToEnd();

                    currentPart.RawContent = currentPart.Encoding.GetString(dataStream.ReadBytes((int)(dataStream.BaseStream.Length - dataStream.BaseStream.Position)));
                }
                else
                {
                    bool isDataFinalPart;
                    currentPart.RawContent = currentPart.RawContent = this.ReadData(dataStream, parentPart.PartBoundary, parentPart.ContentBoundary, currentPart.Encoding,
                        out isDataFinalPart);

                    if (isDataFinalPart)
                        finalPart = true;
                }

            }



            if (currentPart.RawContent == null)
                currentPart.HasEmptyContent = true;

            return currentPart;
        }

        /// <summary> Reads a line.</summary>
        /// <param name="dataStream"> The data stream. </param>
        /// <param name="encoding">   The encoding. </param>
        /// <returns> The line.</returns>
        private string ReadLine(LineReader dataStream, Encoding encoding)
        {
            var offset = 0;
            var cur = dataStream.BaseStream.Position;

            while (dataStream.BaseStream.CanRead && dataStream.ReadChar() != '\n')
                offset++;

            dataStream.BaseStream.Seek(cur, SeekOrigin.Begin);

            var line = encoding.GetString(dataStream.ReadBytes(offset));
            dataStream.ReadChar();

            line = line.TrimEnd('\r');

            return line;
        }

        /// <summary> Reads a data.</summary>
        /// <param name="dataStream">      The data stream. </param>
        /// <param name="partBoundary">    The part boundary. </param>
        /// <param name="contentBoundary"> The content boundary. </param>
        /// <param name="encoding">        The encoding. </param>
        /// <param name="isFinalPart">     [out] The is final part. </param>
        /// <returns> The data.</returns>
        public string ReadData(LineReader dataStream, string partBoundary, string contentBoundary, Encoding encoding, out bool isFinalPart)
        {
            //  byte[] expected = Encoding.Default.GetBytes("World");

            isFinalPart = false;
            // will read till boundary line is found :
            var sb = new StringBuilder();

            var line = String.Empty;
            while (line != null)
            {
                // read line by line :

                //    Array.IndexOf(dinosaurs, "Tyrannosaurus")

                //   line = dataStream.ReadLine();
                //line = ReadLine(dataStream, encoding);
                line = dataStream.ReadLine(encoding);
                if (line != null)
                {
                    // check if line is part boundary
                    if (!String.IsNullOrEmpty(partBoundary) && line.Equals(partBoundary))
                    {
                        isFinalPart = false;
                        break;
                    }

                    // check if line is content boundary
                    if (!String.IsNullOrEmpty(contentBoundary) && line.Equals(contentBoundary))
                    {
                        // if content boundary was found - this is the final part
                        isFinalPart = true;
                        break;
                    }

                    sb.Append(line + Environment.NewLine);
                }
            }

            if (line == null)   // end of document is allways final part ...
                isFinalPart = true;

            // concatanate document parts to content string
            return sb.ToString();
        }

        /// <summary> Parse headers.</summary>
        /// <param name="dataStream">    The data stream. </param>
        /// <param name="currentPart">   The current part. </param>
        /// <param name="parentPart">    The parent part. </param>
        /// <param name="encoding">      The encoding. </param>
        /// <param name="boundaryFound"> [out] The boundary found. </param>
        public void ParseHeaders(LineReader dataStream, MIMEpart currentPart, MIMEpart parentPart, Encoding encoding, out bool boundaryFound)
        {
            var separator = new char[] { ':' };
            var line = String.Empty;

            MIMEheader currentHeader = null;

            boundaryFound = false;

            while (line != null)
            {   // read line by line
                //line = dataStream.ReadLine();)
                line = dataStream.ReadLine(encoding);
                if (line != null)
                {
                    // break whene empty line (headers delimiter was found)
                    // or end of document was reached
                    if (line.Equals(String.Empty) || line.Length == 0)
                        break;

                    // check for parrent boundary line - in case of empty part conent
                    if (parentPart != null && parentPart.ContentBoundary != null)
                    {
                        if (line.Equals(parentPart.ContentBoundary) || line.Equals(parentPart.PartBoundary))
                        {   // content is empty - break
                            boundaryFound = true;
                            break;
                        }
                    }

                    // store line to raw headers string
                    currentPart.RawHeaders += line + Environment.NewLine;

                    // handle multiline headers :
                    if (char.IsWhiteSpace(line[0]))
                    {
                        // if header line begins with white character
                        // add its conent to previous header value
                        if (currentHeader != null)
                            currentHeader.Value += line.Trim();
                    }
                    else
                    {   // next header was detected :
                        if (currentHeader != null)
                        {   // if this is not the first one - store previous header
                            currentPart.AddHeader(currentHeader);
                            currentHeader = null;
                        }

                        // split first part of header to type and value parts :
                        var parts = line.Split(separator, 2);

                        if (parts.Count() == 2)
                        {
                            currentHeader = new MIMEheader();
                            currentHeader.Type = parts[0].Trim();
                            currentHeader.Value = parts[1].Trim();
                        }
                    }

                }
            }

            // store last found header before exit
            if (currentHeader != null)
                currentPart.AddHeader(currentHeader);
        }


        #endregion

    }
}