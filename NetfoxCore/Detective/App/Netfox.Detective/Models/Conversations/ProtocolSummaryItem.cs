namespace Netfox.Detective.Models.Conversations
{
    public class ProtocolSummaryItem
    {
        public ProtocolSummaryItem(string name, long totalBytes, float percent)
        {
            this.Name = name;
            this.TotalBytes = totalBytes;
            this.Percent = percent;
        }

        public string Name { get; set; }
        public long TotalBytes { get; set; }
        public float Percent { get; set; }
    }
}