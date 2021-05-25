using System;
using Riganti.Utils.Infrastructure.Core;

namespace Netfox.Web.DAL.Entities
{
    public class CaptureStats : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid InvestigationId { get; set; }
        public virtual Investigation Investigation { get; set; }
        public DateTime CaptureStart { get; set;}
        public DateTime CaptureFinish { get; set; }
        public long Size { get; set; }
        public long Frames { get; set; }
        public long IPv4Conversations { get; set; }
        public long IPv6Conversations { get; set; }
        public long UniqueHostsCount { get; set; }
        public long ReckognizedProtocolsCount { get; set; }
        public long UpFlowFrames { get; set; }
        public long DownFlowFrames { get; set; }
        public long UpFlowBytes { get; set; }
        public long DownFlowBytes { get; set; }
        public long TcpConversations { get; set; }
        public long TotalTcpBytes { get; set; }
        public long UdpConversations { get; set; }
        public long TotalUdpBytes { get; set; }
        public long UpFlowTcpLostBytes { get; set; }
        public long DownFlowTcpLostBytes { get; set; }
        public string AppProtocolsDistribution { get; set; }
        public string AppProtocolsSummary { get; set; }
        public string TransportProtocolsDistribution { get; set; }
        public string TransportProtocolsSummary { get; set; }
        public string LinkProtocolsDistribution { get; set; }
        public string LinkProtocolsSummary { get; set; }
        public string TrafficErrors { get; set; }
        public string TrafficHistory { get; set; }

        /* Filter limits */
        public DateTime FilterDurationMin { get; set; }
        public DateTime FilterDurationMax { get; set; }

        public long L3FramesMin { get; set; }
        public long L3FramesMax { get; set; }
        public long L3BytesMin { get; set; }
        public long L3BytesMax { get; set; }

        public long L4FramesMin { get; set; }
        public long L4FramesMax { get; set; }
        public long L4BytesMin { get; set; }
        public long L4BytesMax { get; set; }

        public long L7FramesMin { get; set; }
        public long L7FramesMax { get; set; }
        public long L7BytesMin { get; set; }
        public long L7BytesMax { get; set; }

        public long FramesBytesMin { get; set; }
        public long FramesBytesMax { get; set; }

        /* Investigation stats */

        public long TotalL3Conversations { get; set; }
        public long TotalL4Conversations { get; set; }
        public long TotalL7Conversations { get; set; }
        public long TotalFrames { get; set; }

        public string ExportedProtocols { get; set; }

    }
}