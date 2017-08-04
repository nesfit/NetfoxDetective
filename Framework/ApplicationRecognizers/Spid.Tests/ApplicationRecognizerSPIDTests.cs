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
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Netfox.AppIdent;
using Netfox.NetfoxFrameworkAPI.Tests;
using Netfox.NetfoxFrameworkAPI.Tests.Properties;
using NUnit.Framework;

namespace Spid.Tests
{
    /// <summary> An application recognizer spid tests.</summary>
    [TestFixture]
    public class ApplicationRecognizerSPIDTests : FrameworkBaseTests
    {
        /// <summary> The test start Date/Time.</summary>
        private static DateTime _testStart;

        [OneTimeSetUp]

        public void SetUp()
        {
            base.SetUpInMemory();
            this.WindsorContainer.Register(Component.For<ApplicationRecognizerSpid>());
        }
        
        [Test][Ignore("Debugging test")]
        public void DumpTestSpid()
        {
            var csv = new StringBuilder();
            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(SnoopersPcaps.Default.pcap_mix_root_cz_cap));

            var conversations = this.L7Conversations.ToArray();

            csv.AppendLine(string.Format(this.PmCaptures.First().FileInfo.FullName) + "\n\n");
            var spid = this.WindsorContainer.Resolve<ApplicationRecognizerSpid>();

            foreach(var conv in conversations) { spid.UpdateModelForProtocol(conv); }


            this.FrameworkController.ProcessCapture(this.PrepareCaptureForProcessing(Pcaps.Default.pcap_mix_root_cz_cap));

            conversations = this.L7Conversations.ToArray();

            csv.AppendLine(string.Format(this.PmCaptures.First().FileInfo.FullName) + "\n\n");

            var measure = new Dictionary<string, Array>();
            var unknown = 0;

            foreach(var conv in conversations)
            {
                var appTag = conv.AppTag;
                if(appTag == null) { continue; }

                var tmpRet = spid.RecognizeConversation2(conv);
                if(tmpRet == null)
                {
                    unknown++;
                    continue;
                }


                var line = string.Format(conv.SourceEndPoint + " " + conv.DestinationEndPoint + ";" + appTag + ";" + tmpRet);
                Console.WriteLine(line);
                csv.AppendLine(line);

                if(!measure.ContainsKey(tmpRet))
                {
                    measure.Add(tmpRet, new[]
                    {
                        0,
                        0,
                        0
                    });
                }

                if(!measure.ContainsKey(appTag))
                {
                    measure.Add(appTag, new[]
                    {
                        0,
                        0,
                        0
                    });
                }

                if(appTag == tmpRet)
                {
                    //TP
                    measure[tmpRet].SetValue((int) measure[tmpRet].GetValue(0) + 1, 0);
                }
                else if(appTag != tmpRet)
                {
                    //FP
                    measure[tmpRet].SetValue((int) measure[tmpRet].GetValue(2) + 1, 2);
                    //FN
                    measure[appTag].SetValue((int) measure[appTag].GetValue(1) + 1, 1);
                }
            }

            var unk = string.Format("\n" + "Unknown " + unknown);
            csv.AppendLine(unk);
            Console.WriteLine(unk);

            foreach(var tag in measure)
            {
                var line = string.Format("\n" + tag.Key + "   TP: " + tag.Value.GetValue(0) + "   FP: " + tag.Value.GetValue(2) + "   FN: " + tag.Value.GetValue(1));
                Console.WriteLine(line);
                csv.AppendLine(line);

                line = string.Format("Precision: " + Utilities.Precission((int) tag.Value.GetValue(0), (int) tag.Value.GetValue(2)));
                Console.WriteLine(line);
                csv.AppendLine(line);

                line = string.Format("Recall: " + Utilities.Recall((int) tag.Value.GetValue(0), (int) tag.Value.GetValue(0) + (int) tag.Value.GetValue(1)));
                Console.WriteLine(line);
                csv.AppendLine(line);

                line =
                    string.Format("F-Measure: "
                                  + Utilities.F_Measure((int) tag.Value.GetValue(0), (int) tag.Value.GetValue(2), (int) tag.Value.GetValue(0) + (int) tag.Value.GetValue(1)));
                Console.WriteLine(line);
                csv.AppendLine(line);

                //Console.WriteLine("\n" + tag.Key + "   TP: " + tag.Value.GetValue(0) + "   FP: " + tag.Value.GetValue(2) + "   FN: " + tag.Value.GetValue(1));
                //Console.WriteLine("Precision: " + this.Precission((int)tag.Value.GetValue(0), (int)tag.Value.GetValue(2)));
                //Console.WriteLine("Recall: " + this.Recall((int)tag.Value.GetValue(0), (int)tag.Value.GetValue(0) + (int)tag.Value.GetValue(1)));
                //Console.WriteLine("F-Measure: " + this.F_Measure((int)tag.Value.GetValue(0), (int)tag.Value.GetValue(2), (int)tag.Value.GetValue(0) + (int)tag.Value.GetValue(1)));
            }

            //File.AppendAllText("X:\\DPtest\\SPID.csv", csv.ToString());
        }

        /// <summary> Tests application recognizer spid.</summary>
        //[SetUp]
        //public void ApplicationRecognizerSPIDTest() { this.recognizer = new ApplicationRecognizerSpid(); }

        // <summary> Tests recognize conversation fast.</summary>
        //[Test]
        //public void RecognizeConversationFastTest()
        //{
        //TestStart();
        //var path = Pcaps.Default.pcap_mix_imap_smtp_collector_pcap; //@"..\..\..\TestingData\pcap_mix\http.cap"; // 

        //var captureProcessor = new Capture(path);
        //captureProcessor.TrackConversations();
        //Console.WriteLine("Conversations> " + captureProcessor.GetConversations().Count());

        //var captureProcessorSPID = new Capture(path);
        //captureProcessorSPID.TrackConversations(this.recognizer);
        //var convsSPID = captureProcessorSPID.GetConversations();

        ////Debug.Assert(convsSPID != null, "convs != null");
        //var ctConversationValues = convsSPID as CsConversationValue[] ?? convsSPID.ToArray();

        //var HTTP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.http));
        //var ICQ = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.icq));
        //var YMSG = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.ymsg));
        //var MSN = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.msn));
        //var XMPP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.xmpp));
        //var POP3 = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.pop3));
        //var IMAP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.imap));
        //var SMTP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.smtp));
        //var DNS = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.dns));

        //var portAppTags = (from conv in captureProcessor.GetConversations()
        //              select conv).GroupBy(conv => conv.ApplicationTags);
        //var SPIDAppTags = (from conv in convsSPID
        //                   select conv).GroupBy(conv => conv.ApplicationTags);

        //var appTags = portAppTags as IGrouping<string, CsConversationValue>[] ?? portAppTags.ToArray();
        //var spidAppTags = SPIDAppTags as IGrouping<string, CsConversationValue>[] ?? SPIDAppTags.ToArray();
        //foreach (var appstag in (appTags.Concat(spidAppTags)).GroupBy(i=>i.Key))
        //{
        //    var port = appTags.Where(i => i.Key == appstag.Key);
        //    var spid = spidAppTags.Where(i => i.Key == appstag.Key);
        //    int ports =0;
        //    int spids=0;
        //    if (port.Any())
        //        ports = port.First().Count();
        //    if (spid.Any()) 
        //        spids = spid.First().Count();
        //    Console.WriteLine("{0}, port> {1}, SPID> {2}", appstag.Key, ports, spids);
        //}

        //TestStop();

        //Assert.IsTrue(HTTP == 0 && ICQ == 0 && YMSG == 0 && MSN == 0 && XMPP == 0 && POP3 == 0 && IMAP == 7 && SMTP == 22 && DNS == 11);
        //}

        //[Test]
        //[Category("LongRunning")]
        //public void RecognizeConversationTest()
        //{
        //    TestStart();

        //    var path = Pcaps.Default.pcap_mix_mix_pcap;

        //    var captureProcessor = new Capture(path);
        //    captureProcessor.TrackConversations();
        //    Console.WriteLine("Conversations> " + captureProcessor.GetConversations().Count());

        //    var captureProcessorSPID = new Capture(path);
        //    captureProcessorSPID.TrackConversations(this.recognizer);
        //    var convsSPID = captureProcessorSPID.GetConversations();

        //    Debug.Assert(convsSPID != null, "convs != null");
        //    var ctConversationValues = convsSPID as CsConversationValue[] ?? convsSPID.ToArray();

        //    var HTTP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.http));
        //    var ICQ = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.icq));
        //    var YMSG = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.ymsg));
        //    var MSN = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.msn));
        //    var XMPP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.xmpp));
        //    var POP3 = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.pop3));
        //    var IMAP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.imap));
        //    var SMTP = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.smtp));
        //    var DNS = ctConversationValues.Count(c => c.isXYProtocolConversation(NBARprotocols.Default.dns));

        //    //var portAppTags = (from conv in captureProcessor.GetConversations()
        //    //                   select conv).GroupBy(conv => conv.ApplicationTags);
        //    //var SPIDAppTags = (from conv in convsSPID
        //    //                   select conv).GroupBy(conv => conv.ApplicationTags);

        //    //var appTags = portAppTags as IGrouping<string, CsConversationValue>[] ?? portAppTags.ToArray();
        //    //var spidAppTags = SPIDAppTags as IGrouping<string, CsConversationValue>[] ?? SPIDAppTags.ToArray();
        //    //foreach (var appstag in (appTags.Concat(spidAppTags)).GroupBy(i => i.Key))
        //    //{
        //    //    var port = appTags.Where(i => i.Key == appstag.Key);
        //    //    var spid = spidAppTags.Where(i => i.Key == appstag.Key);
        //    //    int ports = 0;
        //    //    int spids = 0;
        //    //    if (port.Any())
        //    //        ports = port.First().Count();
        //    //    if (spid.Any())
        //    //        spids = spid.First().Count();
        //    //    Console.WriteLine("{0}, port> {1}, SPID> {2}", appstag.Key, ports, spids);
        //    //}

        //    TestStop();

        //    Assert.IsTrue(HTTP == 3176 && ICQ == 0 && YMSG == 0 && MSN == 3 && XMPP == 0 && POP3 == 0 && IMAP == 21 && SMTP == 23 && DNS == 2852);
        //}
        /// <summary> Tests recognize conversation.</summary>
        /// <summary> Tests start.</summary>
        private static void TestStart()
        {
            _testStart = DateTime.Now;
        }

        /// <summary> Tests stop.</summary>
        private static void TestStop()
        {
            var testStop = DateTime.Now;
            var duration = testStop - _testStart;
            Console.WriteLine("Timeduration: " + duration.Hours + ":" + duration.Minutes + ":" + duration.Seconds + "." + duration.Milliseconds);
        }
    }
}