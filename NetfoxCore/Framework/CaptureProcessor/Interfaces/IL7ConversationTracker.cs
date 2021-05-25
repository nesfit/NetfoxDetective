using System.Collections.Generic;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IL7ConversationTracker
    {
        L4Conversation L4Conversation { get; }
        IEnumerable<L7Conversation> Complete();
        IEnumerable<L7Conversation> ProcessPmFrame(PmFrameBase frame);
    }
}