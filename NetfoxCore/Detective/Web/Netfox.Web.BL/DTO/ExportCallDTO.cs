using System;

namespace Netfox.Web.BL.DTO
{
    public class ExportCallDTO
    {
        public DateTime FirstSeen { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationText { get; set; }
        public Guid Id { get; set; }
    }
}