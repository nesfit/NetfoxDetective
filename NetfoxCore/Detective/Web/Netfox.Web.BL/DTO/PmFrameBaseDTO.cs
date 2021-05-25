using System;

namespace Netfox.Web.BL.DTO
{
    public class PmFrameBaseDTO
    {
        public Guid Id { get; set; }
        public Int64 FrameIndex { get; set; }
        public DateTime FirstSeen { get; set; }
        public string SourceEndPoint { get; set; }
        public string DestinationEndPoint { get; set; }
        public string IpProtocol { get; set; }
        public Int64 OriginalLength { get; set; }


    }
}