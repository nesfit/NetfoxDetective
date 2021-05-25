using System;

namespace Netfox.Web.BL.DTO
{
    public class LxConversationBaseDTO
    {
        public Guid Id { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public string Transport { get; set; }
        public long UpFlowFramesCount { get; set; }
        public long DownFlowFramesCount { get; set; }
        public long UpFlowBytes { get; set; }
        public long DownFlowBytes { get; set; }
        public long? MalformedFrames { get; set; }
    }
}