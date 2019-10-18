using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Web.BL.DTO
{
    public class ConversationFilterDTO
    {
        public long FrameMax { get; set; }
        public long FrameMin { get; set; } 
        public long BytesMax { get; set; } 
        public long BytesMin { get; set; }

        public DateTime DurationMin { get; set; }
        public DateTime DurationMax { get; set; }

        public string DurationFrom { get; set; }
        public string DurationTo { get; set; }

        public long BytesFrom { get; set; }
        public long BytesTo { get; set; }
        public long FramesFrom { get; set; }
        public long FramesTo { get; set; }
        public string SearchText { get; set; }


    }
}
