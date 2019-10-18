using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Web.BL.DTO
{
    public class PmFrameBaseDetailDTO : PmFrameBaseDTO
    {
        public L3ConversationDTO L3Conversation { get; set; }

        public L4ConversationDTO L4Conversation { get; set; }

        public L7ConversationDTO L7Conversation { get; set; }

        public byte[] Data { get; set; }
    }
}
