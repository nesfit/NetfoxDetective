// Copyright (c) 2017 Jan Pluskal, Dudek Jindrich
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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Castle.Windsor;
using HtmlAgilityPack;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperHTTP.Models;
using Netfox.SnooperXchat.Models;
using Netfox.SnooperXchat.Models.Text;
using HttpRequestHeader = Netfox.SnooperHTTP.Models.HttpRequestHeader;

namespace Netfox.SnooperXchat
{
    /// <summary>
    /// Main class of XChat snooper. Parses HTTP messages, XChat objects and exports them.
    /// </summary>
    public class SnooperXchat : SnooperBase
    {
        //Constructors: 
        public SnooperXchat() { }
        //public SnooperXchat(WindsorContainer investigationWindsorContainer, SelectedConversations conversations, DirectoryInfo exportDirectory, bool ignoreApplicationTags) : base(investigationWindsorContainer, conversations, exportDirectory, ignoreApplicationTags) { }
        public SnooperXchat(WindsorContainer investigationWindsorContainer, IEnumerable<SnooperExportBase> sourceExports, DirectoryInfo exportDirectory) : base(investigationWindsorContainer,sourceExports, exportDirectory) { }
        #region Overrides of SnooperBase
        public override string Name => "XChat";

        public override string[] ProtocolNBARName => new[]
        {
            "http",
            "xchat"
        };

        public override string Description => "XChat snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            80
        };

        public override SnooperExportBase PrototypExportObject { get; } = new XChatSnooperExport();
        //internal new XChatSnooperExport SnooperExport => base.SnooperExport as XChatSnooperExport;
        protected override SnooperExportBase CreateSnooperExport() => new XChatSnooperExport();
        /// <summary>
        /// Starts processing of conversations for identifying and exporting of XChat messages.
        /// </summary>
        protected override void RunBody()
        {
            this.OnConversationProcessingBegin();

            this.ProcessConversation();

            this.OnConversationProcessingEnd();
        }
        /// <summary>
        /// Identificates and exports XChat objects from conversation.
        /// </summary>
        protected override void ProcessConversation()
        {
            var roomNames = new Dictionary<string, string>(); //Key: id of public chat room, value: name of the room
            foreach (var snooperExportBase in this.SourceExports)
            {
                foreach (var exportedObject in snooperExportBase.ExportObjects)
                {
                    this.OnBeforeProtocolParsing();

                    var httpObject = exportedObject as SnooperExportedDataObjectHTTP;
                    if (httpObject == null) //Skip exports from other snoopers
                        break;

                    var httpMessage = httpObject.Message; //Get http message
                    var httpContent = httpMessage.HTTPContent;
                    //XChat messages must have content
                    if (httpContent?.Content == null)
                    {
                        continue;
                    }

                    var requestHeader = httpMessage.HTTPHeader as HttpRequestHeader;
                    if(requestHeader != null)
                    {
                        if((requestHeader.Method == HTTPRequestMethod.POST) && requestHeader.RequestURI.Contains("new_msg.php")) //Identifies sent message from user
                        {
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var parsedMessage = GetXChatPrivateSentMessage(httpMessage, this.SnooperExport);
                            parsedMessage.ExportSources.Add(exportedObject.ExportSource);
                            
                            //Export of private sent message:
                            this.SnooperExport.AddExportObject(parsedMessage);
                            this.OnAfterDataExporting();
                        }
                        continue;
                    }

                    var strippedContent = Encoding.GetEncoding("ISO-8859-2").GetString(httpContent.Content); //Convert bytes to string in ISO 8859-2 encoding

                    var html = new HtmlDocument(); //For parsing HTML document
                    html.LoadHtml(strippedContent);

                    if(html.DocumentNode == null) //Not Xchat message
                        continue;

                    var roomId = ""; //Variable that change its content when roomMessage is detected 

                    var type = GetMessageType(html, ref roomId); //Get type of message
                    switch(type)
                    {
                        case "rcvdPrivateMessage": //Received private message
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var parsedPrivateMessage = GetXChatPrivateReceivedMessage(httpMessage, html, this.SnooperExport);
                            parsedPrivateMessage.ExportSources.Add(exportedObject.ExportSource);

                            //Export of private received message:
                            this.SnooperExport.AddExportObject(parsedPrivateMessage);
                            this.OnAfterDataExporting();
                            break;
                        case "roomMessage": //Message in public room chat
                            var parsedMessages = GetXchatRoomMessages(html, this.SnooperExport, roomId, roomNames);
                            if(parsedMessages.Length == 0) //If there were no messages
                                continue;

                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();
                            foreach(var parsedRoomMessage in parsedMessages)
                            {
                                parsedRoomMessage.ExportSources.Add(exportedObject.ExportSource);
                                //Export of room message:
                                this.SnooperExport.AddExportObject(parsedRoomMessage);
                            }
                            this.OnAfterDataExporting();
                            break;
                        case "roomName": //Message that contains id and name of public chat room
                            GetRoomName(html, roomNames);
                            break;
                        default: //Not Xchat message
                            continue;
                    }
                }
            }
        }
        /// <summary>
        /// Creates object which represents sent private message from application/x-www-form-urlencoded serialization type
        /// </summary>
        /// <param name="httpMessage">Original HTTP message from HTTP snooper</param>
        /// <param name="exportObject">For referencing XChatSnooperExport from XChat object</param>
        /// <returns>Instance of Xchat sent message</returns>
        private static XChatPrivateMessage GetXChatPrivateSentMessage(HTTPMsg httpMessage, SnooperExportBase exportObject)
        {
            var strippedContent = Encoding.GetEncoding("ISO-8859-2").GetString(httpMessage.HTTPContent.Content); //Convert bytes to string in ISO-8859-2 encoding

            var parsedUrl = HttpUtility.ParseQueryString(strippedContent, Encoding.GetEncoding("ISO-8859-2")); //Parse query from message body

            var message = new XChatPrivateMessage(exportObject)
            {
                Time = httpMessage.TimeStamp.ToString("dd. MM. yyyy HH:mm:ss"),
                Text = parsedUrl["message"],
                Target = parsedUrl["to"],
                Subject = parsedUrl["subject"]
            };
            
            //Getting sender nickname from Cookie in header:
            var requestHeader = httpMessage.HTTPHeader as HttpRequestHeader;
            List<string> values; //List which contains values from Cookie in HTTP request header

            if (requestHeader.Fields.TryGetValue("Cookie", out values))
            {
                var sb = new StringBuilder();
                foreach (var value in values) { sb.Append(value).Append("; "); } //Concatenes values from Cookie in HTTP header

                var regex = new Regex(@"[0-9]{8}=[0-9]{8}%3A([0-9a-zA-Z][a-zA-Z0-9\.-_]{2,19})"); //Regular expression for getting message sender nickname
                var match = regex.Match(sb.ToString());

                message.Source = match.Success ? match.Groups[1].ToString() : "";
            }
            else
                message.Source = "";

            return message;
        }
        /// <summary>
        /// Gets type of Xchat message
        /// </summary>
        /// <param name="html">Parsed HTML document</param>
        /// <param name="roomId">Variable passed by reference which is set when roomMessage is detected</param>
        /// <returns>String that represents type of message</returns>
        private static string GetMessageType(HtmlDocument html, ref string roomId)
        {
            HtmlNode node;

            if (html.DocumentNode.SelectSingleNode("//form[@name='readmsg']") != null) //Recieved private message has in HTML TAG form with name readmsg 
                return "rcvdPrivateMessage";

            if ((node = html.DocumentNode.SelectSingleNode("/html/head/meta[@http-equiv='Refresh' and @content]")) != null) //Messages from public chat are identified with meta tag
            {
                var url = node.Attributes["content"].Value; //Value of attribute content in meta tag
                var position = url.IndexOf("http://", StringComparison.Ordinal); //Get starting position of URI
                if(position != -1) //If substring was found
                {
                    url = url.Substring(position);
                    Uri uri;
                    if (Uri.TryCreate(url, UriKind.Absolute, out uri)) //Convert url in string data type to Uri datatype
                    {
                        var queryParams = HttpUtility.ParseQueryString(uri.Query); //Parse query from URL
                        if(uri.Host == "www.xchat.cz" && queryParams["last_line"] != null && queryParams["rid"] != null) //If host in url is xchat.cz and query contains attributes last_line and rid (roomid)
                        {
                            roomId = queryParams["rid"];
                            return "roomMessage";
                        }
                            
                    }
                }
            }
            if((node = html.DocumentNode.SelectSingleNode("/html/body/p")) != null)
            {
                if(node.InnerText.Contains("Místnost:") && node.SelectSingleNode("//strong/a[@href]") != null)
                    return "roomName";                    
            } 

            return "";
        }
        /// <summary>
        /// Creates object which represents recieved private message from HTML document
        /// </summary>
        /// <param name="httpMessage">Original HTTP message</param>
        /// <param name="html">Parsed HTML document</param>
        /// <param name="exportObject">For referencing XChatSnooperExport from XChat object</param>
        /// <returns>Instance of Xchat received message</returns>
        private static XChatPrivateMessage GetXChatPrivateReceivedMessage(HTTPMsg httpMessage, HtmlDocument html, SnooperExportBase exportObject)
        {
            HtmlNode node;
            var message = new XChatPrivateMessage(exportObject);

            var form = html.DocumentNode.SelectSingleNode("//form[@name='readmsg']"); //Node that represents form with class readmsg

            node = form.SelectSingleNode("//span[@class='offlinesm']/child::a[@title='profil uživatele']/strong"); //Position in document where sender nickname is located
            message.Source = node?.InnerText;

            node = form.SelectSingleNode("//table/descendant::table/tr[4]/td[2]"); //Position in document where receiver nickname is located
            message.Target = node?.InnerText;

            node = form.SelectSingleNode("//div[@class='boxudaje2']/div[@class='boxudaje3']/descendant::div"); //Position in document where message text is located
            message.Text = node?.InnerText;

            node = form.SelectSingleNode("//table/descendant::table/tr[2]/td[2]");
            message.Subject = node?.InnerText;

            message.Time = httpMessage.TimeStamp.ToString("dd. MM. yyyy HH:mm:ss");

            return message;
        }
        /// <summary>
        /// Creates array of Xchat room messages parsed from HTML document
        /// </summary>
        /// <param name="html">Parsed html document</param>
        /// <param name="exportObject">For referencing XChatSnooperExport from XChat object</param>
        /// <param name="roomId">RoomId that was parsed in GetMessageTypeMethod</param>
        /// <param name="roomNames">Dictionary that has roomId as key and name of room as value</param>
        /// <returns>Array of room messages from Xchat</returns>
        private static XChatRoomMessage[] GetXchatRoomMessages(HtmlDocument html, SnooperExportBase exportObject, string roomId, Dictionary<string, string> roomNames)
        {
            var parsedMessages = new List<XChatRoomMessage>();

            var chatArea = html.DocumentNode.SelectSingleNode("/html/body/script[@language='javascript' and @type='text/javascript']"); //Part of HTML document that contains messages
            if(chatArea != null)
            {
                var messages = GetMessages(chatArea.InnerText); //Array of strings that contain time and message
                for(var i = 0; i < messages.Length; i++)
                {
                    messages[i] = messages[i].Replace("\\\"", "\"");

                    var htmlMessage = new HtmlDocument();
                    htmlMessage.LoadHtml(messages[i]); //Parse message as html document
                    if(htmlMessage.DocumentNode != null)
                    {
                        XChatRoomMessage result;
                        if (htmlMessage.DocumentNode.SelectSingleNode("//font[@class='systemtext']") != null) //Check if message is from system or from user
                            result = ParseXchatRoomSystemMessage(messages[i], exportObject); //System message
                        else
                            result = ParseXchatRoomUserMessage(messages[i], exportObject); //User message
               
                        if (result != null)
                        {
                            string roomName;
                            result.RoomName = roomNames.TryGetValue(roomId, out roomName) ? roomName : "Unknown";
                            parsedMessages.Add(result);
                        }           
                    }
                }
            }

            return parsedMessages.ToArray();
        }
        /// <summary>
        /// Gets messages from part of HTML document that contains them
        /// </summary>
        /// <param name="input">String that contains part of HTML document containing messages</param>
        /// <returns>Array of strings that contain messages</returns>
        private static string[] GetMessages(string input)
        {
            var result = new List<string>();
            bool betweenQuotes = false;
            var startIndex = 0;

            var position = input.IndexOf("Array(");

            if (position == -1)
                return result.ToArray();

            var parsedContent = input.Substring(position + "Array".Length + 1);
            for (var i = 0; i < parsedContent.Length; i++)
            {
                if (parsedContent[i] == '\'')
                {
                    if (betweenQuotes)
                    {
                        var message = parsedContent.Substring(startIndex + 1, i - (startIndex + 1));
                        if (message != "")
                            result.Add(message);
                        betweenQuotes = false;
                    }
                    else
                    {
                        startIndex = i;
                        betweenQuotes = true;
                    }

                }
                else if (parsedContent[i] == ';' && !betweenQuotes)
                    break;
            }
            return result.ToArray();
        }
        /// <summary>
        /// Converts Xchat system message in room chat to object.
        /// </summary>
        /// <param name="message">Message in string</param>
        /// <param name="exportObject">For referencing XChatSnooperExport from XChat object</param>
        /// <returns>Object that represents Xchat system room message</returns>
        private static XChatRoomMessage ParseXchatRoomSystemMessage(string message, SnooperExportBase exportObject)
        {
            message = Regex.Replace(message, "<.*?>", string.Empty); //Remove HTML tags from message

            var positionSpace = message.IndexOf(' '); //Time and text of message are divided by single space
            if(positionSpace == -1)
                return null;

            var time = message.Substring(0, positionSpace);
            var text = message.Substring(positionSpace + 1);
            var result = new XChatRoomMessage(exportObject)
            {
                Time = time,
                Source = "System",
                Text = HttpUtility.HtmlDecode(text)
            };
            return result;
        }
        /// <summary>
        /// Converts Xchat user message in room chat to object.
        /// </summary>
        /// <param name="message">Message in string</param>
        /// <param name="exportObject">For referencing XChatSnooperExport from XChat object</param>
        /// <returns>Object that represents Xchat user room message</returns>
        private static XChatRoomMessage ParseXchatRoomUserMessage(string message, SnooperExportBase exportObject)
        {
            message = Regex.Replace(message, "<.*?>", string.Empty); //Remove HTML tags from message

            var positionSpace = message.IndexOf(' '); //Time and user nickname are divided by single space
            if(positionSpace == -1)
                return null;

            var time = message.Substring(0, positionSpace);

            var position = message.IndexOf(':', positionSpace); //User nickname and message are divided by colon
            if(position == -1)
                return null;

            var sender = message.Substring(positionSpace + 1, position - positionSpace - 1);
            var text = message.Substring(position + 2);

            text = text.Trim();
            if(text != "") //If message does not contain only whitespace
            {
                var result = new XChatRoomMessage(exportObject)
                {
                    Time = time,
                    Source = sender,
                    Text = HttpUtility.HtmlDecode(text)
                };
                return result;
            }
            return null;
        }
        /// <summary>
        /// Parses id and name of the room
        /// </summary>
        /// <param name="html">HTML document to be parsed</param>
        /// <param name="roomNames">Dictionary that contains id as key and name as value</param>
        private static void GetRoomName(HtmlDocument html, Dictionary<string, string> roomNames)
        {
            var node = html.DocumentNode.SelectSingleNode("/html/body/p/strong/a");

            var roomId = node.Attributes["href"].Value; //RoomId is located in href attribute between two brackets
            var openingBracketIndex = roomId.IndexOf('(');
            var closingBracketIndex = roomId.IndexOf(')');
            if(openingBracketIndex != -1 && closingBracketIndex != -1)
            {
                roomId = roomId.Substring(openingBracketIndex + 1, closingBracketIndex - openingBracketIndex - 1); //Get roomID
                roomNames.Add(roomId, node.InnerText.Trim()); //InnerText is room name
            }
        }
        #endregion
    }
}