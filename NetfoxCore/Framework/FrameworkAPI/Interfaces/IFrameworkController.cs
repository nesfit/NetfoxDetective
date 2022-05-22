using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.FrameworkAPI.Interfaces
{
    public interface IFrameworkController
    {
        void ExportData(
            IEnumerable<ISnooper> snoopers,
            IEnumerable<ILxConversation> conversations,
            DirectoryInfo exportDirectory,
            Boolean ignoreApplicationTags);

        void ExportData(
            IEnumerable<Type> snooperTypes,
            IEnumerable<ILxConversation> conversations,
            DirectoryInfo exportDirectory,
            Boolean ignoreApplicationTags);

        void ExportData(IEnumerable<ISnooper> snoopers, IEnumerable<SnooperExportBase> snooperExports,
            DirectoryInfo exportDirectory);

        void ExportData(IEnumerable<ISnooper> snoopers, ConcurrentBag<SnooperExportBase> snooperExports,
            DirectoryInfo exportDirectory);

        void ExportData(IEnumerable<ISnooper> snoopers, IEnumerable<FileInfo> sourceFiles,
            DirectoryInfo exportDirectory);

        void ProcessCapture(FileInfo captureFileInfo);
    }
}