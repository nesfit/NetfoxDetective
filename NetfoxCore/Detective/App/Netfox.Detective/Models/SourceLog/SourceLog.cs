using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using Netfox.Core.Database;

namespace Netfox.Detective.Models.SourceLog
{
    [Persistent]
    public class SourceLog : IEntity
    {
        private string FilePath { get; set; }
        private FileInfo _sourceLogFileInfo;

        private SourceLog()
        {
        }

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

        public FileInfo SourceLogFileInfo =>
            this._sourceLogFileInfo ?? (this._sourceLogFileInfo = new FileInfo(this.FilePath));

        public string Name => this.SourceLogFileInfo?.Name;

        #region Implementation of IEntity

        [Key] [DataMember] public Guid Id { get; set; } = Guid.NewGuid();

        #region Implementation of IEntity

        public DateTime FirstSeen { get; set; }

        #endregion

        #endregion
    }
}