using System;

namespace Netfox.Web.BL.DTO
{
    public class ExportFileMessageDTO
    {
        public DateTime FirstSeen { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
    }
}