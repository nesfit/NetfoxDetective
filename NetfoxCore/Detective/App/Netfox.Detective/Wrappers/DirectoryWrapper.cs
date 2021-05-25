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