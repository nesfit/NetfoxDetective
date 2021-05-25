namespace Netfox.Detective.Messages.Conversations
{
    class AddConversationToExportMessage
    {
        public object ConversationVm { get; set; }
        public bool BringToFront { get; set; }
    }
}