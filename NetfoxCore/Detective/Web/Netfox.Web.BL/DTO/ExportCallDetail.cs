using System;
using System.Collections.Generic;

namespace Netfox.Web.BL.DTO
{
    public class ExportCallDetailDTO
    {
        public DateTime FirstSeen { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationText { get; set; }
        public string CallId { get; set; }
        public List<string> RTPAddress { get; set; }

        public Guid Id { get; set; }
    }
}