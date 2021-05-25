using System;

namespace Netfox.Snoopers.SnooperDNS.WEB.DTO
{
    public class SnooperDNSListDTO
    {
        public Guid Id { get; set; }

        public DateTime FirstSeen { get; set; }

        public string SourceEndPoint { get; set; }

        public string DestinationEndPoint { get; set; }

        public int MessageId { get; set; }

        public int Flags { get; set; }

        public string QueryType { get; set; }

        public string ResponseCode { get; set; }

        public bool IsAuthoritativeAnswer { get; set; }

        public bool IsTrunCation { get; set; }

        public bool IsRecursionDesired { get; set; }

        public bool IsRecursionAvailable { get; set; }
    }
}
