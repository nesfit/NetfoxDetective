namespace Netfox.Snoopers.SnooperDNS.WEB.DTO
{
    public class SnooperDNSDetailDTO
    {
        public SnooperDNSListDTO Info { get; set; }

        public string Queries { get; set; }

        public string Answer { get; set; }

        public string Authority { get; set; }

        public string Additional { get; set; }

    }
}
