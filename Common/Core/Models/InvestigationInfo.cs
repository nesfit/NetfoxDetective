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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Castle.Core;
using Castle.Core.Logging;
using Netfox.Core.Extensions;
using Netfox.Core.Interfaces;
using Netfox.Core.Properties;

namespace Netfox.Core.Models
{
    [DataContract(Name = "Investigation", Namespace = "Netfox.Detective.Models.Base.Detective")]
    public class InvestigationInfo : IInvestigationInfo
    {
        private const string ExportsDirectory = "Exports";
        private const string SourceDirectory = "Sources";
        private const string SourceCaptureDirectory = "Captures";
        private const string SourceNetflowDirectory = "Netflow";
        private const string SourceLogsDirectory = "Logs";
        private const string SettingsDirectory = "Settings";
        private const string TempDirectory = "Temp";
        private const string LogsDirectory = "Logs";
        private const string DatabaseDirectory = "Database";

        public InvestigationInfo(ILogger logger)
        {
            this.Logger = logger;
            this.Guid = Guid.NewGuid();
        }

        public ILogger Logger { get; }

        public DirectoryInfo InvestigationsDirectoryInfo
        {
            get => this._investigationsDirectoryInfo;
            set
            {
                this.CleanFilesStructreInfo();
                this._investigationsDirectoryInfo = value;
                this.Created = DateTime.UtcNow;
                this.LastRecentlyUsed = this.Created;
            }
        }

        public DirectoryInfo InvestigationDirectoryInfo => this._investigationsDirectoryInfo?.GetSubdirectory(this.InvestigationName + "_" + this.Guid);
        private DirectoryInfo _investigationsDirectoryInfo ;

        public void CreateFileStructure()
        {
            if(!this.InvestigationsDirectoryInfo.Exists) { this.InvestigationsDirectoryInfo.Create(); }
            if(!this.InvestigationDirectoryInfo.Exists) { this.InvestigationDirectoryInfo.Create(); }
            if(!this.DatabaseDirectoryInfo.Exists) { this.DatabaseDirectoryInfo.Create(); }
            if(!this.ExportsDirectoryInfo.Exists) { this.ExportsDirectoryInfo.Create(); }
            if(!this.SourceDirectoryInfo.Exists) { this.SourceDirectoryInfo.Create(); }
            if(!this.SourceCaptureDirectoryInfo.Exists) { this.SourceCaptureDirectoryInfo.Create(); }
            if(!this.SourceNetflowDirectoryInfo.Exists) { this.SourceNetflowDirectoryInfo.Create(); }
            if(!this.SourceLogsDirectoryInfo.Exists) { this.SourceLogsDirectoryInfo.Create(); }
            if(!this.SettingsDirectoryInfo.Exists) { this.SettingsDirectoryInfo.Create(); }
            if(!this.TempDirectoryInfo.Exists) { this.TempDirectoryInfo.Create(); }
        }

        private void CleanFilesStructreInfo()
        {
            //this._investigationsDirectoryInfo = null;
            this._databaseDirectoryInfo = null;
            this._exportsDirectoryInfo = null;
            this._sourceDirectoryInfo = null;
            this._sourceCaptureDirectoryInfo = null;
            this._sourceNetflowDirectoryInfo = null;
            this._sourceLogsDirectoryInfo = null;
            this._settingsDirectoryInfo = null;
            this._tempDirectoryInfo = null;
            this._databaseFileInfo = null;
        }

        #region properties
        [DataMember]
        public string InvestigationName
        {
            get => this._investigationName;
            set
            {
                this._investigationName = value;
                this.CleanFilesStructreInfo();
            }
        }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public DateTime LastRecentlyUsed { get; set; }

        [DataMember]
        public Guid Guid { get; set; }

        [DataMember][DoNotWire]
        public SqlConnectionStringBuilder SqlConnectionStringBuilder { get; set; }

        private FileInfo _investigationFileInfo;

        [XmlIgnore]
        public FileInfo InvestigationFileInfo
        {
            get
            {
                if(this._investigationFileInfo != null) { return this._investigationFileInfo; }
                if(this.InvestigationDirectoryInfo == null || this.InvestigationName == null) { return null; }
                this._investigationFileInfo = new FileInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName,
                    this.InvestigationName + "." + NetfoxSettings.Default.InvestigationFileExtension));
                return this._investigationFileInfo;
            }
        }

        public String InvestigationFileRelativePath
        {
            get
            {
                if(this.InvestigationFileInfo == null || this._investigationsDirectoryInfo == null) { return null; }
                if(String.IsNullOrEmpty(this.InvestigationFileInfo.FullName)) { return null; }
                if(String.IsNullOrEmpty(this._investigationsDirectoryInfo.FullName)) { return this.InvestigationFileInfo.FullName; }

                var uri1 = new Uri(this.InvestigationFileInfo.FullName, UriKind.RelativeOrAbsolute);
                if(!uri1.IsAbsoluteUri) { return this.InvestigationFileInfo.FullName; } // else it is already a relative path

                var uri2 = new Uri(this._investigationsDirectoryInfo.FullName + "/", UriKind.Absolute);
                var relativeUri = uri2.MakeRelativeUri(uri1);
                return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
            }
        }

        public DirectoryInfo DatabaseDirectoryInfo
        {
            get
            {
                if(this._databaseDirectoryInfo != null) { return this._databaseDirectoryInfo; }
                if(this.InvestigationDirectoryInfo == null) { return null; }
                this._databaseDirectoryInfo = new DirectoryInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName, DatabaseDirectory));

                this.OnPropertyChanged();
                return this._databaseDirectoryInfo;
            }
        }

        public FileInfo DatabaseFileInfo
        {
            get
            {
                if(this._databaseFileInfo != null) return this._databaseFileInfo;
                if(this.DatabaseDirectoryInfo == null) { return null; }
                this._databaseFileInfo = this.DatabaseDirectoryInfo.GetSubFileInfo(this.InvestigationName + "." + NetfoxSettings.Default.DatabaseFileExtension);

                this.OnPropertyChanged();
                return this._databaseFileInfo;
            }
        }

        public DirectoryInfo ExportsDirectoryInfo
        {
            get
            {
                if(this._exportsDirectoryInfo != null) { return this._exportsDirectoryInfo; }
                if(this.InvestigationDirectoryInfo == null) { return null; }
                this._exportsDirectoryInfo = new DirectoryInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName, ExportsDirectory));

                this.OnPropertyChanged();
                return this._exportsDirectoryInfo;
            }
        }

        public DirectoryInfo SourceDirectoryInfo
        {
            get
            {
                if(this._sourceDirectoryInfo != null) { return this._sourceDirectoryInfo; }
                if(this.InvestigationDirectoryInfo == null) { return null; }
                this._sourceDirectoryInfo = new DirectoryInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName, SourceDirectory));

                this.OnPropertyChanged();
                return this._sourceDirectoryInfo;
            }
        }

        public DirectoryInfo SourceCaptureDirectoryInfo
        {
            get
            {
                if(this._sourceCaptureDirectoryInfo != null) { return this._sourceCaptureDirectoryInfo; }
                if(this.SourceDirectoryInfo == null) { return null; }
                this._sourceCaptureDirectoryInfo = new DirectoryInfo(Path.Combine(this.SourceDirectoryInfo.FullName, SourceCaptureDirectory));

                this.OnPropertyChanged();
                return this._sourceCaptureDirectoryInfo;
            }
            internal set => this._sourceCaptureDirectoryInfo = value;
        }

        public DirectoryInfo SourceNetflowDirectoryInfo
        {
            get
            {
                if(this._sourceNetflowDirectoryInfo != null) { return this._sourceNetflowDirectoryInfo; }
                if(this.SourceDirectoryInfo == null) { return null; }
                this._sourceNetflowDirectoryInfo = new DirectoryInfo(Path.Combine(this.SourceDirectoryInfo.FullName, SourceNetflowDirectory));

                this.OnPropertyChanged();
                return this._sourceNetflowDirectoryInfo;
            }
        }

        public DirectoryInfo SourceLogsDirectoryInfo
        {
            get
            {
                if(this._sourceLogsDirectoryInfo != null) { return this._sourceLogsDirectoryInfo; }
                if(this.SourceDirectoryInfo == null) { return null; }
                this._sourceLogsDirectoryInfo = new DirectoryInfo(Path.Combine(this.SourceDirectoryInfo.FullName, SourceLogsDirectory));

                this.OnPropertyChanged();
                return this._sourceLogsDirectoryInfo;
            }
        }

        public DirectoryInfo SettingsDirectoryInfo
        {
            get
            {
                if(this._settingsDirectoryInfo != null) { return this._settingsDirectoryInfo; }
                if(this.InvestigationDirectoryInfo == null) { return null; }
                this._settingsDirectoryInfo = new DirectoryInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName, SettingsDirectory));

                this.OnPropertyChanged();
                return this._settingsDirectoryInfo;
            }
        }

        public DirectoryInfo TempDirectoryInfo
        {
            get
            {
                if(this._tempDirectoryInfo != null) { return this._tempDirectoryInfo; }
                if(this.InvestigationDirectoryInfo == null) { return null; }
                this._tempDirectoryInfo = new DirectoryInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName, TempDirectory));

                this.OnPropertyChanged();
                return this._tempDirectoryInfo;
            }
        }

        private DirectoryInfo _databaseDirectoryInfo;
        private DirectoryInfo _exportsDirectoryInfo;
        private DirectoryInfo _sourceDirectoryInfo;
        private DirectoryInfo _sourceCaptureDirectoryInfo;
        private DirectoryInfo _sourceNetflowDirectoryInfo;
        private DirectoryInfo _sourceLogsDirectoryInfo;
        private DirectoryInfo _settingsDirectoryInfo;
        private DirectoryInfo _tempDirectoryInfo;
        private FileInfo _databaseFileInfo;
        private string _investigationName;
        private DirectoryInfo _logsDirectoryInfo;

        public DirectoryInfo LogsDirectoryInfo
        {
            get
            {
                if(this._logsDirectoryInfo != null) { return this._logsDirectoryInfo; }
                if(this.InvestigationDirectoryInfo == null) { return null; }
                this._logsDirectoryInfo = new DirectoryInfo(Path.Combine(this.InvestigationDirectoryInfo.FullName, LogsDirectory));

                this.OnPropertyChanged();
                return this._logsDirectoryInfo;
            }
        }

        [DataMember]
        public bool IsInMemory { get; set; } = true;
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}