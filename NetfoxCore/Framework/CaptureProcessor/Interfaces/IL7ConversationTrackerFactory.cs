using Netfox.Framework.CaptureProcessor.L7Tracking.TCP;
using Netfox.Framework.CaptureProcessor.L7Tracking.UDP;
using Netfox.Framework.Models;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IL7ConversationTrackerFactory
    {
        TCPTracker CreateTCPTracker(L4Conversation l4Conversation);
        UDPTracker CreateUDPTracker(L4Conversation l4Conversation);
    }
}