using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.SnooperFTP.WEB.DTO
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
