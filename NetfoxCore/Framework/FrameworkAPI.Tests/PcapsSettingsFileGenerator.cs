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
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Netfox.FrameworkAPI.Tests
{
//     [TestFixture][Ignore("For settings file generation only")]
//     internal static class PcapsSettingsFileGenerator
//     {
//         [Test]
//         public static void Generate()
//         {
//             Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
//
//             var testingDataPath = Paths.Default.TestingDataBaseFilePath;
//             var pcapsSettingsFilePath = Paths.Default.PcapsSettingsFilePath;
//             var snooperPcapsSettingsFilePath = Paths.Default.SnoopersPcapsSettingsFilePath;
//
//             var caps = Directory.EnumerateFiles(testingDataPath, "*.cap", SearchOption.AllDirectories);
//             caps = caps.Concat(Directory.EnumerateFiles(testingDataPath, "*.pcap", SearchOption.AllDirectories));
//             caps = caps.Concat(Directory.EnumerateFiles(testingDataPath, "*.pcapng", SearchOption.AllDirectories));
//             caps = caps.Concat(Directory.EnumerateFiles(testingDataPath, "*.wdat", SearchOption.AllDirectories));
//             var enumCaps = caps as string[] ?? caps.ToArray();
//
//
//             using(var pcapsSW = new StreamWriter(pcapsSettingsFilePath))
//             {
//                 pcapsSW.Write(
//                     "<?xml version='1.0' encoding='utf-8'?><SettingsFile xmlns=\"http://schemas.microsoft.com/VisualStudio/2004/01/settings\" CurrentProfile=\"(Default)\" GeneratedClassNamespace=\"Netfox.FrameworkCompact.Properties\" GeneratedClassName=\"Pcaps\"><Profiles /><Settings>");
//                 pcapsSW.Write(pcapsSW.NewLine);
//
//                 foreach(var cap in enumCaps)
//                 {
//                     var filePath = new String(cap.Skip(testingDataPath.Count()).ToArray());
//                     Console.WriteLine(filePath);
//                     filePath = filePath.Replace(@"\\", "_").Replace(@"\", "_").Replace(".", "_").Replace("-", "_").Replace(" ", "_").Replace(",", "_");
//                     pcapsSW.Write("<Setting Name=\"{0}\" Type=\"System.String\" Scope=\"Application\"><Value Profile=\"(Default)\">{1}</Value></Setting>{2}", filePath, cap,
//                         pcapsSW.NewLine);
//                 }
//                 pcapsSW.Write("</Settings></SettingsFile>");
//             }
//
//             using(var snoopersPcapsSW = new StreamWriter(snooperPcapsSettingsFilePath))
//             {
//                 snoopersPcapsSW.Write(
//                     "<?xml version='1.0' encoding='utf-8'?><SettingsFile xmlns=\"http://schemas.microsoft.com/VisualStudio/2004/01/settings\" CurrentProfile=\"(Default)\" GeneratedClassNamespace=\"Netfox.FrameworkCompact.Properties\" GeneratedClassName=\"SnoopersPcaps\"><Profiles /><Settings>");
//                 snoopersPcapsSW.Write(snoopersPcapsSW.NewLine);
//
//                 foreach(var cap in enumCaps)
//                 {
//                     var filePath = new String(cap.Skip(testingDataPath.Count()).ToArray());
//                     var snooperCap = String.Concat(@"..\", cap);
// //                    Console.WriteLine(filePath);
//                     filePath = filePath.Replace(@"\\", "_").Replace(@"\", "_").Replace(".", "_").Replace("-", "_").Replace(" ", "_").Replace(",", "_");
//                     snoopersPcapsSW.Write("<Setting Name=\"{0}\" Type=\"System.String\" Scope=\"Application\"><Value Profile=\"(Default)\">{1}</Value></Setting>{2}", filePath,
//                         snooperCap, snoopersPcapsSW.NewLine);
//                 }
//                 snoopersPcapsSW.Write("</Settings></SettingsFile>");
//             }
//         }
//     }
}