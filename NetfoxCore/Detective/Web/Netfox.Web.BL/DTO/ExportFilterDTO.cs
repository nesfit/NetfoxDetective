using System;

namespace Netfox.Web.BL.DTO
{
    public class ExportFilterDTO
    {
        public DateTime DurationMin { get; set; }

        public DateTime DurationMax { get; set; }

        public string DurationFrom { get; set; }

        public string DurationTo { get; set; }

        public string SearchText { get; set; }
    }
}
