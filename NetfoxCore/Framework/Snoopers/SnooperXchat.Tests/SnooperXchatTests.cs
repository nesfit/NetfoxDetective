// Copyright (c) 2017 Jan Pluskal, Matus Dobrotka
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
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.Framework.Models.Snoopers;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperXchat.Models;
using Netfox.Snoopers.SnooperXchat.Models.Text;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperXchat.Tests
{
    /// <summary>
    /// Test class for unit testing of Xchat reconstruction.
    /// </summary>
    [TestFixture]
    public class SnooperXchatTests : SnooperBaseTests
    {
        public SnooperXchatTests()
        {
            this.SnoopersToUse = new List<ISnooper> { new Netfox.Snoopers.SnooperHTTP.SnooperHTTP(), new SnooperXchat() };
        }
        [Test]
        public void Test_Xchat_private_messages()
        {
            var privateMessagePatterns = new[] //Messages that were sent/received, their timestampes, senders and receivers
             {
                new [] {"notender", "Ardus","11. 02. 2016 13:13:30", "" ,"Phasellus rhoncus."},
                new [] {"Ardus", "notender", "11. 02. 2016 13:13:36", "Re: ","Etiam dictum tincidunt diam."},
                new [] {null, "Všem","11. 02. 2016 13:13:56", "Upozornění na podvodné vzkazy" ,"Ahoj,\nobjevily se informace, že v poslední době někdo\nrozesílá podvodné vzkazy typu:\n\n\"Vážený uživateli xchat.cz, Vašemu xchat ID\nvyprší platnost xx.yy.zzzz.Jestli si ho chcete\nprodloužit pošlete SMS na číslo ccccccc ve tvaru\nabc xxxxxxx , tato SMS je bezplatná platí ji Váš\noperátor, který se dohodl na spolupráci s\nxchat.cz.\"\n\nUpozorňujeme, že žádná dohoda s operátory\nneexistuje a SMS zprávy jsou zpoplatněny vysokou\nfinanční částkou.\n\nDále platí, že účtu nevyprší platnost žádným\njiným způsobem, než dlouhodobou neaktivitou a\nnikdy nebudeme po uživateli vyžadovat sdělení\naktuálního hesla.\n\nPokud tedy dostanete vzkaz (či email) podobného\ntypu, nereagujte na něj nijak a smažte jej."},
                new [] {"notender", "Ardus", "11. 02. 2016 13:14:00", "Re (1):", "Phasellus rhoncus."},
                new [] {"Ardus", "notender","11. 02. 2016 13:14:08", "Re (2):", "Ut tempus purus at lorem. "},
                new [] {"notender", "Ardus", "11. 02. 2016 13:14:53", "Re (3):", "Nullam faucibus mi quis velit."},
                new [] {"Ardus", "notender","11. 02. 2016 13:14:59", "Re (4):", "Ut tempus purus at lorem. "},
                new [] {"notender", "Ardus", "11. 02. 2016 13:15:16", "Re (5):", "Morbi leo mi, nonummy eget tristique non, rhoncus\nnon leo." },
                new [] {"Ardus", "notender","11. 02. 2016 13:15:22", "Re (6):", "Curabitur bibendum justo non orci."},
                new [] {"notender", "Ardus", "11. 02. 2016 13:15:51", "Re (7):", "Aenean id metus id velit ullamcorper pulvinar."},
                new [] {"Ardus", "notender","11. 02. 2016 13:15:56", "Re (8):", "Cras pede libero, dapibus nec, pretium sit amet, tempor quis."},
                new [] {"notender", "Ardus", "11. 02. 2016 13:16:30", "Re (9):", "Aliquam erat volutpat."}
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.xchat_xchat_chat_pcapng)));
            var conversations = this.L7Conversations.ToArray();

            this.FrameworkController.ExportData(this.SnoopersToUse, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(this.SnoopersToUse, this.SnooperExports, this.CurrentTestBaseDirectory);

            XChatSnooperExport exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get XchatSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as XChatSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(12, exportedObjectBases.Length); //Total number of exported objects should be 12

            var messages = exportedObjectBases.Where(i => i is XChatPrivateMessage).Cast<XChatPrivateMessage>().OrderBy(it => it.Time).ToArray();
            Assert.AreEqual(12, messages.Length); //Every exported object should be private message
            Assert.AreEqual(privateMessagePatterns.Length, messages.Length); //Both arrays should have same length

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].Source, privateMessagePatterns[i][0]);
                Assert.AreEqual(messages[i].Target, privateMessagePatterns[i][1]);
                Assert.AreEqual(messages[i].Time, privateMessagePatterns[i][2]);
                Assert.AreEqual(messages[i].Subject, privateMessagePatterns[i][3]);
                Assert.AreEqual(messages[i].Text, privateMessagePatterns[i][4]);
            }
        }
        [Test]
        public void Test_Xchat_room_messages()
        {
            var roomMessagePatterns = new[] //Messages that were sent/received, their timestampes, senders and receivers
             {
                new [] {"notender", "14:14:58", "Nam quis nulla."},
                new [] {"Certifikace", "14:15:01", "Chceš modrou hvězdičku zdarma? Chceš hlavičku místo panáčka? Chceš, aby všichni věděli, že o sobě nelžeš?"},
                new [] {"Ardus", "14:15:11", "Lorem ipsum"},
                new [] {"notender", "14:15:24", "Aliquam ornare wisi eu metus."},
                new [] {"Ardus", "14:15:30", "et, suscipit a, interdum id, felis. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore ma"},
                new [] {"notender", "14:15:47", "Etiam commodo dui eget wisi."},
                new [] {"Ardus", "14:16:04", "Integer lacinia."},
                new [] {"notender", "14:16:07", "Proin mattis lacinia justo."},
                new [] {"notender", "14:16:20", "Mauris metus."},
                new [] {"Ardus", "14:16:21", "Praesent vitae arcu tempor neque lacinia pretium."}
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.xchat_xchat_room_chat_pcapng)));
            var conversations = this.L7Conversations.ToArray();

            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);
            this.FrameworkController.ExportData(new List<ISnooper> { new SnooperXchat() }, this.SnooperExports, this.CurrentTestBaseDirectory);

            XChatSnooperExport exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get XchatSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as XChatSnooperExport) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(10, exportedObjectBases.Length); //Total number of exported objects should be 10

            var messages = exportedObjectBases.Where(i => i is XChatRoomMessage).Cast<XChatRoomMessage>().OrderBy(it => it.Time).ToArray();
            Assert.AreEqual(10, messages.Length); //Every exported object should be private message
            Assert.AreEqual(roomMessagePatterns.Length, messages.Length); //Both arrays should have same length

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].Source, roomMessagePatterns[i][0]);
                Assert.AreEqual(messages[i].Time, roomMessagePatterns[i][1]);
                Assert.AreEqual(messages[i].Text, roomMessagePatterns[i][2]);
                Assert.AreEqual(messages[i].RoomName, "Unknown");
            }
        }
        /*  [Test]
          public void Test_demonstrate_http_header_missing()
          {
              this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(xchat_xchat_room_chat_pcapng));
              var conversations = this.L7Conversations.ToArray();

              foreach (var conversation in conversations) { conversation.Key = null; }

              this.FrameworkController.ExportData(new List<ISnooper> { new SnooperHTTP.SnooperHTTP() }, conversations, this.CurrentTestBaseDirectory, true);

              //Pretypovani vsech exportu na typ SnooperExportHTTP pomoci LINQ
              var snooperExportReference = (from i in this.SnooperExports where (i is SnooperExportHTTP) select i as SnooperExportHTTP).ToArray();

              //Vybrani pouze tech exportu obsahujici HTTP zpravy
              var snooperExportedMessagesReference = snooperExportReference.Where(i => i.ExportObjects.Count != 0).ToArray();

              foreach(var messagesExport in snooperExportedMessagesReference) //Projizdeni jednotlivych exportu
              {
                  foreach(var message in messagesExport.ExportObjects) //Projizdeni zprav v ramci exportu
                  {
                      var messageHTTP = message as SnooperExportedDataObjectHTTP;
                      Assert.IsNotNull(messageHTTP); //Vsechny zpravy musi jit pretypovat

                      Assert.AreNotEqual(messageHTTP.Message.HTTPHeader.Fields.Count, 0); //Kazda hlavicka by mela mit alespon jednu polozku

                      if (!(messageHTTP.Message.HTTPHeader is HttpResponseHeader)) //Kazda hlavicka by mela jit pretypovat na request nebo response
                          Assert.IsTrue(messageHTTP.Message.HTTPHeader is HttpRequestHeader);

                  }
              }
          }*/
    }
}