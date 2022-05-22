namespace Netfox.Detective.Messages.Conversations
{
    class ChangedCurrentConversationMessage
    {
        public object ConversationVm { get; set; }
        public bool BringToFront { get; set; }
    }
}