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
using System.IO;
using System.Linq;
using System.Text;
using Netfox.Backend.Framework.Enums;
using Netfox.Backend.Framework.Interfaces;
using Netfox.Backend.Framework.PDUProviders;
using Netfox.Backend.Framework.Properties;
using Netfox.Backend.Framework.Snoopers;
using Netfox.Backend.Snoopers.SnooperHTTP.Models;
using Netfox.Backend.Snoopers.SnooperLide.Models;
using Netfox.Backend.Snoopers.SnooperLide.Models.Discussions;
using Netfox.Backend.Snoopers.SnooperLide.Models.Photos;
using Netfox.Backend.Snoopers.SnooperLide.Models.Text;
using Netfox.Backend.Snoopers.SnooperLide.Models.Users;
using NUnit.Framework;

namespace Netfox.Backend.Snoopers.SnooperLide.Tests
{
    /// <summary>
    /// Test class for unit testing of Lide.cz reconstruction.
    /// </summary>
    [TestFixture]
    public class SnooperLideTests : SnooperBaseTests
    {
        /// <summary>
        /// Testing of content that is loaded immediately after user joins Discussion room - info about discussion, messages sent in discussion, info about users and their profile photos
        /// </summary>
        [Test]
        public void Test_Lide_discussion_chat_loaded_content()
        {
            var discussionMessagePatterns = new[] //Messages that were sent/received, their timestampes, senders and receivers
            {
                //         Timestamp                   Message Text               ThreadID          UserID
                new [] { "01.03.2016 14:09:00", "Pokus o zachyceni komunikace.", "394483449", "ujiQAliLgBAKB23e"},
                new [] { "01.03.2016 14:09:47", "Lorem ipsum dolor sit amet, consectetuer adipiscing elit.", "394484049", "HEKR4leX1aBnGXo1"},
                new [] { "01.03.2016 14:10:01", "Aenean fermentum risus id tortor.", "394484249", "ujiQAliLgBAKB23e"},
                new [] { "01.03.2016 14:10:04", "Duis viverra diam non justo.", "394484449", "HEKR4leX1aBnGXo1"},
                new [] { "01.03.2016 14:10:17", "Nullam sit amet magna in magna gravida vehicula.", "394484749", "ujiQAliLgBAKB23e"},
                new [] { "01.03.2016 14:10:25", "petr.hromadka007: Excepteur sint occaecat cupidatat non proident.", "394484749", "HEKR4leX1aBnGXo1"},
                new [] { "01.03.2016 14:10:33", "TeaP34ck: Nullam dapibus fermentum ipsum.", "394484749", "ujiQAliLgBAKB23e"}
            };

            var usersPatterns = new[]  //Informations about users
            {
                //            User ID             Nickname
                new [] { "ujiQAliLgBAKB23e", "petr.hromadka007"},
                new [] { "HEKR4leX1aBnGXo1", "TeaP34ck" }
            };

            var photosPatterns = new[]  //Informations about photos of users
            {
                 //        Photo ID       Nickname                    Photo URL                 Width  Height
                new [] { "61167650", "petr.hromadka007", "//sdn.szn.cz/d_37/c_B_C/7t0DSG.jpg", "720", "960"},
                new [] { "61167602", "TeaP34ck", "//sdn.szn.cz/d_37/c_A_D/SP0DSz.jpg", "960", "960" }
            };

            this.FrameworkController.ProcessCapture(new FileInfo(SnoopersPcaps.Default.lide_lide_discussion_loaded_chat_pcapng), this.ProcessCaptureOpContext);
            var conversations = this.L7Conversations.ToArray();

            var pk = File.ReadAllText(PrivateKeys.Default.lide_pk);
            foreach (var conversation in conversations)
               conversation.Key = new CypherKey { ServerPrivateKey = pk};

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, this.ExportOpContext, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperLide() }, this.SnooperExports, this.CurrentTestBaseDirectory, this.ExportOpContext);

            //Select Snooper export from Lide
            var snooperLideExportReference = (from i in this.SnooperExports where (i is LideSnooperExport) select i as LideSnooperExport).ToArray();

            Assert.IsNotNull(snooperLideExportReference);
            Assert.AreEqual(1, snooperLideExportReference.Length); //Snooper export should be only one

            var exportedObjectBases = snooperLideExportReference[0].ExportObjects.ToArray();

            Assert.AreEqual(14, exportedObjectBases.Length); //Total number of exported objects should be 14

            //Cast base object to its real type:
            var discussionInfo = (from i in exportedObjectBases where (i is LideDiscussion) select i as LideDiscussion).ToArray();
            var historyMessages = (from i in exportedObjectBases where (i is LideDiscussionMessage) select i as LideDiscussionMessage).OrderBy(i => i.Timestamp).ToArray();
            //And also remove duplicite objects:
            var users = (from i in exportedObjectBases where (i is LideUser) select i as LideUser).OrderBy(i => i.Nickname).GroupBy(i => i.UserId).Select(i => i.First()).ToArray();
            var photos = (from i in exportedObjectBases where (i is LidePhoto) select i as LidePhoto).OrderBy(i => i.UserNickname).GroupBy(i => i.PhotoId).Select(i => i.First()).ToArray();

            Assert.AreEqual(1, discussionInfo.Length); //Just one object is discussion info
            Assert.AreEqual(7, historyMessages.Length); //Number of preloaded messages in room chat should be 7
            Assert.AreEqual(2, users.Length); //Number of users after deleting duplicates in room should be 2
            Assert.AreEqual(2, photos.Length); //Number of photos of users after deleting duplicates should be 2

            //Testing of objects content:

            Assert.AreEqual(discussionInfo[0].Name, "Netfox FIT"); //Testing info about discussion
            Assert.AreEqual(discussionInfo[0].Description, "Pokus o zachyceni komunikace.");
            Assert.AreEqual(discussionInfo[0].DiscussionId, 91949);

            for (var i = 0; i < historyMessages.Length; i++) //Testing of messages content
            {
                Assert.AreEqual(historyMessages[i].Timestamp, discussionMessagePatterns[i][0]); //Check timestamp
                Assert.AreEqual(historyMessages[i].Text, discussionMessagePatterns[i][1]); //Check message content
                Assert.AreEqual(historyMessages[i].ThreadId, int.Parse(discussionMessagePatterns[i][2])); //Check thread id
                Assert.AreEqual(historyMessages[i].SourceId, discussionMessagePatterns[i][3]); //Check sender Nickname
                Assert.AreEqual(historyMessages[i].Title, "Titulek"); //Check title of message
                Assert.AreEqual(historyMessages[i].DiscussionId, 91949); //Check id of room
                Assert.AreEqual(historyMessages[i].Deleted.ToString(), "False"); //Check if message was deleted or not
            }

            for(var i = 0; i < users.Length; i++) //Testing of users info
            {
                Assert.AreEqual(users[i].UserId, usersPatterns[i][0]);
                Assert.AreEqual(users[i].Nickname, usersPatterns[i][1]);
                Assert.AreEqual(users[i].SexId, 2);
            }

            for(var i = 0; i < photos.Length; i++) //Testing of user's photo
            {
                Assert.AreEqual(photos[i].PhotoId, int.Parse(photosPatterns[i][0]));
                Assert.AreEqual(photos[i].UserNickname, photosPatterns[i][1]);
                Assert.AreEqual(photos[i].Url, photosPatterns[i][2]);
                Assert.AreEqual(photos[i].Width, int.Parse(photosPatterns[i][3]));
                Assert.AreEqual(photos[i].Height, int.Parse(photosPatterns[i][4]));
                Assert.AreEqual(photos[i].ApprovalStatus, "approved");
                Assert.AreEqual(photos[i].LikeCounter, 0);
            }
        }
        /// <summary>
        /// Testing of content that is loaded after user joins private conversation with another user - messages in history, and info about both users
        /// </summary>
        [Test]
        public void Test_Lide_private_chat_loaded_content()
        {
            var usersPatterns = new[]  //Informations about users
            {
                //           User ID             Nickname
                new [] { "ujiQAliLgBAKB23e", "petr.hromadka007"},
                new [] { "HEKR4leX1aBnGXo1", "TeaP34ck" }
            };

            var photosPatterns = new[]  //Informations about photos of users
            {
                //        Photo ID       Nickname                    Photo URL                 Width  Height
                new [] { "61167650", "petr.hromadka007", "//sdn.szn.cz/d_37/c_B_C/7t0DSG.jpg", "720", "960"},
                new [] { "61167602", "TeaP34ck", "//sdn.szn.cz/d_37/c_A_D/SP0DSz.jpg", "960", "960" }
            };

            var privateMessagesPattern = new[] //Mesages in history
            {
                //            Sender ID                Timestamp              Receiver ID            Message text
                new [] { "HEKR4leX1aBnGXo1", "16.02.2016 13:57:14", "ujiQAliLgBAKB23e", "Lorem ipsum dolor sit amet"},
                new [] { "ujiQAliLgBAKB23e", "16.02.2016 13:57:18", "HEKR4leX1aBnGXo1", "Sed vel lectus."},
                new [] { "HEKR4leX1aBnGXo1", "16.02.2016 13:57:24", "ujiQAliLgBAKB23e", "Praesent dapibus."},
                new [] { "ujiQAliLgBAKB23e", "16.02.2016 13:57:30", "HEKR4leX1aBnGXo1", "Phasellus enim erat"},
                new [] { "HEKR4leX1aBnGXo1", "16.02.2016 13:57:32", "ujiQAliLgBAKB23e", "Nunc auctor."},
                new [] { "ujiQAliLgBAKB23e", "16.02.2016 13:57:39", "HEKR4leX1aBnGXo1", "Aenean id metus id velit ullamcorper pulvinar"},
                new [] { "HEKR4leX1aBnGXo1", "16.02.2016 13:57:41", "ujiQAliLgBAKB23e", "Aliquam erat volutpat."},
                new [] { "ujiQAliLgBAKB23e", "16.02.2016 13:57:56", "HEKR4leX1aBnGXo1", "Sed vel lectus." },
                new [] { "HEKR4leX1aBnGXo1", "16.02.2016 13:57:58", "ujiQAliLgBAKB23e", "Aenean vel massa quis mauris vehicula lacinia." }
            };

            this.FrameworkController.ProcessCapture(new FileInfo(SnoopersPcaps.Default.lide_lide_private_loaded_chat_pcapng), this.ProcessCaptureOpContext);
            var conversations = this.L7Conversations.ToArray();

            var pk = File.ReadAllText(PrivateKeys.Default.lide_pk);
            foreach (var conversation in conversations)
                conversation.Key = new CypherKey { ServerPrivateKey = pk };

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, this.ExportOpContext, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperLide() }, this.SnooperExports, this.CurrentTestBaseDirectory, this.ExportOpContext);

            //Select Snooper export from Lide
            var snooperLideExportReference = (from i in this.SnooperExports where (i is LideSnooperExport) select i as LideSnooperExport).ToArray();

            Assert.IsNotNull(snooperLideExportReference);
            Assert.AreEqual(1, snooperLideExportReference.Length); //Snooper export should be only one

            var exportedObjectBases = snooperLideExportReference[0].ExportObjects.ToArray();
            Assert.AreEqual(13, exportedObjectBases.Length); //Total number of exported objects should be 13

            var historyMessages = (from i in exportedObjectBases where (i is LidePrivateMessage) select i as LidePrivateMessage).OrderBy(i => i.Timestamp).ToArray();
            //Cast to its real type and remove duplicite objects
            var users = (from i in exportedObjectBases where (i is LideUser) select i as LideUser).OrderBy(i => i.Nickname).GroupBy(i => i.UserId).Select(i => i.First()).ToArray();
            var photos = (from i in exportedObjectBases where (i is LidePhoto) select i as LidePhoto).OrderBy(i => i.UserNickname).GroupBy(i => i.PhotoId).Select(i => i.First()).ToArray();
            
            Assert.AreEqual(9, historyMessages.Length);
            Assert.AreEqual(2, users.Length);
            Assert.AreEqual(2, photos.Length);

            for (var i = 0; i < users.Length; i++) //Testing of users info
            {
                Assert.AreEqual(users[i].UserId, usersPatterns[i][0]);
                Assert.AreEqual(users[i].Nickname, usersPatterns[i][1]);
                Assert.AreEqual(users[i].SexId, 2);
            }

            for (var i = 0; i < photos.Length; i++) //Testing of user's photo
            {
                Assert.AreEqual(photos[i].PhotoId, int.Parse(photosPatterns[i][0]));
                Assert.AreEqual(photos[i].UserNickname, photosPatterns[i][1]);
                Assert.AreEqual(photos[i].Url, photosPatterns[i][2]);
                Assert.AreEqual(photos[i].Width, int.Parse(photosPatterns[i][3]));
                Assert.AreEqual(photos[i].Height, int.Parse(photosPatterns[i][4]));
                Assert.AreEqual(photos[i].ApprovalStatus, "approved");
                Assert.AreEqual(photos[i].LikeCounter, 0);
            }

            for(var i = 0; i < privateMessagesPattern.Length; i++) //Test of message objects content
            {
                Assert.AreEqual(historyMessages[i].SourceId, privateMessagesPattern[i][0]);
                Assert.AreEqual(historyMessages[i].Timestamp, privateMessagesPattern[i][1]);
                Assert.AreEqual(historyMessages[i].TargetId, privateMessagesPattern[i][2]);
                Assert.AreEqual(historyMessages[i].Text, privateMessagesPattern[i][3]);
            }
        }
        /// <summary>
        /// Testing of content that were sent in real time when traffic was captured
        /// </summary>
        [Test]
        public void Test_Lide_realtime_private_chat()
        {
            var usersPatterns = new[]  //Informations about users
            {
                //           User ID             Nickname
                new [] { "ujiQAliLgBAKB23e", "petr.hromadka007"},
                new [] { "HEKR4leX1aBnGXo1", "TeaP34ck" }
            };

            var photosPatterns = new[]  //Informations about photos of users
            {
                //        Photo ID       Nickname                    Photo URL                 Width  Height
                new [] { "61167650", "petr.hromadka007", "//sdn.szn.cz/d_37/c_B_C/7t0DSG.jpg", "720", "960"},
                new [] { "61167602", "TeaP34ck", "//sdn.szn.cz/d_37/c_A_D/SP0DSz.jpg", "960", "960" }
            };

            var privateMessagesPattern = new[] //Mesages in history
            {
                //            Sender ID            Timestamp             Receiver ID                   Message text
                new [] { "ujiQAliLgBAKB23e", "01.03.2016 14:22:47", "HEKR4leX1aBnGXo1", "Vestibulum fermentum tortor id mi."},
                new [] { "HEKR4leX1aBnGXo1", "01.03.2016 14:22:54", "ujiQAliLgBAKB23e", "Etiam bibendum elit eget erat."},
                new [] { "ujiQAliLgBAKB23e", "01.03.2016 14:23:01", "HEKR4leX1aBnGXo1", "Nullam sit amet magna in magna gravida vehicula." },
                new [] { "HEKR4leX1aBnGXo1", "01.03.2016 14:23:07", "ujiQAliLgBAKB23e", "In laoreet, magna id viverra tincidunt, sem odio bibendum justo." },
                new [] { "ujiQAliLgBAKB23e", "01.03.2016 14:23:13", "HEKR4leX1aBnGXo1", "Nam quis nulla." },
                new [] { "HEKR4leX1aBnGXo1", "01.03.2016 14:23:20", "ujiQAliLgBAKB23e", "Aliquam in lorem sit amet leo accumsan lacinia." }

            };

            this.FrameworkController.ProcessCapture(new FileInfo(SnoopersPcaps.Default.lide_lide_private_realtime_chat_pcapng), this.ProcessCaptureOpContext);
            var conversations = this.L7Conversations.ToArray();

            var pk = File.ReadAllText(PrivateKeys.Default.lide_pk);
            foreach (var conversation in conversations)
                conversation.Key = new CypherKey { ServerPrivateKey = pk };

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, this.ExportOpContext, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperLide() }, this.SnooperExports, this.CurrentTestBaseDirectory, this.ExportOpContext);

            //Select Snooper export from Lide
            var snooperLideExportReference = (from i in this.SnooperExports where (i is LideSnooperExport) select i as LideSnooperExport).ToArray();

            Assert.IsNotNull(snooperLideExportReference);
            Assert.AreEqual(1, snooperLideExportReference.Length); //Snooper export should be only one

            var exportedObjectBases = snooperLideExportReference[0].ExportObjects.ToArray();
            Assert.AreEqual(10, exportedObjectBases.Length); //Total number of exported objects should be 10

            var privateMessages = (from i in exportedObjectBases where (i is LidePrivateMessage) select i as LidePrivateMessage).OrderBy(i => i.Timestamp).ToArray();
            //Cast to its real type and remove duplicite objects
            var users = (from i in exportedObjectBases where (i is LideUser) select i as LideUser).OrderBy(i => i.Nickname).GroupBy(i => i.UserId).Select(i => i.First()).ToArray();
            var photos = (from i in exportedObjectBases where (i is LidePhoto) select i as LidePhoto).OrderBy(i => i.UserNickname).GroupBy(i => i.PhotoId).Select(i => i.First()).ToArray();

            Assert.AreEqual(6, privateMessages.Length);
            Assert.AreEqual(2, users.Length);
            Assert.AreEqual(2, photos.Length);

            for (var i = 0; i < users.Length; i++) //Testing of users info
            {
                Assert.AreEqual(users[i].UserId, usersPatterns[i][0]);
                Assert.AreEqual(users[i].Nickname, usersPatterns[i][1]);
                Assert.AreEqual(users[i].SexId, 2);
            }

            for (var i = 0; i < photos.Length; i++) //Testing of user's photo
            {
                Assert.AreEqual(photos[i].PhotoId, int.Parse(photosPatterns[i][0]));
                Assert.AreEqual(photos[i].UserNickname, photosPatterns[i][1]);
                Assert.AreEqual(photos[i].Url, photosPatterns[i][2]);
                Assert.AreEqual(photos[i].Width, int.Parse(photosPatterns[i][3]));
                Assert.AreEqual(photos[i].Height, int.Parse(photosPatterns[i][4]));
                Assert.AreEqual(photos[i].ApprovalStatus, "approved");
                Assert.AreEqual(photos[i].LikeCounter, 0);
            }

            for (var i = 0; i < privateMessagesPattern.Length; i++) //Test of message objects content
            {
                Assert.AreEqual(privateMessages[i].SourceId, privateMessagesPattern[i][0]);
                Assert.AreEqual(privateMessages[i].Timestamp, privateMessagesPattern[i][1]);
                Assert.AreEqual(privateMessages[i].TargetId, privateMessagesPattern[i][2]);
                Assert.AreEqual(privateMessages[i].Text, privateMessagesPattern[i][3]);
            }
        }
        /// <summary>
        /// Testing of content that were sent in real time when traffic was captured
        /// </summary>
        [Test]
        public void Test_Lide_realtime_discussion_chat()
        {
            var usersPatterns = new[]  //Informations about users
{
                //           User ID             Nickname
                new [] { "ujiQAliLgBAKB23e", "petr.hromadka007"},
                new [] { "HEKR4leX1aBnGXo1", "TeaP34ck" }
            };

            var photosPatterns = new[]  //Informations about photos of users
            {
                //        Photo ID       Nickname                    Photo URL                 Width  Height
                new [] { "61167650", "petr.hromadka007", "//sdn.szn.cz/d_37/c_B_C/7t0DSG.jpg", "720", "960"},
                new [] { "61167602", "TeaP34ck", "//sdn.szn.cz/d_37/c_A_D/SP0DSz.jpg", "960", "960" }
            };

            var discussionMessagesPatterns = new[]
            {
                //              Timestamp                  Message                    ThreadId         Sender
                new [] { "01.03.2016 14:12:16", "Etiam posuere lacus quis dolor.", "394486449", "ujiQAliLgBAKB23e"},
                new [] { "01.03.2016 14:12:21", "Mauris elementum mauris vitae tortor.", "394486549", "HEKR4leX1aBnGXo1"},
                new [] { "01.03.2016 14:12:33", "Vivamus ac leo pretium faucibus.", "394486749", "ujiQAliLgBAKB23e"},
                new [] { "01.03.2016 14:12:35", "Phasellus faucibus molestie nisl. Mauris elementum mauris vitae tortor.", "394486849", "HEKR4leX1aBnGXo1"},
                new [] { "01.03.2016 14:12:45", "TeaP34ck: Morbi leo mi, nonummy eget tristique non, rhoncus non leo.", "394486849", "ujiQAliLgBAKB23e"},
                new [] { "01.03.2016 14:12:49", "petr.hromadka007: Fusce consectetuer risus a nunc.", "394486849", "HEKR4leX1aBnGXo1"}
            };

            this.FrameworkController.ProcessCapture(new FileInfo(SnoopersPcaps.Default.lide_lide_discussion_realtime_chat_pcapng), this.ProcessCaptureOpContext);
            var conversations = this.L7Conversations.ToArray();

            var pk = File.ReadAllText(PrivateKeys.Default.lide_pk);
            foreach (var conversation in conversations)
                conversation.Key = new CypherKey { ServerPrivateKey = pk };

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, this.ExportOpContext, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperLide() }, this.SnooperExports, this.CurrentTestBaseDirectory, this.ExportOpContext);

            //Select Snooper export from Lide
            var snooperLideExportReference = (from i in this.SnooperExports where (i is LideSnooperExport) select i as LideSnooperExport).ToArray();

            Assert.IsNotNull(snooperLideExportReference);
            Assert.AreEqual(1, snooperLideExportReference.Length); //Snooper export should be only one

            var exportedObjectBases = snooperLideExportReference[0].ExportObjects.ToArray();
            Assert.AreEqual(10, exportedObjectBases.Length); //Total number of exported objects should be 10

            var discussionMessages = (from i in exportedObjectBases where (i is LideDiscussionMessage) select i as LideDiscussionMessage).OrderBy(i => i.Timestamp).ToArray();
            //Cast to its real type and remove duplicite objects
            var users = (from i in exportedObjectBases where (i is LideUser) select i as LideUser).OrderBy(i => i.Nickname).GroupBy(i => i.UserId).Select(i => i.First()).ToArray();
            var photos = (from i in exportedObjectBases where (i is LidePhoto) select i as LidePhoto).OrderBy(i => i.UserNickname).GroupBy(i => i.PhotoId).Select(i => i.First()).ToArray();

            Assert.AreEqual(6, discussionMessages.Length);
            Assert.AreEqual(2, users.Length);
            Assert.AreEqual(2, photos.Length);

            for (var i = 0; i < users.Length; i++) //Testing of users info
            {
                Assert.AreEqual(users[i].UserId, usersPatterns[i][0]);
                Assert.AreEqual(users[i].Nickname, usersPatterns[i][1]);
                Assert.AreEqual(users[i].SexId, 2);
            }

            for (var i = 0; i < photos.Length; i++) //Testing of user's photo
            {
                Assert.AreEqual(photos[i].PhotoId, int.Parse(photosPatterns[i][0]));
                Assert.AreEqual(photos[i].UserNickname, photosPatterns[i][1]);
                Assert.AreEqual(photos[i].Url, photosPatterns[i][2]);
                Assert.AreEqual(photos[i].Width, int.Parse(photosPatterns[i][3]));
                Assert.AreEqual(photos[i].Height, int.Parse(photosPatterns[i][4]));
                Assert.AreEqual(photos[i].ApprovalStatus, "approved");
                Assert.AreEqual(photos[i].LikeCounter, 0);
            }

            for (var i = 0; i < discussionMessages.Length; i++) //Testing of messages content
            {
                Assert.AreEqual(discussionMessages[i].Timestamp, discussionMessagesPatterns[i][0]); //Check timestamp
                Assert.AreEqual(discussionMessages[i].Text, discussionMessagesPatterns[i][1]); //Check message content
                Assert.AreEqual(discussionMessages[i].ThreadId, int.Parse(discussionMessagesPatterns[i][2])); //Check thread id
                Assert.AreEqual(discussionMessages[i].SourceId, discussionMessagesPatterns[i][3]); //Check sender Nickname
                Assert.AreEqual(discussionMessages[i].Title, "Titulek"); //Check title of message
                Assert.AreEqual(discussionMessages[i].DiscussionId, 91949); //Check id of room
                Assert.AreEqual(discussionMessages[i].Deleted.ToString(), "False"); //Check if message was deleted or not
            }

        }
    }
}
