// Copyright (c) 2017 Jan Pluskal, Tomas Bruckner, Matus Dobrotka
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
using System.IO;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using NUnit.Framework;
using SnooperFacebook.Models;
using SnooperFacebook.Models.Files.Group;
using SnooperFacebook.Models.Files.Messenger;
using SnooperFacebook.Models.Statuses;
using SnooperFacebook.Models.Text;

namespace Netfox.Snoopers.SnooperFacebook.Tests
{
    /// <summary>
    /// Test class for unit testing of Facebook reconstruction.
    /// </summary>
    class SnooperFacebookTests : SnooperBaseTests
    {
        [Test]
        public void Test_Facebook_status()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_status_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new global::SnooperFacebook.SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);
            
            //Assert.AreEqual(2, this.SnooperExports.Count);

            FacebookSnooperExport exportedObjects = null;
            
            //this.SnooperExports.Reset();
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            var status = (from item in exportedObjectBases
                          where item.GetType() == typeof(FacebookStatus)
                          select item as FacebookStatus).ToList();
            Assert.AreEqual(1, status.Count);
            Assert.AreEqual(status[0].FacebookStatusTimestamp, "265817278101112658011511995106");
            Assert.AreEqual(status[0].SenderId, "100007717846239");
            Assert.AreEqual(status[0].TargetId, "100007717846239");
            Assert.AreEqual(status[0].StatusText, "Test status");
        }

        [Test]
        public void Test_Facebook_chat()
        {
            var messagePatterns = new[]
            {
                new[] {"1430335728592","765374730","100007717846239","Lorem ipsum dolor sit amet" },
                new[] {"1430335746325","100007717846239","765374730","mea quis doming nemore ad" },
                new[] {"1430335774017","100007717846239","765374730","vis cu dolorem minimum omittam" },
                new[] {"1430335786963","765374730","100007717846239","brute graece platonem usu ea" },
                new[] {"1430335804566","100007717846239","765374730","ius vero consulatu complectitur ad" },
                new[] {"1430335815391","765374730","100007717846239","id usu detracto iracundia euripidis" }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_chat_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new global::SnooperFacebook.SnooperFacebook() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(17, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is FacebookMessage).Cast<FacebookMessage>().OrderBy(it => it.FbTimeStamp).ToArray();

            Assert.AreEqual(6, messages.Length);
            Assert.AreEqual(messages.Length, messagePatterns.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].FbTimeStamp.ToString(), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].SenderId.ToString(), messagePatterns[i][1]);
                Assert.AreEqual(messages[i].TargetId.ToString(), messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Text, messagePatterns[i][3]);
            }
        }

        [Test]
        public void Test_Facebook_groupchat()
        {
            var messagePatterns = new[]
            {
                new[] {"1430427525024","100007717846239","And now here is my secret, a very simple secret: It is only with the heart that one can see rightly; what is essential is invisible to the eye." },
                new[] {"1430427555826","100009331491476","You're beautiful, but you're empty.... No one could die for you" },
                new[] {"1430427603378","765374730","Only the children know what they are looking for." },
                new[] {"1430427624091","100009331491476","For the travelers the stars are guides. For others they are nothing but tiny lights." },
                new[] {"1430427644664","765374730","What makes the desert beautiful,\" said the little prince, \"is that somewhere it hides a well." },
                new[] {"1430427659648","100007717846239","It is such a mysterious place, the land of tears." }
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_group_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(18, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is FacebookGroupMessage).Cast<FacebookGroupMessage>().OrderBy(it => it.FbTimeStamp).ToArray();

            Assert.AreEqual(messages.Length, 6);
            Assert.AreEqual(messages.Length, messagePatterns.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].FbTimeStamp.ToString(), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].SenderId.ToString(), messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Text, messagePatterns[i][2]);
            }
        }

        [Test]
        public void Test_Facebook_photo()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_photo_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var photos = exportedObjectBases.Where(i => i is FacebookMessengerPhoto).Cast<FacebookMessengerPhoto>().OrderBy(it => it as FacebookMessengerPhoto).ToArray();
            Assert.AreEqual(photos.Length, 1);

            Assert.AreEqual(photos[0].Name, "image-1564026470531266");
            Assert.AreEqual(photos[0].SenderId, 100007717846239);
            Assert.AreEqual(photos[0].TargetId, 765374730);
            Assert.AreEqual(photos[0].Url,
                "https://fbcdn-sphotos-h-a.akamaihd.net/hphotos-ak-xpt1/v/t34.0-12/p206x206/11139509_1564026470531266_1032067402_n.jpg?oh=46e9ba071c790b96ca8d4e19b9361f8d&oe=553DC440&__gda__=1430098539_123b1ab04b0700255e483079fd38041a"
            );
        }

        [Test]
        public void Test_Facebook_file()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_file_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var files = exportedObjectBases.Where(i => i is FacebookMessengerFile).Cast<FacebookMessengerFile>().OrderBy(it => it as FacebookMessengerFile).ToArray();
            Assert.AreEqual(files.Length, 1);

            Assert.AreEqual(files[0].FbTimeStamp, 1429992295707);
            Assert.AreEqual(files[0].Name, "test.txt");
            Assert.AreEqual(files[0].SenderId, 100007717846239);
            Assert.AreEqual(files[0].TargetId, 765374730);
            Assert.AreEqual(files[0].Url,
                "https://cdn.fbsbx.com/hphotos-xaf1/v/t59.2708-21/11172381_1564175987182981_2104073377_n.txt/test.txt?oh=f40481ec246f9f62db70b236e9a2fba1&oe=553E5CFE&dl=1"
            );
        }

        [Test]
        public void Test_Facebook_groupphoto()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_groupphoto_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(2, exportedObjectBases.Length);

            var photos = exportedObjectBases.Where(i => i is FacebookMessengerGroupPhoto).Cast<FacebookMessengerGroupPhoto>().OrderBy(it => it as FacebookMessengerGroupPhoto).ToArray();
            Assert.AreEqual(photos.Length, 1);

            Assert.AreEqual(photos[0].FbTimeStamp, 1430516559580);
            Assert.AreEqual(photos[0].GroupName, "NetfoxSnooper");
            Assert.AreEqual(photos[0].Name, "image-1566376120296301");
            Assert.AreEqual(photos[0].Url,
                "https://fbcdn-sphotos-h-a.akamaihd.net/hphotos-ak-xpt1/v/t34.0-12/p206x206/11049523_1566376040296309_2824715817304330990_n.jpg?oh=8f161c5e82242e9fc7ac4332de8bff3d&oe=55464B29&__gda__=1430677806_97fbf8d0ae71d0ee4c65ba3aae04e207"
            );
        }

        [Test]
        public void Test_Facebook_groupfile()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_groupfile_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var files = exportedObjectBases.Where(i => i is FacebookMessengerGroupFile).Cast<FacebookMessengerGroupFile>().OrderBy(it => it as FacebookMessengerGroupFile).ToArray();
            Assert.AreEqual(files.Length, 1);

            Assert.AreEqual(files[0].FbTimeStamp, 1430427730964);
            Assert.AreEqual(files[0].GroupName, "NetfoxSnooper");
            Assert.AreEqual(files[0].Name, "test.txt");
            Assert.AreEqual(files[0].Url,
                "https://cdn.fbsbx.com/hphotos-xft1/v/t59.2708-21/11119012_1566060883661158_1678031127_n.txt/test.txt?oh=b001ba81778c13192e56ab4237282f73&oe=5544E8C4&dl=1"
            );
        }

        [Test]
        public void Test_Facebook_comment()
        {
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.facebook_fb_comment_pcapng)));

            var conversations = this.L7Conversations.ToArray();
            var pk = File.ReadAllText(PcapPath.GetKey(PcapPath.KeyPath.fb_pk));

            foreach (var conversation in conversations)
            {
                conversation.Key = new CypherKey { ServerPrivateKey = pk };
            }

            this.FrameworkController.ExportData(this.AvailableSnoopers, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.AvailableSnoopers, this.SnooperExports, this.CurrentTestBaseDirectory);

            //Assert.AreEqual(this.SnooperExports.Count, 2);

            FacebookSnooperExport exportedObjects = null;
            foreach (var objs in this.SnooperExports.ToArray()) //Get FacebookSnooper exported objects
            {
                if ((exportedObjects = objs as FacebookSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjects);

            var exportedObjectBases = exportedObjects.ExportObjects.ToArray();
            Assert.AreEqual(1, exportedObjectBases.Length);

            var comments = exportedObjectBases.Where(i => i is FacebookComment).Cast<FacebookComment>().OrderBy(it => it as FacebookComment).ToArray();

            Assert.AreEqual(comments[0].Text, "Komentar");
            Assert.AreEqual(comments[0].FbTimeStamp, 1429947212);
            Assert.AreEqual(comments[0].SenderId, 100007717846239);
        }
    }
}