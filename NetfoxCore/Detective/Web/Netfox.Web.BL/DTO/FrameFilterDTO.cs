using System;

namespace Netfox.Web.BL.DTO
{
    public class FrameFilterDTO
    {
        public long BytesMax { get; set; } 
        public long BytesMin { get; set; }

        public DateTime DurationMin { get; set; }
        public DateTime DurationMax { get; set; }

        public string DurationFrom { get; set; }
        public string DurationTo { get; set; }

        public long BytesFrom { get; set; }
        public long BytesTo { get; set; }
        public string SearchText { get; set; }


    }
}
