using System;

namespace Netfox.Snoopers.SnooperHTTP.WEB.DTO
{
    public class SnooperHTTPListDTO
    {
        public Guid Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public string SourceEndPoint { get; set; }

        public string DestinationEndPoint { get; set; }

        public string MessageType { get; set; }

        public string StatusLine { get; set; }
    }
}
