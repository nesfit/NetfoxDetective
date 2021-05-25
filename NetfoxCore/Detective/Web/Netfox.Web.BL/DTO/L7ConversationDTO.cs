namespace Netfox.Web.BL.DTO
{
    public class L7ConversationDTO : LxConversationBaseDTO
    {
        public string SourceEndPoint { get; set; }
        public string DestinationEndPoint { get; set; }
        public string Application { get; set; }
        public long? ExtractedBytes { get; set; }
        public long? MissingBytes { get; set; }
        public long? MissingFrames { get; set; }
    }
}