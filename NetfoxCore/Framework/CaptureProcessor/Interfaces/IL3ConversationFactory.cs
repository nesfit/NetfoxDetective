using System.Net;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.Models;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IL3ConversationFactory
    {
        L3ConversationExtended Create(IPAddress ipAddress1, IPAddress ipAddress2);
        L3ConversationExtended Create(L3Conversation conversationKey);
    }
}