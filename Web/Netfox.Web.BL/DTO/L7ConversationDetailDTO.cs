namespace Netfox.Web.BL.DTO
{
    public class L7ConversationDetailDTO : L7ConversationDTO
    {
        public L3ConversationDTO L3Conversation { get; set; }

        public L4ConversationDTO L4Conversation { get; set; }
    }
}