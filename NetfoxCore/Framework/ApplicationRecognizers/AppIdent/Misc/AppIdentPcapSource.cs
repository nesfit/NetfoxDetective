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
using System.IO;

namespace Netfox.AppIdent.Misc {
    [Serializable]
    public class AppIdentPcapSource
    {
        public IReadOnlyList<string> TestingPcaps => this._testingPcaps;
        public IReadOnlyList<string> VerificationPcaps => this._verificationPcaps;
        private readonly List<string> _testingPcaps  = new List<string>();
        private readonly List<string> _verificationPcaps = new List<string>();

        public void AddTesting(string pcapFilePath)
        {
            this._testingPcaps.Add(pcapFilePath);
        }
        public void AddVerification(string pcapFilePath)
        {
            this._verificationPcaps.Add(pcapFilePath);
        }

        public void AddTesting(string directoryFilePath, string wildcard, bool recursive)
        {
            var extensions = wildcard.Split('|');
            foreach (var extension in extensions)
            {
                var pcapFilePaths = Directory.GetFiles(directoryFilePath, extension, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (var pcapFilePath in pcapFilePaths)
                {
                    this._testingPcaps.Add(pcapFilePath);
                }
            }
        }
        public void AddVerification(string directoryFilePath, string wildcard, bool recursive)
        {
            var extensions = wildcard.Split('|');
            foreach(var extension in extensions)
            {
                var pcapFilePaths = Directory.GetFiles(directoryFilePath, extension, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (var pcapFilePath in pcapFilePaths)
                {
                    this._verificationPcaps.Add(pcapFilePath);
                }
            }
        }
    }
}