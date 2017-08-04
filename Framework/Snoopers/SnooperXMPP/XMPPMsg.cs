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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Castle.Core.Internal;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.SnooperXMPP
{
    public enum XMPPMsgType
    {
        OTHER,
        MSG // message
    }

    public class XMPPMsg
    {
        private readonly PDUStreamReader _reader;

        public XMPPMsg(PDUStreamReader reader)
        {
            // init
            this._reader = reader;
            this.Valid = true;
            this.InvalidReason = String.Empty;
            this.ExportSources = new List<IExportSource>();


            this.Parse();
        }

        public DateTime Timestamp { get; set; }
        public IEnumerable<PmFrameBase> Frames { get; set; }
        public string InvalidReason { get; set; }
        public List<IExportSource> ExportSources { get; set; }
        public bool Valid { get; set; }
        public XMPPMsgType MsgType { get; set; }
        public string MsgBody { get; set; }
        public DaRFlowDirection Direction { get; set; }

        public string From { get; private set; }
        public string To { get; private set; }

        /// <summary> Whether a given character is allowed by XML 1.0.</summary>
        /// <param name="character"> The character. </param>
        /// <returns> true if legal XML character, false if not.</returns>
        private bool IsLegalXmlChar(int character)
            =>
                (character == 0x9 /* == '\t' == 9   */|| character == 0xA /* == '\n' == 10  */|| character == 0xD /* == '\r' == 13  */|| (character >= 0x20 && character <= 0xD7FF)
                 || (character >= 0xE000 && character <= 0xFFFD) || (character >= 0x10000 && character <= 0x10FFFF));

        private void Parse()
        {
            this.MsgBody = string.Empty;
            var streamProvider = this._reader.PDUStreamBasedProvider;
            this.Frames = streamProvider.ProcessedFrames;

            if(streamProvider.GetCurrentPDU() != null) {
                this.Timestamp = streamProvider.GetCurrentPDU().FirstSeen;
            }
            else
            {
                this.InvalidReason = "could not retrieve PDU";
                this.ExportSources.Add(streamProvider.Conversation);
                this.Valid = false;
                return;
            }

            if(!streamProvider.GetCurrentPDU().L7Conversation.ApplicationTags.Any())
            {
                this.Valid = false;
                this.InvalidReason = "no application tag";
                this.ExportSources.Add(streamProvider.GetCurrentPDU());
                return;
            }

            // get message strign
            var msgString = this._reader.ReadToEnd();

            // prepare XML parsing
            var xmlString = this.SanitizeXmlString(msgString);
            if(xmlString.IsNullOrEmpty())
            {
                // TODO: what to do with empty messages?
                return;
            }
            var nt = new NameTable();
            var nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("stream", "http://etherx.jabber.org/streams");
            nsmgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace");
            var context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);
            var xset = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                ValidationType = ValidationType.None,
                CheckCharacters = false,
                ConformanceLevel = ConformanceLevel.Fragment,
                ValidationFlags = XmlSchemaValidationFlags.None
            };

            this.ExportSources.Add(streamProvider.GetCurrentPDU());
            this.Direction = this._reader.PDUStreamBasedProvider.GetCurrentPDU().FlowDirection;
            using(var reader = XmlReader.Create(new StringReader(xmlString), xset, context))
            {
                try
                {
                    while(reader.Read())
                    {
                        switch(reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch(reader.Name)
                                {
                                    case "message":
                                        this.MsgType = XMPPMsgType.MSG;
                                        this.ProcessMessageElement(reader);
                                        break;
                                }
                                break;
                        }
                    }
                }
                catch(XmlException ex)
                {
                    if(ex.Message.StartsWith("Unexpected end of file has occurred.")) { return; }

                    this.InvalidReason = "xml parsing error";
                    this.ExportSources.Add(streamProvider.Conversation);
                    this.Valid = false;
                    return;
                }
                catch(Exception)
                {
                    this.InvalidReason = "unknown parsing error";
                    this.ExportSources.Add(streamProvider.Conversation);
                    this.Valid = false;
                    return;
                }
            }
        }

        private void ProcessMessageElement(XmlReader reader)
        {
            this.From = reader.GetAttribute("from");
            this.To = reader.GetAttribute("to");

            while(reader.Read())
            {
                //reader.Read();
                if(reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("message", StringComparison.OrdinalIgnoreCase))
                {
                    //If there is not found body el. and should reach out of message elemenet
                    break;
                }

                if(reader.NodeType != XmlNodeType.Element) { continue; }
                switch(reader.Name)
                {
                    case "body": //Read until Body element is found
                        reader.Read();
                        if(reader.NodeType == XmlNodeType.Text)
                        {
                            if(!this.MsgBody.IsNullOrEmpty()) { this.MsgBody += Environment.NewLine; }
                            this.MsgBody += reader.Value;
                        }
                        break;
                }
            }
        }

        private string SanitizeXmlString(string xml)
        {
            if(xml == null) { return null; }

            var buffer = new StringBuilder(xml.Length);

            foreach(var c in xml) { if(this.IsLegalXmlChar(c)) { buffer.Append(c); } }

            return buffer.ToString();
        }
    }
}