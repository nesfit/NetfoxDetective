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

using System.Globalization;
using System.Linq;
using ApplicationProtocolExport.Tests;
using Netfox.FrameworkAPI.Tests;
using Netfox.Snoopers.SnooperXMPP.Models;
using NUnit.Framework;

namespace Netfox.Snoopers.SnooperXMPP.Tests
{
    internal class SnooperXMPPTests : SnooperBaseTests
    {
        [Test]
        public void SnooperXMPPTest()
        {
            var messagePatterns = new[]
            {
                new []{"27.07.2011 8:14:38","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","čau chlapáku jak se vede"},
                new []{"27.07.2011 8:15:25","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","    ( x - x0)^2 + ( y - y0)^2=r^2 "},
                new []{"27.07.2011 8:15:56","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","aha. vidím, že sis dal práci s hledáním rovnice kružnice"},
                new []{"27.07.2011 8:16:03","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","takový základy analytické geometrie snad ví každný ne"},
                new []{"27.07.2011 8:16:44","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","snad jo"},
                new []{"27.07.2011 8:16:48","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","dobrý den"},
                new []{"27.07.2011 8:16:48","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","dobrej"},
                new []{"27.07.2011 8:16:59","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","v kolik jsi šel spat?"},
                new []{"27.07.2011 8:17:02","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","jak se vede a kam se jede\r\nve 2"},
                new []{"27.07.2011 8:17:10","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","aha a vstával?"},
                new []{"27.07.2011 8:17:18","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","ve 4, ne :-D"},
                new []{"27.07.2011 8:17:30","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","v 7, jel jsem na firmu přestěhovat ten server"},
                new []{"27.07.2011 8:17:35","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","aha. tak se měl pak za mnou stavit. udělal bych ti tuřany, když jsi tak unavený\r\nbys teprve koukal"},
                new []{"27.07.2011 8:18:25","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","jinak ten guláš co jsem ti nabízel už je fuč\r\n:-D"},
                new []{"27.07.2011 8:18:29","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","říkal jsem si že bych se stavil ale nakonec jsem jel dom jsem fakt utahanej\r\naha"},
                new []{"27.07.2011 8:18:37","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","dal sis na véééču jo"},
                new []{"27.07.2011 8:18:43","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","tak tak"},
                new []{"27.07.2011 8:18:46","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","jeden gulášek s pěti knedlama\r\nja"},
                new []{"27.07.2011 8:19:04","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","měj jsem hlad, že bych se zeblil. však to znáš ;)\r\nnic. tohle by mělo stačit\r\ndíkec\r\na dobrou noc"},
                new []{"27.07.2011 8:19:31","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","20% dolu z výnosů"},
                new []{"27.07.2011 8:19:38","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","tak když to převedu na těch tvojich 30%, tak se to dorovnává na těch 50"},
                new []{"27.07.2011 8:20:13","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735",":-D"},
                new []{"27.07.2011 8:20:21","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC",":-P"},
                new []{"27.07.2011 8:20:23","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","no nic jdu spát"},
                new []{"27.07.2011 8:20:29","leoshek@jabbim.com/100A1-PC","niklicek@qip.ru/Studio-1735","dobrou"},
                new []{"27.07.2011 8:20:31","niklicek@qip.ru/Studio-1735","leoshek@jabbim.com/100A1-PC","brou"}
            };

            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(PcapPath.GetPcap(PcapPath.Pcaps.pcap_mix_xmpp_nosecure_cap)));

            var conversations = this.L7Conversations.ToArray();
            this.FrameworkController.ExportData(this.AvailableSnoopersTypes, conversations, this.CurrentTestBaseDirectory, false);

            SnooperExportXMPP exportedObjectsReference = null;
            foreach (var exportedObjects in this.SnooperExports.ToArray()) //Get XMPPSnooper exported objects
            {
                if ((exportedObjectsReference = exportedObjects as SnooperExportXMPP) != null)
                    break;
            }
            Assert.IsNotNull(exportedObjectsReference);

            var exportedObjectBases = exportedObjectsReference.ExportObjects.ToArray();
            Assert.AreEqual(26, exportedObjectBases.Length);

            var messages = exportedObjectBases.Where(i => i is SnooperExportedObjectXMPP).Cast<SnooperExportedObjectXMPP>().OrderBy(it => it.TimeStamp).ToArray();
            Assert.AreEqual(26, messages.Length); //Every exported object should be private message
            Assert.AreEqual(messagePatterns.Length, messages.Length);

            for (var i = 0; i < messages.Length; i++)
            {
                Assert.AreEqual(messages[i].TimeStamp.ToString(new CultureInfo("cs-CZ", false)), messagePatterns[i][0]);
                Assert.AreEqual(messages[i].Sender, messagePatterns[i][1]);
                Assert.AreEqual(messages[i].Receiver, messagePatterns[i][2]);
                Assert.AreEqual(messages[i].Message, messagePatterns[i][3]);
            }
        }
    }
}
