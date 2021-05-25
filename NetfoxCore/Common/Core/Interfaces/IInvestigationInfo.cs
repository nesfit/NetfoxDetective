using System;
using System.Data.SqlClient;
using System.IO;
using System.IO.Abstractions;

namespace Netfox.Core.Interfaces
{
    public interface IInvestigationInfo
    {
        DirectoryInfoBase InvestigationsDirectoryInfo { get; set; }
        DirectoryInfoBase InvestigationDirectoryInfo { get; }
        string InvestigationName { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        DateTime Created { get; set; }
        DateTime LastRecentlyUsed { get; set; }
        Guid Guid { get; set; }
        SqlConnectionStringBuilder SqlConnectionStringBuilder { get; set; }
        FileInfo InvestigationFileInfo { get; }
        String InvestigationFileRelativePath { get; }
        DirectoryInfo DatabaseDirectoryInfo { get; }
        FileInfo DatabaseFileInfo { get; }
        DirectoryInfo ExportsDirectoryInfo { get; }
        DirectoryInfo SourceDirectoryInfo { get; }
        DirectoryInfo SourceCaptureDirectoryInfo { get; }
        DirectoryInfo SourceNetflowDirectoryInfo { get; }
        DirectoryInfo SourceLogsDirectoryInfo { get; }
        DirectoryInfo SettingsDirectoryInfo { get; }
        DirectoryInfo TempDirectoryInfo { get; }
        DirectoryInfo LogsDirectoryInfo { get; }
        bool IsInMemory { get; set; }
        void CreateFileStructure();
    }
}