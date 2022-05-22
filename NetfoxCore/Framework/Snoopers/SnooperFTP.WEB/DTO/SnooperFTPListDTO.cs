using System;

namespace Netfox.Snoopers.SnooperFTP.WEB.DTO
{
    public class SnooperFTPListDTO
    {
        public DateTime FirstSeen { get; set; }

        public string SourceEndpointString { get; set; }

        public string DestinationEndpointString { get; set; }

        public string Command { get; set; }

        public string Value { get; set; }
    }
}
