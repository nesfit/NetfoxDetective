using System;

namespace Netfox.Web.BL.DTO
{
    public class ExportChatMessageDTO
    {
        public DateTime FirstSeen { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public Guid Id { get; set; }

    }
}