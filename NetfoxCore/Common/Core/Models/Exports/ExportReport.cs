using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Netfox.Core.Attributes;
using Netfox.Core.Database;
using Netfox.Core.Enums;
using Netfox.Core.Interfaces;
using Netfox.Core.Interfaces.Model.Exports;

namespace Netfox.Core.Models.Exports
{
    public class ExportReport : IQueryableClass, IEntity
    {
        #region Implementation of IEntity

        [Key] public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime FirstSeen { get; }

        #endregion

        public enum ReportLevel
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }

        [QueryableProperty] [NotMapped] public ReportLevel Level { get; set; }

        [QueryableProperty] [NotMapped] public IExportSource ExportSource => this?.ExportSources?.FirstOrDefault();

        [QueryableProperty] public string Description { get; set; }

        [QueryableProperty] public string Detail { get; set; }

        public string Detail2 { get; set; }
        [NotMapped] public uint[] ConcernedFrameNumbers { get; set; }
        [NotMapped] public Exception Exception { get; set; }
        [NotMapped] public IEnumerable<IExportSource> ExportSources { get; set; }
        public string SourceComponent { get; set; }


        //TODO: old - fix to L7PDU

        public static ReportLevel ReportLevelConv(FcLogLevel level)
        {
            switch (level)
            {
                case FcLogLevel.Debug:
                    return ReportLevel.Debug;
                case FcLogLevel.Info:
                    return ReportLevel.Info;
                case FcLogLevel.Warn:
                    return ReportLevel.Warn;
                case FcLogLevel.Error:
                    return ReportLevel.Error;
                case FcLogLevel.Fatal:
                    return ReportLevel.Fatal;
            }

            return ReportLevel.Debug;
        }

        public new virtual string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("    level: " + this.Level);
            sb.AppendLine("    Source: " + this.SourceComponent);
            sb.AppendLine("    ExportSource: " + this.ExportSource);
            sb.AppendLine("    Description: " + this.Description);
            sb.AppendLine("    Detail: " + this.Detail);
            sb.AppendLine("    Detail2: " + this.Detail2);
            sb.AppendLine("    Exception.Message: " + this.Exception?.Message);
            sb.Append("    Exception.StackTrace: " + this.Exception?.StackTrace);
            return sb.ToString();
        }
    }
}