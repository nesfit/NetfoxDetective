using Netfox.Framework.CaptureProcessor.L7Tracking;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IL7ConversationTrackerBlockFactory
    {
        L7ConversationTrackerBlock Create();
    }
}