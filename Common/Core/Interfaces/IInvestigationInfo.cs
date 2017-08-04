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
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using Castle.Core.Logging;

namespace Netfox.Core.Interfaces {
    public interface IInvestigationInfo {
        DirectoryInfo InvestigationsDirectoryInfo { get; set; }
        DirectoryInfo InvestigationDirectoryInfo { get; }
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