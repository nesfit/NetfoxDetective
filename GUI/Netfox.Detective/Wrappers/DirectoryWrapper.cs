// Copyright (c) 2018 Hana Slamova
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using Netfox.Detective.Interfaces;

namespace Netfox.Detective.Wrappers
{
    public class DirectoryWrapper:IDirectoryWrapper
    {
        public void Delete(string directoryFullName)
        {
            while(AreLocked("*.log", directoryFullName))
            {
                Thread.Sleep(100);
            }

            Delete(directoryFullName,true);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void Delete(string directoryFullName, bool isRecursive)
        {
            Directory.Delete(directoryFullName, isRecursive);
        }

        private bool AreLocked(string filesPattern, string directoryFullName)
        {
            var files = GetFiles(directoryFullName,filesPattern);
            foreach (var file in files)
            {
                if (IsLocked(file)) return true;
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        protected virtual string[] GetFiles(string directoryFullName,string pattern,SearchOption option=SearchOption.AllDirectories)
        {
            return Directory.GetFiles(directoryFullName, pattern,option);
        }
        private bool IsLocked(String fileFullName)
        {
            try
            {
                using (OpenFile(fileFullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return true;
            }
        }

        [ExcludeFromCodeCoverage]
        protected virtual IDisposable OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        }
    }
}
