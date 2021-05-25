namespace Netfox.Web.BL.DTO
{
    public class ExportStatisticsDTO
    {
        public long TotalExportedObject { get; set; }
        public long TotalCalls { get; set; }
        public long TotalMessage { get; set; }
        public long TotalEmail { get; set; }
        public long TotalOther { get; set; }
    }
}