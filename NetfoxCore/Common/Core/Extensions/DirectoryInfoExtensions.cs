using System.IO;
using System.IO.Abstractions;

namespace Netfox.Core.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo GetSubdirectory(this DirectoryInfo directoryInfo, string directory)
        {
            return new DirectoryInfo(Path.Combine(directoryInfo.FullName, directory));
        }

        public static FileInfo GetSubFileInfo(this DirectoryInfo directoryInfo, string fileName)
        {
            return new FileInfo(Path.Combine(directoryInfo.FullName, fileName));
        }

        public static DirectoryInfoBase GetSubdirectory(this DirectoryInfoBase directoryInfo, string directory)
        {
            return new FileSystem().DirectoryInfo.FromDirectoryName(Path.Combine(directoryInfo.FullName, directory)) as
                DirectoryInfoBase;
        }
    }
}