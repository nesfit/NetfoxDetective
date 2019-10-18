using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.SnooperHTTP.WEB.DTO
{
    public class SnooperHTTPFileDTO
    {
        public DateTime TimeStamp { get; set; }

        public List<Guid> FrameGuids { get; set; }

        public string SourceEndPoint { get; set; }

        public string DestinationEndPoint { get; set; }

        public string StatusLine { get; set; }

        public string ContentType { get; set; }

        public string Size { get; set; }

        public string Path { get; set; }

        public string Url { get; set; }

    }
}
