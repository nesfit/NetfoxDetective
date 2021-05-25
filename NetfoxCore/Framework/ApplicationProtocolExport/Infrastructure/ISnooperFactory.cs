using System;
using System.Collections.Generic;
using System.IO;
using Netfox.Framework.ApplicationProtocolExport.Snoopers;
using Netfox.Framework.Models.Snoopers;

namespace Netfox.Framework.ApplicationProtocolExport.Infrastructure
{
    public interface ISnooperFactory
    {
        ISnooper Create(ISnooper snooperPrototype, SelectedConversations conversations, DirectoryInfo exportDirectory,
            Boolean ignoreApplicationTags);

        ISnooper Create(Type snooperType, SelectedConversations conversations, DirectoryInfo exportDirectory,
            Boolean ignoreApplicationTags);

        ISnooper Create(ISnooper snooperPrototype, IEnumerable<SnooperExportBase> sourceExports,
            DirectoryInfo exportDirectory);

        ISnooper Create(ISnooper snooperPrototype, IEnumerable<FileInfo> sourceFiles, DirectoryInfo exportDirectory);
        Type[] AvailableSnoopersTypes { get; }
        ISnooper[] AvailableSnoopers { get; }
    }
}