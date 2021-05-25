using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IL3L4ConversationTrackerBlockFactory
    {
        L3L4ConversationTrackerBlock Create();
    }
}