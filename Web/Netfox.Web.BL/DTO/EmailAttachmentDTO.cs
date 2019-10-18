using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netfox.Web.BL.DTO
{
    public class EmailAttachmentDTO
    {
        public string ContentDispositionFileName { get; set; }
        public string ContentType { get; set; }
        public string ContentSubtype { get; set; }
        public string StoredContentFilePath { get; set; }

    }
}
