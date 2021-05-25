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

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Netfox.Framework.Properties;
//using NUnit.Framework;

//namespace Netfox.Framework.PmLib
//{
//    /// <summary> A pm library tests.</summary>
//    [TestFixture]
//    public static class PmLibTests
//    {
//        /// <summary> The test start Date/Time.</summary>
//        private static DateTime _testStart;

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void BasicOpenAndParseTest()
//        {
//            //Creation of index file.. if not existed before
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_0_cap); // @"..\..\..\TestingData\small_pcaps\0.cap";
//            var indexFilePath = Pcaps.Default.small_pcaps_0_cap + ".index"; // @"..\..\..\TestingData\small_pcaps\0.cap.index";
//            PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath);

//            TestStart();
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            capture.PrintFramesBrief();
//            Assert.IsTrue(Equals(capture.GetFrames().Count(), 23));
//            TestStop();
//        }

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void BasicOpenAndParseTestIndexMismatchTest()
//        {
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_0_cap);
//            var indexFilePath = Pcaps.Default.small_pcaps_0_cap + ".index"; // @"..\..\..\TestingData\small_pcaps\0.cap.index";
//            //Create index
//            PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath);
//            //malform index ... assumption - first 20B of index file belongs to hash 
//            using(var stream = new FileStream(indexFilePath, FileMode.Open, FileAccess.ReadWrite))
//            {
//                stream.Position = 10;
//                stream.WriteByte(0x04);
//                stream.Position = 11;
//                stream.WriteByte(0x04);
//                stream.Position = 12;
//                stream.WriteByte(0x04);
//            }
//            TestStart();
//            //Beaware, index file must exist! It is created in ExportTest.
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            //capture.PrintFramesBrief();
//            Assert.IsTrue(Equals(capture.GetFrames().Count(), 23));
//            TestStop();
//        }

//        /// <summary> Tests export.</summary>
//        [Test]
//        public static void ExportTest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_0_cap);
//           Console.WriteLine(@"Testing		 - {0}", pcapFileInfo.FullName);
//            var cap = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            cap.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));
//            cap.ExportFramesList(pcapFileInfo.FullName + ".index");
//            cap.SerializeFramesList(pcapFileInfo.FullName + ".serindex");
//            var fbin = new FileInfo(pcapFileInfo.FullName + ".index");
//            var fser = new FileInfo(pcapFileInfo.FullName + ".serindex");
//            Console.WriteLine(@"Binary index	 - Expected: 3033 bytes	Current: {0} bytes", fbin.Length);
//            Console.WriteLine(@"Serial index	 - Expected: 7320 bytes Current: {0} bytes", fser.Length);
//            Assert.IsTrue((Equals(fbin.Length, (long) 3033)));
//            Assert.IsTrue((Equals(fser.Length, (long) 7320)));
//            TestStop();
//        }

//        [Test]
//        public static void GRETest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_gre_sample_pcap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name))));
//            capture.PrintFramesDetail();
//            TestStop();
//        }

//        /// <summary> Tests ignore mnm filters.</summary>
//        [Test]
//        public static void IgnoreMnmFiltersTest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_mnmfilter_cap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));

//            foreach(var ram in capture.GetFrames()) { Console.Write("{0} ", ram.FrameIndex); }
//            Assert.IsTrue(Equals(capture.GetRealFramesCount(), capture.GetFramesCount()));
//            Assert.IsTrue(Equals(capture.GetRealFramesCount(), 8));
//            var last = (int) capture.GetFrames().ToList().Last().FrameIndex;
//            Console.WriteLine("\nCelkovy pocet ramcu = {0}\nIndex posledniho ramce = {1}", capture.GetRealFramesCount(), last);
//            Assert.IsTrue(Equals(last, 7));
//            TestStop();
//        }

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void IPFragmentationWithUDPTest()
//        {
//            //Creation of index file.. if not existed before
//            var pcapFileInfo = new FileInfo(Pcaps.Default.voip_asterisk_koleje_test_call_gsm_pcap);
//            var indexFilePath = Pcaps.Default.voip_asterisk_koleje_test_call_gsm_pcap + ".index"; // @"..\..\..\TestingData\small_pcaps\0.cap.index";
//            File.Delete(indexFilePath);
//            //PmCaptureManager.OpenCaptureFile(pcapFilePath, indexFilePath);

//            TestStart();
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            capture.PrintFramesBrief();
//            var frames = capture.GetFrames().ToList();
//            var fullUDPframe = frames[31];
//            var fragmentUDPframe = frames[32];

//            Assert.IsTrue(fullUDPframe.L2Offset == 6883 && fullUDPframe.L3Offset == 6897 && fullUDPframe.L4Offset == 6917 && fullUDPframe.L7Offset == 6925);

//            Assert.IsTrue(fragmentUDPframe.L2Offset == 8413 && fragmentUDPframe.L3Offset == 8427 && fragmentUDPframe.L4Offset == -1 && fragmentUDPframe.L7Offset == 8447);

//            Assert.IsTrue(Equals(frames.Count, 192));
//            TestStop();
//        }

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void OpenAndParseISAFromIndexMismatchTest()
//        {
//            var pcapFileInfo = new FileInfo(Pcaps.Default.pcap_mix_isa_http_pcap);
//            var indexFilePath = Pcaps.Default.pcap_mix_isa_http_pcap + ".index";
//            //Create index
//            PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath);
//            //malform index ... assumption - first 20B of index file belongs to hash 
//            using(var stream = new FileStream(indexFilePath, FileMode.Open, FileAccess.ReadWrite))
//            {
//                stream.Position = 10;
//                stream.WriteByte(0x04);
//                stream.Position = 11;
//                stream.WriteByte(0x04);
//                stream.Position = 12;
//                stream.WriteByte(0x04);
//            }
//            TestStart();
//            //Beaware, index file must exist! It is created in ExportTest.
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            //capture.PrintFramesBrief();
//            Assert.IsTrue(Equals(capture.GetFrames().Count(), 1332293));
//            TestStop();
//        }

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void OpenAndParseISAFromIndexTest()
//        {
//            var pcapFileInfo = new FileInfo(Pcaps.Default.pcap_mix_isa_http_pcap);
//            var indexFilePath = Pcaps.Default.pcap_mix_isa_http_pcap + ".index";
//            TestStart();
//            //Beaware, index file must exist! It is created in ExportTest.
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            //capture.PrintFramesBrief();
//            Assert.IsTrue(Equals(capture.GetFrames().Count(), 1332293));
//            TestStop();
//        }

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void OpenAndParseISAWholeTest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.pcap_mix_isa_http_pcap);
//            var indexFilePath = Pcaps.Default.pcap_mix_isa_http_pcap + ".index";
//            File.Delete(indexFilePath);
//            //Beaware, index file must exist! It is created in ExportTest.
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            //capture.PrintFramesBrief();
//            Assert.IsTrue(Equals(capture.GetFrames().Count(), 1332293));
//            TestStop();
//        }

//        /// <summary> Tests basic open and parse.</summary>
//        [Test]
//        public static void OpenAndParseMalformedPacketTest()
//        {
//            //Creation of index file.. if not existed before
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_malformed_last_packet_pcap);
//            var indexFilePath = Pcaps.Default.small_pcaps_malformed_last_packet_pcap + ".index"; // @"..\..\..\TestingData\small_pcaps\0.cap.index";
//            File.Delete(indexFilePath);
//            //PmCaptureManager.OpenCaptureFile(pcapFilePath, indexFilePath);

//            TestStart();
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo, indexFilePath) as PmCaptureProcessorBase;
//            capture.PrintFramesBrief();
//            var frames = capture.GetFrames().ToList();
//            Assert.IsTrue(Equals(frames.Count, 21));
//            TestStop();
//        }

//        /// <summary> Searches for the first test.</summary>
//        [Test]
//        public static void SearchTest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_0_cap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));
//            //Set pattern for multicast address 239.255.255.250
//            var pattern = new List<byte[]>
//            {
//                new byte[]
//                {
//                    0x01,
//                    0x00,
//                    0x5e,
//                    0x7f,
//                    0xff,
//                    0xfa
//                }
//            };
//            var lis = capture.SearchFrames(pattern);
//            //Whole pcap search
//            Console.WriteLine("Pattern should be found in 12 frames and it was detected in {0} frames", lis.Count());
//            Assert.IsTrue(Equals(lis.Count(), 12));
//            //Selective search based on indexes
//            var lis2 = capture.SearchFrames(new List<UInt32>
//            {
//                13,
//                14,
//                15,
//                16,
//                17,
//                18
//            }, pattern);
//            Console.WriteLine("Pattern should be found in 4 frames and it was detected in {0} frames", lis2.Count());
//            Assert.IsTrue(Equals(lis2.Count(), 4));
//            /*
//            Console.WriteLine("Pattern found in following frames:");         
//            foreach (var fr in lis)
//            {
//                Console.WriteLine(fr.ToString());   
//            }
//             */
//            TestStop();
//        }

//        [Test]
//        public static void TeredoTest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_teredo_complete_pcap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));
//            capture.PrintFramesDetail();
//            TestStop();
//        }

//        [Test]
//        public static void Test_6in4()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_6in4_singleFrame_pcap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));
//            capture.PrintFramesDetail();
//            TestStop();
//        }

//        /// <summary> Tests test banch.</summary>
//        [Test]
//        public static void TestBanchTest()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.pcap_mix_mix_pcap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));


//            var find = new uint[10000];
//            for(uint i = 0; i < 10000; i++) { find[i] = i; }

//            var frames = capture.GetFramesEnumerable(find);
//            TestStop();
//        }

//        /// <summary> Tests without l 2.</summary>
//        [Test]
//        public static void WithoutL2Test()
//        {
//            TestStart();
//            var pcapFileInfo = new FileInfo(Pcaps.Default.small_pcaps_L2_missing_pcap);
//            var capture = PmCaptureManager.OpenCaptureFile(pcapFileInfo) as PmCaptureProcessorBase;
//            capture.ParseCaptureFile(Path.Combine(pcapFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(pcapFileInfo.Name)));
//            capture.PrintFramesDetail();
//            TestStop();
//        }

//        /// <summary> Tests start.</summary>
//        private static void TestStart() => _testStart = DateTime.Now;

//        /// <summary> Tests stop.</summary>
//        private static void TestStop()
//        {
//            var testStop = DateTime.Now;
//            var duration = testStop - _testStart;
//            Console.WriteLine("Timeduration: " + duration.Hours + ":" + duration.Minutes + ":" + duration.Seconds + "." + duration.Milliseconds);
//        }
//    }
//}
namespace Netfox.Framework.Tests
{
}