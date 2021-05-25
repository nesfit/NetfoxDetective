using System;

namespace Netfox.Web.BL.DTO
{
    public class SnooperExportBaseDTO
    {
        public Guid Id { get; set; }
        public string Period { get; set; }
        public string SourceEndPoint { get; set; }
        public string DestinationEndPoint { get; set; }
        public string ExporterType { get; set; }
        public int TotalExportReportsCount { get; set; }
        public int ExportObjectsCount { get; set; }
    }
}