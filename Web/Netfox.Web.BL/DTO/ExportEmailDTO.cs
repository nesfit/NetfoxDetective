using System;
using System.Net;

namespace Netfox.Web.BL.DTO
{
    public class ExportEmailDTO
    {
        public string Date { get; set; }
        public DateTime Timestamp { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public string RawContent { get; set; }
        public string StoredHeadersFilePath { get; set; }
        public string StoredContentFilePath { get; set; }
        public string ContentTypeCharset { get; set; }
        public string ContentType { get; set; }
        public string ContentSubtype { get; set; }
        public string MessageID { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public Guid Id { get; set; }
    }
}