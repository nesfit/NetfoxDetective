// Copyright (c) 2017 Jan Pluskal, Martin Mares, Martin Kmet
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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using Netfox.Core.Database;

namespace Netfox.Detective.Models.SourceLog
{
    [Persistent]
    public class SourceLog :  IEntity
    {
        private string FilePath { get; set; }
        private FileInfo _sourceLogFileInfo;
        private SourceLog() { }
        public SourceLog(string filePath)
        {
            this.FilePath = filePath;
            this._sourceLogFileInfo = new FileInfo(filePath);
        }

        public SourceLog(FileInfo sourceLogFileInfo)
        {
            this._sourceLogFileInfo = sourceLogFileInfo; 
            this.FirstSeen = DateTime.Now;
        }

        public FileInfo SourceLogFileInfo => this._sourceLogFileInfo ?? (this._sourceLogFileInfo = new FileInfo(this.FilePath));

        public string Name => this.SourceLogFileInfo?.Name;

        #region Implementation of IEntity
        [Key]
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();

        #region Implementation of IEntity
        public DateTime FirstSeen { get; set; }
        #endregion

        #endregion
    }
}