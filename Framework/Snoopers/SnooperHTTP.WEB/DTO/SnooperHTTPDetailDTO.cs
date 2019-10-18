using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.SnooperHTTP.WEB.DTO
{
    public class SnooperHTTPDetailDTO
    {
        public SnooperHTTPListDTO Info { get; set; }

        public string Header { get; set; }

        public string Content { get; set; }

    }
}
