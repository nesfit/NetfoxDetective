// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using Castle.Windsor;
using Netfox.Core.Models.Exports;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;
using Netfox.SnooperFacebook.Models;
using Netfox.SnooperFacebook.Models.Events;
using Netfox.SnooperFacebook.Models.Files.Group;
using Netfox.SnooperFacebook.Models.Files.Messenger;
using Netfox.SnooperFacebook.Models.Statuses;
using Netfox.SnooperFacebook.Models.Text;
using Netfox.SnooperHTTP.Models;
using Newtonsoft.Json.Linq;
using HttpRequestHeader = Netfox.SnooperHTTP.Models.HttpRequestHeader;

namespace Netfox.SnooperFacebook
{
    /// <summary>
    /// Core class of Facebook snooper. Parses HTTP messages, identifies Facebook objects and exports them.
    /// </summary>
    public class SnooperFacebook : SnooperBase
    {
        //public SnooperFacebook(SelectedConversations conversations, string exportDirectory, FrameworkController2.SnooperExportHandler snooperExportHandler) : base(conversations, exportDirectory, snooperExportHandler) {}
        public SnooperFacebook() { }
        public SnooperFacebook(WindsorContainer investigationWindsorContainer, IEnumerable<SnooperExportBase> sourceExports, DirectoryInfo exportDirectory) : base(investigationWindsorContainer,sourceExports, exportDirectory) { }

        #region Overrides of SnooperBase
        public override string Name => "Facebook";

        public override string[] ProtocolNBARName => new[]
        {
            "http",
            "ssl",
            "facebook"
        };

        public override string Description => "Facebook snooper";

        public override int[] KnownApplicationPorts => new[]
        {
            80,
            443
        };

        public override SnooperExportBase PrototypExportObject { get; } = new FacebookSnooperExport();

        /// <summary>
        /// Starts processing of conversations for identifying and exporting of Facebook objects.
        /// </summary>
        protected override void RunBody()
        {
            this.OnConversationProcessingBegin();

            this.ProcessConversation();

            this.OnConversationProcessingEnd();
        }

        /// <summary>
        /// Identificates and exports Facebook objects from conversation.
        /// </summary>
        protected override void ProcessConversation()
        {
            // for every snooperExport 
            foreach (var snooperExportBase in this.SourceExports)
            {
                foreach (var exportedObject in snooperExportBase.ExportObjects)
                {
                    this.OnBeforeProtocolParsing();

                    var httpObject = exportedObject as SnooperExportedDataObjectHTTP;
                    if(httpObject == null) break; // for exports from invalid snooper
                   
                    var msg = httpObject.Message;
                    var httpContent = msg.HTTPContent;

                    // every facebook message has content
                    if (httpContent?.Content == null)
                    {
                        continue;
                    }

                    // check for facebook status
                    var header = msg.HTTPHeader as HttpRequestHeader;
                    if (header != null)
                    {
                        var requestUri = header.RequestURI;

                        if (requestUri.Contains("updatestatus.php"))
                        {
                            this.OnAfterProtocolParsing();

                            this.OnBeforeDataExporting();

                            var parsedStatus = GetFacebookStatus(msg, this.SnooperExport);
                            parsedStatus.ExportSources.Add(exportedObject.ExportSource);
                            // export
                            this.SnooperExport.AddExportObject(parsedStatus);
                            this.OnAfterDataExporting();

                            continue;
                        }
                    }

                    // strip anti json hijack protection from json
                    var strippedContent = Encoding.Default.GetString(httpContent.Content).Substring(9);
                    try
                    {
                        JObject.Parse(strippedContent);
                    }
                    catch (Exception)
                    {
                        // not a facebook message
                        continue;
                    }

                    var msgJson = JObject.Parse(strippedContent);

                    var index = 0;
                    var type = GetMessageType(msgJson, ref index);

                    // facebook messenger message
                    switch (type)
                    {
                        case "message":
                            this.OnAfterProtocolParsing();

                            this.OnBeforeDataExporting();

                            var facebookMessage = GetFacebookMessage(msgJson, this.SnooperExport, index);
                            facebookMessage.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(facebookMessage);
                            this.OnAfterDataExporting();
                            break;

                        case "comment":
                            this.OnAfterProtocolParsing();

                            this.OnBeforeDataExporting();

                            var facebookComment = GetFacebookComment(msgJson, this.SnooperExport, index);
                            facebookComment.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(facebookComment);
                            this.OnAfterDataExporting();
                            break;

                        case "group":
                            this.OnAfterProtocolParsing();

                            this.OnBeforeDataExporting();

                            var facebookGroupMessage = GetFacebookGroupMessage(msgJson, this.SnooperExport, index);
                            facebookGroupMessage.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(facebookGroupMessage);
                            this.OnAfterDataExporting();
                            break;

                        case "file":
                            this.OnAfterProtocolParsing();

                            var attachmentType = (string)msgJson["ms"][index]["message"]["attachments"][0]["attach_type"];
                            switch (attachmentType)
                            {
                                case "photo":
                                    this.OnBeforeDataExporting();

                                    var parsedPhoto = GetFacebookMessengerPhoto(msgJson, this.SnooperExport, index);
                                    parsedPhoto.ExportSources.Add(exportedObject.ExportSource);

                                    // export
                                    this.SnooperExport.AddExportObject(parsedPhoto);
                                    this.OnAfterDataExporting();
                                    break;

                                case "file":
                                    this.OnBeforeDataExporting();

                                    var facebookFile = GetFacebookMessengerFile(msgJson, this.SnooperExport, index);
                                    facebookFile.ExportSources.Add(exportedObject.ExportSource);

                                    // export
                                    this.SnooperExport.AddExportObject(facebookFile);
                                    this.OnAfterDataExporting();
                                    break;

                                // Unknown facebook message filetype
                                default:
                                    Debug.WriteLine(@"Unknown facebook message filetype");
                                    this.SnooperExport.Reports.Add(new ExportReport()
                                    {
                                        Level = ExportReport.ReportLevel.Error,
                                        Description = "Unknown facebook message filetype"
                                    });
                                    break;
                            }

                            var fileText = (string)msgJson["ms"][index]["message"]["body"];

                            // check if file containted message
                            if (!string.IsNullOrEmpty(fileText))
                            {
                                this.OnBeforeDataExporting();

                                var message = GetFacebookMessage(msgJson, this.SnooperExport, index);
                                message.ExportSources.Add(exportedObject.ExportSource);

                                // export
                                this.SnooperExport.AddExportObject(message);
                                this.OnAfterDataExporting();
                            }
                            break;

                        case "groupfile":
                            this.OnAfterProtocolParsing();

                            var attachmentGroupType = (string)msgJson["ms"][index]["message"]["attachments"][0]["attach_type"];
                            switch (attachmentGroupType)
                            {
                                case "photo":
                                    this.OnBeforeDataExporting();

                                    var parsedPhoto = GetFacebookMessengerGroupPhoto(msgJson, this.SnooperExport, index);
                                    parsedPhoto.ExportSources.Add(exportedObject.ExportSource);

                                    // export
                                    this.SnooperExport.AddExportObject(parsedPhoto);
                                    this.OnAfterDataExporting();
                                    break;

                                case "file":
                                    this.OnBeforeDataExporting();

                                    var facebookFile = GetFacebookMessengerGroupFile(msgJson, this.SnooperExport, index);
                                    facebookFile.ExportSources.Add(exportedObject.ExportSource);

                                    // export
                                    this.SnooperExport.AddExportObject(facebookFile);
                                    this.OnAfterDataExporting();
                                    break;


                                // Unknown attachment type
                                default:
                                    var report = new ExportReport
                                    {
                                        Description = "Unknown attachemnt type in Facebook Groupfile",
                                        Level = ExportReport.ReportLevel.Error
                                        
                                    };
                                    this.SnooperExport.Reports.Add(report);
                                    break;
                            }

                            var groupText = (string)msgJson["ms"][index]["message"]["body"];

                            if (!string.IsNullOrEmpty(groupText))
                            {
                                this.OnBeforeDataExporting();

                                var groupMessage = GetFacebookGroupMessage(msgJson, this.SnooperExport, index);
                                groupMessage.ExportSources.Add(exportedObject.ExportSource);

                                // export
                                this.SnooperExport.AddExportObject(groupMessage);
                                this.OnAfterDataExporting();
                            }
                            break;

                        case "sent_push":
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var sentEvent = GetFacebookEventSentPush(msgJson, this.SnooperExport, index);
                            sentEvent.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(sentEvent);
                            this.OnAfterDataExporting();
                            break;

                        case "read_receipt":
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var readReceipt = GetFacebookEventReadReceipt(msgJson, this.SnooperExport, index);
                            readReceipt.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(readReceipt);
                            this.OnAfterDataExporting();
                            break;

                        case "read":
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var readEvent = GetFacebookEventRead(msgJson, this.SnooperExport, index);
                            readEvent.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(readEvent);
                            this.OnAfterDataExporting();
                            break;

                        case "typing":
                            this.OnAfterProtocolParsing();
                            this.OnBeforeDataExporting();

                            var typingEvent = GetFacebookEventTyping(msgJson, this.SnooperExport, index);
                            typingEvent.ExportSources.Add(exportedObject.ExportSource);

                            // export
                            this.SnooperExport.AddExportObject(typingEvent);
                            this.OnAfterDataExporting();
                            break;

                        default:
                            // not a facebook message
                            continue;
                    }
                }
            }
        }

        /// <summary>
        /// Creates Facebook file object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed file from Facebook group conversation.</returns>
        private static FacebookMessengerGroupFile GetFacebookMessengerGroupFile(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var messengerGroupFile = new FacebookMessengerGroupFile(exportObject)
            {
                Url = (string)msgJson["ms"][index]["message"]["attachments"][0]["url"],
                SourceType = (string)msgJson["ms"][index]["message"]["source"],
                SenderId = (ulong)msgJson["ms"][index]["message"]["sender_fbid"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["message"]["timestamp"],
                Name = (string)msgJson["ms"][index]["message"]["attachments"][0]["name"],
                GroupName = (string)msgJson["ms"][index]["message"]["group_thread_info"]["name"]
            };

            if (!(msgJson["ms"][index]["message"]["group_thread_info"]["participant_ids"] is JArray))
            {
                return messengerGroupFile;
            }

            var temp = (JArray)msgJson["ms"][index]["message"]["group_thread_info"]["participant_ids"];
            foreach (var item in temp)
            {
                messengerGroupFile.ParticipantsId.Add((ulong)item);
            }
            return messengerGroupFile;
        }

        /// <summary>
        /// Creates Facebook photo object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed photo from Facebook group conversation.</returns>
        private static FacebookMessengerGroupPhoto GetFacebookMessengerGroupPhoto(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var messengerGroupPhoto = new FacebookMessengerGroupPhoto(exportObject)
            {
                Url = (string)msgJson["ms"][index]["message"]["attachments"][0]["preview_url"],
                SourceType = (string)msgJson["ms"][index]["message"]["source"],
                SenderId = (ulong)msgJson["ms"][index]["message"]["sender_fbid"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["message"]["timestamp"],
                Name = (string)msgJson["ms"][index]["message"]["attachments"][0]["name"],
                GroupName = (string)msgJson["ms"][index]["message"]["group_thread_info"]["name"]
            };

            if (!(msgJson["ms"][index]["message"]["group_thread_info"]["participant_ids"] is JArray))
            {
                return messengerGroupPhoto;
            }

            var temp = (JArray)msgJson["ms"][index]["message"]["group_thread_info"]["participant_ids"];
            foreach (var item in temp)
            {
                messengerGroupPhoto.ParticipantsId.Add((ulong)item);
            }
            return messengerGroupPhoto;
        }

        /// <summary>
        /// Creates Facebook group message by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed group message from Facebook conversation.</returns>
        private static FacebookGroupMessage GetFacebookGroupMessage(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var facebookGroupMessage = new FacebookGroupMessage(exportObject)
            {
                Text = (string)msgJson["ms"][index]["message"]["body"],
                SourceType = (string)msgJson["ms"][index]["message"]["source"],
                SenderId = (ulong)msgJson["ms"][index]["message"]["sender_fbid"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["message"]["timestamp"],
                GroupName = (string)msgJson["ms"][index]["message"]["group_thread_info"]["name"]
            };

            if (!(msgJson["ms"][index]["message"]["group_thread_info"]["participant_ids"] is JArray))
            {
                return facebookGroupMessage;
            }
            var temp = (JArray)msgJson["ms"][index]["message"]["group_thread_info"]["participant_ids"];
            foreach (var item in temp)
            {
                facebookGroupMessage.ParticipantsId.Add((ulong)item);
            }
            return facebookGroupMessage;
        }

        /// <summary>
        /// Identifies type of Facebook object for future parsing. 
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed group message from Facebook conversation.</returns>
        private static string GetMessageType(JObject msgJson, ref int index)
        {
            if (!(msgJson["ms"] is JArray))
            {
                return string.Empty;
            }

            var temp = (JArray)msgJson["ms"];

            foreach (var item in temp)
            {
                if (item["message"] is JObject)
                {
                    if (item["message"]["has_attachment"] != null)
                    {
                        var hasAttachments = (bool)msgJson["ms"][index]["message"]["has_attachment"];

                        if (item["message"]["group_thread_info"] != null && item["message"]["group_thread_info"].HasValues)
                        {
                            return hasAttachments ? "groupfile" : "group";
                        }

                        return hasAttachments ? "file" : "message";
                    }
                }

                if (item["comments"] != null)
                {
                    return "comment";
                }

                if (item["event"] != null)
                {
                    var eventType = (string)item["event"];
                    switch (eventType)
                    {
                        case "sent_push":
                            return "sent_push";

                        case "read_receipt":
                            return "read_receipt";

                        case "read":
                            return "read";
                    }
                }

                // typing event has different structure than normal event
                if (item["type"] != null)
                {
                    var type = (string)item["type"];
                    if (type == "ttyp")
                    {
                        return "typing";
                    }
                }
                index++;
            }

            // not Facebook object
            return string.Empty;
        }

        /// <summary>
        /// Creates Facebook message by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed message from Facebook conversation.</returns>
        private static FacebookMessage GetFacebookMessage(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedMessage = new FacebookMessage(exportObject)
            {
                Text = (string)msgJson["ms"][index]["message"]["body"],
                SourceType = (string)msgJson["ms"][index]["message"]["source"],
                SenderId = (ulong)msgJson["ms"][index]["message"]["sender_fbid"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["message"]["timestamp"]
            };
            var userId = (ulong)msgJson["u"];
            parsedMessage.TargetId = userId == parsedMessage.SenderId ? (ulong)msgJson["ms"][index]["message"]["other_user_fbid"] : userId;
            return parsedMessage;
        }

        /// <summary>
        /// Creates Facebook comment by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed comment from HTTP content.</returns>
        private static FacebookComment GetFacebookComment(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedComment = new FacebookComment(exportObject)
            {
                Text = (string)msgJson["ms"][index]["comments"][0]["body"]["text"],
                SenderId = (ulong)msgJson["ms"][index]["comments"][0]["author"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["comments"][0]["timestamp"]["time"]
            };
            return parsedComment;
        }

        /// <summary>
        /// Creates Facebook status by parsing content of HTTP message.
        /// </summary>
        /// <param name="msg">Content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <returns>Parsed comment from HTTP content.</returns>
        private static FacebookStatus GetFacebookStatus(HTTPMsg msg, SnooperExportBase exportObject)
        {
            var httpContent = msg.HTTPContent.Content;
            var strippedContent = Encoding.Default.GetString(httpContent);

            // parses urlencoded data
            var parsedUrl = HttpUtility.ParseQueryString(strippedContent);

            var status = new FacebookStatus(exportObject)
            {
                FacebookStatusTimestamp = parsedUrl["ttstamp"],
                SenderId = parsedUrl["__user"],
                TargetId = parsedUrl["xhpc_targetid"],
                StatusText = parsedUrl["xhpc_message_text"]
            };

            return status;
        }

        /// <summary>
        /// Creates Facebook photo object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed photo from Facebook conversation.</returns>
        private static FacebookMessengerPhoto GetFacebookMessengerPhoto(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedPhoto = new FacebookMessengerPhoto(exportObject)
            {
                Url = (string)msgJson["ms"][index]["message"]["attachments"][0]["preview_url"],
                SourceType = (string)msgJson["ms"][index]["message"]["source"],
                SenderId = (ulong)msgJson["ms"][index]["message"]["sender_fbid"],
                Name = (string)msgJson["ms"][index]["message"]["attachments"][0]["name"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["message"]["timestamp"]
            };
            var userId = (ulong)msgJson["u"];
            parsedPhoto.TargetId = userId == parsedPhoto.SenderId ? (ulong)msgJson["ms"][index]["message"]["other_user_fbid"] : userId;

            return parsedPhoto;
        }

        /// <summary>
        /// Creates Facebook file object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed file from Facebook conversation.</returns>
        private static FacebookMessengerFile GetFacebookMessengerFile(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var messengerFile = new FacebookMessengerFile(exportObject)
            {
                Url = (string)msgJson["ms"][index]["message"]["attachments"][0]["url"],
                SourceType = (string)msgJson["ms"][index]["message"]["source"],
                SenderId = (ulong)msgJson["ms"][index]["message"]["sender_fbid"],
                Name = (string)msgJson["ms"][index]["message"]["attachments"][0]["name"],
                FbTimeStamp = (ulong)msgJson["ms"][index]["message"]["timestamp"]
            };
            var userId = (ulong)msgJson["u"];
            messengerFile.TargetId = userId == messengerFile.SenderId ? (ulong)msgJson["ms"][index]["message"]["other_user_fbid"] : userId;

            return messengerFile;
        }

        /// <summary>
        /// Creates Facebook event object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed event object from Facebook conversation.</returns>
        private static FacebookEventSentPush GetFacebookEventSentPush(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedToken = msgJson["ms"][index];
            var fbEvent = new FacebookEventSentPush(exportObject)
            {
                EventType = (string)parsedToken["event"],
                UserId = (ulong)msgJson["u"],
                TargetId = (ulong)parsedToken["to"],
                Timestamp = (ulong)parsedToken["time"]
            };

            return fbEvent;
        }

        /// <summary>
        /// Creates Facebook event object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed event object from Facebook conversation.</returns>
        private static FacebookEventReadReceipt GetFacebookEventReadReceipt(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedToken = msgJson["ms"][index];
            var fbEvent = new FacebookEventReadReceipt(exportObject)
            {
                EventType = (string)parsedToken["event"],
                UserId = (ulong)msgJson["u"],
                Timestamp = (ulong)parsedToken["time"],
                ReaderId = (ulong)parsedToken["reader"]
            };

            if (parsedToken["thread_fbid"] != null)
            {
                fbEvent.ThreadId = (ulong)parsedToken["thread_fbid"];
            }
            return fbEvent;
        }

        /// <summary>
        /// Creates Facebook event object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed event object from Facebook conversation.</returns>
        private static FacebookEventRead GetFacebookEventRead(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedToken = msgJson["ms"][index];
            var fbEvent = new FacebookEventRead(exportObject)
            {
                EventType = (string)parsedToken["event"],
                UserId = (ulong)msgJson["u"],
                Timestamp = (ulong)parsedToken["timestamp"]
            };

            var threadIds = parsedToken["thread_fbids"] as JArray;
            if (threadIds != null)
            {
                foreach (var threadId in threadIds)
                {
                    fbEvent.ThreadIds.Add((ulong)threadId);
                }
            }

            if (!(parsedToken["other_user_fbids"] is JArray))
            {
                return fbEvent;
            }

            var targetIds = (JArray)parsedToken["other_user_fbids"];
            foreach (var targetId in targetIds)
            {
                fbEvent.ThreadIds.Add((ulong)targetId);
            }

            return fbEvent;
        }

        /// <summary>
        /// Creates Facebook event object by parsing JSON data from content of HTTP message.
        /// </summary>
        /// <param name="msgJson">JSON data from content of HTTP message.</param>
        /// <param name="exportObject">For referencing FacebookSnooperExport from Facebook object.</param>
        /// <param name="index">Index to array inside JSON data where we can relevant data.</param>
        /// <returns>Parsed event object from Facebook conversation.</returns>
        private static FacebookEventTyping GetFacebookEventTyping(JObject msgJson, SnooperExportBase exportObject, int index)
        {
            var parsedToken = msgJson["ms"][index];
            var fbEvent = new FacebookEventTyping(exportObject)
            {
                EventType = (string)parsedToken["type"],
                UserId = (ulong)msgJson["u"],
                ThreadId = (ulong)parsedToken["thread_fbid"],
                SenderId = (ulong)parsedToken["from"]
            };
            return fbEvent;
        }

        protected override SnooperExportBase CreateSnooperExport() { return new FacebookSnooperExport(); }

        #endregion
    }
}
