using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.SnooperHTTP.WEB.DTO
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
