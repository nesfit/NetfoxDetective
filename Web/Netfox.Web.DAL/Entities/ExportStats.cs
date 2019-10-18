using System;
using System.ComponentModel.DataAnnotations.Schema;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.DAL.Entities
{
    public class ExportStats : IEntity<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public long TotalExportedObject { get; set; }
        public long TotalCalls { get; set; }
        public long TotalMessage { get; set; }
        public long TotalEmail { get; set; }
        public long TotalOther { get; set; }

    }
}