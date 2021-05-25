using System.Net;

namespace Netfox.Detective.Models.Conversations
{
    public class HostTraffic
    {
        public long DownTraffic { get; set; }
        public long UpTraffic { get; set; }
        public IPAddress Host { get; set; }
        public long TotalTraffic => this.DownTraffic + this.UpTraffic;
    }
}