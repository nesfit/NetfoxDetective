using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Netfox.Framework.Models.Snoopers.Email
{
    public class SnooperExportEmailBase: SnooperExportBase
    {
        [NotMapped] //TODO: old - fix (what?)
        public DirectoryInfo DirectoryInfo { get; private set; }
        protected SnooperExportEmailBase() { } //EF
        protected SnooperExportEmailBase(DirectoryInfo directoryInfo)
        {
            this.DirectoryInfo = directoryInfo;
        }
    }
}