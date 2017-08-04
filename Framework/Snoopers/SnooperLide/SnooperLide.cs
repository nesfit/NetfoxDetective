// The MIT License (MIT)
//  
// Copyright (c) 2012-2016 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
// Author(s):
// Jindrich Dudek (mailto:xdudek04@stud.fit.vutbr.cz)
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify,
// merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Castle.Windsor;
using Netfox.Backend.Framework.Models;
using Netfox.Backend.Framework.Snoopers;
using Netfox.Backend.Framework.Snoopers.Models;
using Netfox.Backend.Snoopers.SnooperHTTP.Models;
using Netfox.Backend.Snoopers.SnooperLide.Models;
using Netfox.Backend.Snoopers.SnooperLide.Models.Discussions;
using Netfox.Backend.Snoopers.SnooperLide.Models.Photos;
using Netfox.Backend.Snoopers.SnooperLide.Models.Text;
using Netfox.Backend.Snoopers.SnooperLide.Models.Users;
using Netfox.Backend.Snoopers.SnooperLide.Parser;
using Netfox.Core.Models.Exports;
using Newtonsoft.Json.Linq;
using HttpRequestHeader = Netfox.Backend.Snoopers.SnooperHTTP.Models.HttpRequestHeader;

namespace Netfox.Backend.Snoopers.SnooperLide
{
    /// <summary>
    /// Core class of Lide.cz snooper. Parses HTTP messages, identifies Lide.cz objects and exports them.
    /// </summary>
    public class SnooperLide : SnooperBase
    {
        public SnooperLide() { }
        public SnooperLide(WindsorContainer investigationWindsorContainer, IEnumerable<SnooperExportBase> sourceExports, DirectoryInfo exportDirectory, FcOperationContext opContext) : base(investigationWindsorContainer, sourceExports, exportDirectory, opContext) { }

        #region Overrides of SnooperBase
        public override string Name => "Lidé.cz";

        public override string[] ProtocolNBARName => new[]
        {
            "http",
            "ssl",
            "lidecz"
        };

        public override string Description => "Lidé.cz snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            80,
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new LideSnooperExport();
        protected override SnooperExportBase CreateSnooperExport() { return new LideSnooperExport(); }

        /// <summary>
        /// Starts processing of conversations for identifying and exporting of Lide.cz objects.
        /// </summary>
        protected override void RunBody()
        {
            this.OnConversationProcessingBegin();

            this.ProcessConversation();

            this.OnConversationProcessingEnd();
        }

        protected override void ProcessConversation()
        {
            //Lists used for storing data before export
            LideDiscussion discussion = null;
            var discussionMsgs = new List<LideDiscussionMessage>();
            var users = new List<LideUser>();
            var photos = new List<LidePhoto>();
            var prvtMsgs = new List<LidePrivateMessage>();
            var parsedJsons = new List<string>();

            foreach (var snooperExportBase in this.SourceExports)
            {
                foreach (var exportedObject in snooperExportBase.ExportObjects)
                {
                    this.OnBeforeProtocolParsing();

                    var httpObject = exportedObject as SnooperExportedDataObjectHTTP;
                    if (httpObject == null) // skip exports from invalid snooper
                        break; 

                    var httpMsg = httpObject.Message;
                    var httpContent = httpMsg.HTTPContent;

                    // every message from Lide.cz has content
                    if (httpContent?.Content == null)
                        continue;

                    var httpRequestHeader = httpMsg.HTTPHeader as HttpRequestHeader;
                    if (httpRequestHeader != null) //skip HTTP requests
                        continue;

                    var strippedContent = Encoding.Default.GetString(httpContent.Content);

                    var type = GetMessageType(httpMsg.HTTPHeader, ref strippedContent, parsedJsons);

                    switch(type)
                    {
                        case "discussionLoadedCnt": //Parses info about discussion, messages that were loaded when user joined, etc.
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            ProcessLideDiscussionInfo(strippedContent, this.SnooperExport,ref discussion, discussionMsgs, users, photos);
                            //Export discussion object:
                            discussion.ExportSources.Add(exportedObject.ExportSource);
                            this.SnooperExport.AddExportObject(discussion);

                            foreach (var discussionMessage in discussionMsgs) //Loop through discussion messages
                            {
                                discussionMessage.ExportSources.Add(exportedObject.ExportSource);
                                //Export of discussion message:
                                this.SnooperExport.AddExportObject(discussionMessage);
                            }

                            foreach (var user in users)
                            {
                                user.ExportSources.Add(exportedObject.ExportSource);
                                //Export of users in discussion:
                                this.SnooperExport.AddExportObject(user);
                            }

                            foreach(var photo in photos)
                            {
                                photo.ExportSources.Add(exportedObject.ExportSource);
                                //Export photos of users in discussions:
                                this.SnooperExport.AddExportObject(photo);
                            }

                            this.OnAfterDataExporting();

                            LideClearLists(discussionMsgs, users, photos, ref discussion, null); //Clear used lists a variables
                            break;
                        case "prvtMsgLoadedCnt": //Messages that were already in conversation, when user joined (messages sent in past)
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            GetLideLoadedPrivateMessage(strippedContent, this.SnooperExport, prvtMsgs); //Parse decoded json 

                            foreach(var privateMessage in prvtMsgs) //Export all objects that were deserialized
                            {
                                privateMessage.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(privateMessage);
                            }

                            this.OnAfterDataExporting();

                            LideClearLists(null, null, null, ref discussion, prvtMsgs); //Clear all used lists and variables
                            break;
                        case "userInfo": //Informations about user
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var userInfo = JObject.Parse(strippedContent);
                            var tokenUser = userInfo.SelectToken("$.data.user");

                            GetLideUser(tokenUser, users, this.SnooperExport);
                            GetLidePhoto(tokenUser, photos, users[0].Nickname, this.SnooperExport);

                            foreach (var user in users)
                            {
                                user.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(user);
                            }

                            foreach(var photo in photos)
                            {
                                photo.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(photo);
                            }

                            this.OnAfterDataExporting();

                            LideClearLists(null, users, photos, ref discussion, null); //Clear used lists a variables
                            break;
                        case "contactInfo": //Informations about person that user chatted with
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var contactInfo = JObject.Parse(strippedContent);
                            var tokenContact = contactInfo.SelectToken("$.data.contact");

                            GetLideUser(tokenContact, users, this.SnooperExport);
                            GetLidePhoto(tokenContact, photos, users[0].Nickname, this.SnooperExport);

                            foreach(var user in users)
                            {
                                user.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(user);
                            }

                            foreach(var photo in photos)
                            {
                                photo.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(photo);
                            }

                            this.OnAfterDataExporting();

                            LideClearLists(null, users, photos, ref discussion, null); //Clear used lists a variables
                            break;
                        case "realTimePrivateMessage": //Messages that were sent between two users while capturing network traffic
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            GetLideRealTimePrivateMessages(parsedJsons, this.SnooperExport, prvtMsgs, users, photos);

                            foreach(var msg in prvtMsgs)
                            {
                                msg.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(msg);
                            }

                            foreach(var user in users)
                            {
                                user.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(user);
                            }

                            foreach(var photo in photos)
                            {
                                photo.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(photo);
                            }
                            

                            this.OnAfterDataExporting();

                            LideClearLists(null, users, photos, ref discussion, prvtMsgs); //Clear used lists a variables
                            break;
                        case "realTimeDiscussionMessage": //Messages that were sent in discussion while caprturing network traffic
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            GetLideRealTimeDiscussionMessage(parsedJsons, this.SnooperExport, discussionMsgs, photos, users);

                            foreach(var msg in discussionMsgs)
                            {
                                msg.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(msg);
                            }

                            foreach(var user in users)
                            {
                                user.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(user);
                            }

                            foreach(var photo in photos)
                            {
                                photo.ExportSources.Add(exportedObject.ExportSource);
                                this.SnooperExport.AddExportObject(photo);
                            }

                            this.OnAfterDataExporting();
                            LideClearLists(discussionMsgs, users, photos, ref discussion, null);
                            break;
                        default:
                            //Not object from Lide.cz
                            continue;
                    }

                }
            }
        }

        /// <summary>
        /// Identifies type of object from Lide.cz and modifies variable strippeContent if nescessary
        /// </summary>
        /// <param name="httpHeader">Header of HTTP message for indication of content-type or transfer-encoding</param>
        /// <param name="strippedContent">Variable with message body converted to string. Value of this variable is decoded and changed in this method if nescessary.</param>
        /// <returns>String with type of message</returns>
        private static string GetMessageType(HTTPHeaderBase httpHeader, ref string strippedContent, List<string> parsedJsons)
        {
            var httpResponseHeader = httpHeader as HttpResponseHeader;
            if (httpResponseHeader == null) //Just for sure, cast should be sucessful
                return "";

            List<string> fieldValues; //List with values of Content-Type in HTTP header

            //If HTTP header contains "Content-Type: application/x-base64-frpc"
            if (httpResponseHeader.Fields.TryGetValue("Content-Type", out fieldValues) && fieldValues.Count == 1 && fieldValues[0] == "application/x-base64-frpc")
            {
                var base64FrpcParser = new Base64FrpcParser();
                var decodedBytes = base64FrpcParser.Base64Atob(strippedContent); //Convert from base64-frpc to frpc format
                var decodedJson = base64FrpcParser.Parse(decodedBytes); //Decode frpc to Json
                if (decodedJson == null) //If decoding was not sucessful
                    return "";

                JObject o;
                try
                {
                    o = JObject.Parse(decodedJson);
                }
                catch (Exception)
                {
                    return "";
                }

                if(!string.IsNullOrEmpty(o.SelectToken("$.data.discussion")?.ToString()))
                {
                    strippedContent = decodedJson;
                    return "discussionLoadedCnt";
                }
                if(!string.IsNullOrEmpty(o.SelectToken("$.data.last_events")?.ToString()))
                {
                    strippedContent = decodedJson;
                    return "prvtMsgLoadedCnt";
                }
                if(!string.IsNullOrEmpty(o.SelectToken("$.data.user")?.ToString()))
                {
                    strippedContent = decodedJson;
                    return "userInfo";
                }
                if(!string.IsNullOrEmpty(o.SelectToken("$.data.contact")?.ToString()))
                {
                    strippedContent = decodedJson;
                    return "contactInfo";
                }
            }
            //Messages sent in private or group chat in time of capturing are identified by "Transfer-Encoding: chunked" and "Access-Control-Allow-Origin: https://www.lide.cz" in HTTP header
            else if((httpResponseHeader.Fields.TryGetValue("Transfer-Encoding", out fieldValues) && fieldValues.Count == 1 && fieldValues[0] == "chunked")
                    && (httpResponseHeader.Fields.TryGetValue("Access-Control-Allow-Origin", out fieldValues) && fieldValues.Count == 1 && fieldValues[0] == "https://www.lide.cz"))
            {
                LideParseRealtimeMessagesJsons(strippedContent, parsedJsons); //Divide jsons in signle string to multiple strings

                if (parsedJsons.Count == 0)
                    return "";

                JObject o;
                try //Try to parse first json in message
                {
                    o = JObject.Parse(parsedJsons[0]);
                }
                catch (Exception) { return ""; }

                if (o.SelectToken("$.event.event_type")?.ToString() == "new_discussion_contribution") //Real time messages in discussion have in json attribute "event_type": "new_discussion_contribution"
                    return "realTimeDiscussionMessage";

                if (o.SelectToken("$.event.event_type")?.ToString() == "message_sent") //Real time messages in private chat have in json attribute "event_type": "message_sent"
                    return "realTimePrivateMessage";

                for (var i = 1; i < parsedJsons.Count; i++) //In HTTP message with private realtime conversation may be also jsons with "event_type": "conversation_read", so try another jsons
                {
                    try //Try to parse another jsons
                    {
                        o = JObject.Parse(parsedJsons[i]);
                    }
                    catch (Exception) { return ""; }

                    if(o.SelectToken("$.event.event_type")?.ToString() == "message_sent")
                        return "realTimePrivateMessage";
                }
            }

            return "";
        }
        /// <summary>
        /// Processes json with discussion info that is loaded when user joins discussion
        /// </summary>
        /// <param name="strippedContent">Json decoded from base64-frpc format</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        /// <param name="discussion">Object with discussion info</param>
        /// <param name="discussionMsgs">List where loaded discussion messages are stored after parsing</param>
        /// <param name="users">List where loaded discussion users are stored after parsing</param>
        /// <param name="photos">List where loaded users photos are stored after parsing</param>
        private static void ProcessLideDiscussionInfo(string strippedContent, SnooperExportBase exportObject, ref LideDiscussion discussion, List<LideDiscussionMessage> discussionMsgs, List<LideUser> users, List<LidePhoto> photos)
        {
            int temp;

            var o = JObject.Parse(strippedContent);
            discussion = new LideDiscussion(exportObject) //Parsing of discussion info
            {
                Name = o.SelectToken("$.data.discussion.name")?.ToString(),
                Description = o.SelectToken("$.data.discussion.description")?.ToString(),
                DiscussionId = Int32.TryParse(o.SelectToken("$.data.discussion.id")?.ToString(), out temp) ? temp : (int?)null
            };

            if (o.SelectToken("$.data.contributions") is JArray) //Parsing messages that already were in discussion when user joined
            {
                var messageArray = (JArray)o.SelectToken("$.data.contributions");
                foreach (var message in messageArray)
                {
                    GetLideLoadedDiscussionMessage(message, discussionMsgs, exportObject);

                    if (!(message.SelectToken("$.contributions") is JArray)) //If message contains nested messages
                        continue;
                    var nestedMessageArray = (JArray)message.SelectToken("$.contributions");

                    foreach (var nestedMessage in nestedMessageArray)
                    {
                        GetLideLoadedDiscussionMessage(nestedMessage, discussionMsgs, exportObject);
                    }
                }
            }

            if (string.IsNullOrEmpty(o.SelectToken("$.data.contributing_users")?.ToString())) //If Json does not contain informations about users
                return;

            var contributingUsers = o.SelectToken("$.data.contributing_users");
            foreach (var user in contributingUsers.Children())
            {
                if (!(user is JProperty))
                    continue;

                var usrProperty = (JProperty)user;

                GetLideUser(usrProperty.Value, users, exportObject);
                GetLidePhoto(usrProperty.Value, photos, users[users.Count - 1].Nickname, exportObject);
            }

        }
        /// <summary>
        /// Processes json with messages that were loaded when user joined discussion room
        /// </summary>
        /// <param name="tokenMessage">Token that represents json with messges</param>
        /// <param name="discussionMsgs">List where loaded discussion messages are stored after parsing</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        private static void GetLideLoadedDiscussionMessage(JToken tokenMessage, List<LideDiscussionMessage> discussionMsgs, SnooperExportBase exportObject)
        {
            int tmpInt;
            bool tmpBool;

            var msgObject = new LideDiscussionMessage(exportObject)
            {
                Title = tokenMessage.SelectToken("$.title")?.ToString(),
                Text = tokenMessage.SelectToken("$.content")?.ToString(),
                SourceId = tokenMessage.SelectToken("$.user_id")?.ToString(),
                Timestamp = tokenMessage.SelectToken("$.create_date")?.ToString(),
                Deleted = Boolean.TryParse(tokenMessage.SelectToken("$.deleted")?.ToString(), out tmpBool) ? tmpBool : (bool?)null,
                ThreadId = Int32.TryParse(tokenMessage.SelectToken("$.thread_id")?.ToString(), out tmpInt) ? tmpInt : (int?)null,
                DiscussionId = Int32.TryParse(tokenMessage.SelectToken("$.discussion_id")?.ToString(), out tmpInt) ? tmpInt : (int?)null
            };
            if (msgObject.Timestamp != null)
                msgObject.Timestamp = TimeStampToDateTime(UInt64.Parse(msgObject.Timestamp)).ToString("dd.MM.yyyy HH:mm:ss");
            discussionMsgs.Add(msgObject);
        }
        /// <summary>
        /// Processes json with user info that was loaded when user joined discussion room
        /// </summary>
        /// <param name="tokenUser">Token that represents json with user info</param>
        /// <param name="users">List where loaded discussion users are stored after parsing</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        private static void GetLideUser(JToken tokenUser, List<LideUser> users, SnooperExportBase exportObject)
        {
            int tmpInt;
            var user = new LideUser(exportObject)
            {
                UserId = tokenUser?.SelectToken("$.id")?.ToString(),
                Nickname = tokenUser?.SelectToken("$.nickname")?.ToString(),
                SexId = Int32.TryParse(tokenUser?.SelectToken("$.sex_id")?.ToString(), out tmpInt) ? tmpInt : (int?)null
            };

            users.Add(user);
        }
        /// <summary>
        /// Processes json with user profile photo that was loaded when user joined discussion room
        /// </summary>
        /// <param name="tokenPhoto">Token that represents json with photo</param>
        /// <param name="photos">List where loaded photos are stored after parsing</param>
        /// <param name="nickname">Nickname of user who own photo</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        private static void GetLidePhoto(JToken tokenPhoto, List<LidePhoto> photos, string nickname, SnooperExportBase exportObject)
        {
            int tmpInt;

            var photo = new LidePhoto(exportObject)
            {
                UserNickname = nickname,
                Url = tokenPhoto?.SelectToken("$.photo.url")?.ToString(),
                ApprovalStatus = tokenPhoto?.SelectToken("$.photo.approval_status")?.ToString(),
                PhotoId = Int32.TryParse(tokenPhoto?.SelectToken("$.photo.id")?.ToString(), out tmpInt) ? tmpInt : (int?)null,
                Height = Int32.TryParse(tokenPhoto?.SelectToken("$.photo.height")?.ToString(), out tmpInt) ? tmpInt : (int?)null,
                Width = Int32.TryParse(tokenPhoto?.SelectToken("$.photo.width")?.ToString(), out tmpInt) ? tmpInt : (int?)null,
                LikeCounter = Int32.TryParse(tokenPhoto?.SelectToken("$.photo.like_counter")?.ToString(), out tmpInt) ? tmpInt : (int?)null
            };

            photos.Add(photo);
        }
        /// <summary>
        /// Processes json with messages that were loaded when user joined private chat
        /// </summary>
        /// <param name="strippedContent">Json used for parsing</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        /// <param name="prvtMsgs">List where private messages are stored after parsing</param>
        private static void GetLideLoadedPrivateMessage(string strippedContent, SnooperExportBase exportObject, List<LidePrivateMessage> prvtMsgs)
        {
            var o = JObject.Parse(strippedContent);

            if (!(o.SelectToken("$.data.last_events") is JArray))
                return;

            var lastEvents = (JArray)o.SelectToken("$.data.last_events");

            foreach (var lastEvent in lastEvents)
            {
                if (lastEvent.SelectToken("$.event_type")?.ToString() != "message_sent")
                    continue;

                var privateMessage = new LidePrivateMessage(exportObject)
                {
                    SourceId = lastEvent.SelectToken("$.user_id")?.ToString(),
                    TargetId = lastEvent.SelectToken("$.target_user_id")?.ToString(),
                    Timestamp = lastEvent.SelectToken("$.created")?.ToString(),
                    Text = lastEvent.SelectToken("$.message")?.ToString()
                };

                prvtMsgs.Add(privateMessage);
            }
        }
        /// <summary>
        /// Processes jsons with private messages sent in real time when traffic was captured
        /// </summary>
        /// <param name="parsedJsons">List with jsons in strings</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        /// <param name="prvtMsgs">List where private messages are stored after parsing</param>
        /// <param name="users">List where loaded discussion users are stored after parsing</param>
        /// <param name="photos">List where loaded photos are stored after parsing</param>
        private static void GetLideRealTimePrivateMessages(List<string> parsedJsons, SnooperExportBase exportObject, List<LidePrivateMessage> prvtMsgs, List<LideUser> users, List<LidePhoto> photos)
        {
            foreach (var json in parsedJsons)
            {
                JObject o;

                try { o = JObject.Parse(json); }
                catch(Exception) { continue; }

                if (o.SelectToken("$.event.event_type")?.ToString() != "message_sent") //Skip conversation read notifications
                    continue;

                var message = new LidePrivateMessage(exportObject)
                {
                    SourceId = o.SelectToken("$.event.user_id")?.ToString(),
                    TargetId = o.SelectToken("$.event.target_user_id")?.ToString(),
                    Timestamp = o.SelectToken("$.event.created")?.ToString(),
                    Text = o.SelectToken("$.event.message")?.ToString()
                };
                if (message.Timestamp != null)
                    message.Timestamp = TimeStampToDateTime(UInt64.Parse(message.Timestamp)).ToString("dd.MM.yyyy HH:mm:ss");

                prvtMsgs.Add(message);

                var tokenUser = o.SelectToken("$.event.user");

                var userId = tokenUser.SelectToken("$.id")?.ToString();

                if(users.Any(i => i.UserId == userId)) //If user is already in list
                    continue;

                GetLideUser(tokenUser, users, exportObject);
                GetLidePhoto(tokenUser, photos, users[users.Count - 1].Nickname, exportObject);
            }
            parsedJsons.Clear();
        }
        /// <summary>
        /// Processes jsons with discussion messages sent in real time when traffic was captured
        /// </summary>
        /// <param name="parsedJsons">List with jsons in strings</param>
        /// <param name="exportObject">For referencing LideSnooperExport from Lide object</param>
        /// <param name="discussionMsgs">List where discussion messages are stored after parsing</param>
        /// <param name="photos">List where photos are stored after parsing</param>
        /// <param name="users">List where user info is stored after parsing</param>
        private static void GetLideRealTimeDiscussionMessage(List<string> parsedJsons, SnooperExportBase exportObject, List<LideDiscussionMessage> discussionMsgs, List<LidePhoto> photos, List<LideUser> users)
        {
            int tmpInt;
            foreach (var json in parsedJsons)
            {
                JObject o;

                try { o = JObject.Parse(json); }
                catch (Exception) { continue; }

                var message = new LideDiscussionMessage(exportObject) //Parse message
                {
                    Title = o.SelectToken("$.event.discussion.contributions[0].title")?.ToString(),
                    Text = o.SelectToken("$.event.discussion.contributions[0].content")?.ToString(),
                    SourceId = o.SelectToken("$.event.discussion.contributions[0].user_id")?.ToString(),
                    Timestamp = o.SelectToken("$.event.discussion.contributions[0].create_date")?.ToString(),
                    Deleted = false,
                    ThreadId = Int32.TryParse(o.SelectToken("$.event.discussion.contributions[0].thread_id")?.ToString(), out tmpInt) ? tmpInt : (int?)null,
                    DiscussionId = Int32.TryParse(o.SelectToken("$.event.discussion.contributions[0].discussion_id")?.ToString(), out tmpInt) ? tmpInt : (int?)null
                };
                if (message.Timestamp != null)
                    message.Timestamp = TimeStampToDateTime(UInt64.Parse(message.Timestamp)).ToString("dd.MM.yyyy HH:mm:ss");

                discussionMsgs.Add(message);

                if (string.IsNullOrEmpty(o.SelectToken("$.event.discussion.contributing_users")?.ToString())) //If Json does not contain informations about users
                    return;

                var contributingUsers = o.SelectToken("$.event.discussion.contributing_users");
                foreach (var user in contributingUsers.Children())
                {
                    if (!(user is JProperty))
                        continue;

                    var usrProperty = (JProperty)user;

                    var userId = usrProperty.Value.SelectToken("$.id")?.ToString(); //Find id of current user

                    if (users.Any(i => i.UserId == userId)) //If user is already in list
                        continue;

                    GetLideUser(usrProperty.Value, users, exportObject);
                    GetLidePhoto(usrProperty.Value, photos, users[users.Count - 1].Nickname, exportObject);
                }
            }
            parsedJsons.Clear();
        }
        /// <summary>
        /// Divide string with many json structures to single string for each of them
        /// </summary>
        /// <param name="strippedContent">String with many jsons</param>
        /// <param name="parsedJsons">List where parsed jsons are stored</param>
        private static void LideParseRealtimeMessagesJsons(string strippedContent, List<string> parsedJsons)
        {
            var insideBrackets = false;
            int startIndex = 0;
            int numberOfBrackets = 0;

            for(int i = 0; i < strippedContent.Length; i++)
            {
                if(strippedContent[i] == '{')
                {
                    if(insideBrackets)
                        numberOfBrackets++;
                    else
                    {
                        insideBrackets = true;
                        numberOfBrackets++;
                        startIndex = i;
                    }
                }
                else if(strippedContent[i] == '}')
                {
                    if(!insideBrackets)
                        continue;
                    numberOfBrackets--;
                    if(numberOfBrackets == 0)
                    {
                        insideBrackets = false;
                        var json = strippedContent.Substring(startIndex, i - startIndex + 1);
                        parsedJsons.Add(json);
                    }
                }
            }
        }
        /// <summary>
        /// Clears all lists that were used for storing data
        /// </summary>
        /// <param name="discussionMsgs">List that contains messages sent in discussion</param>
        /// <param name="users">List that contains informations about users</param>
        /// <param name="photos">List that contains profile photos of users</param>
        /// <param name="discussion">Stores informations about discussion</param>
        /// <param name="prvtMsgs">List that cointains messages sent in private chat</param>
        private static void LideClearLists(List<LideDiscussionMessage> discussionMsgs, List<LideUser> users, List<LidePhoto> photos, ref LideDiscussion discussion, List<LidePrivateMessage> prvtMsgs)
        {
            discussionMsgs?.Clear();
            users?.Clear();
            photos?.Clear();
            prvtMsgs?.Clear();
            discussion = null;
        }
        /// <summary>
        /// Converts unix timestamp do DateTime
        /// </summary>
        /// <param name="unixTimeStamp">Timestamp in unix format</param>
        /// <returns>DateTime structure</returns>
        private static DateTime TimeStampToDateTime(ulong unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        #endregion
    }
}