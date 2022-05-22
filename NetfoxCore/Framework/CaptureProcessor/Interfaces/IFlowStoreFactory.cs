using System;
using Netfox.Framework.CaptureProcessor.L7Tracking;
using Netfox.Framework.Models;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IFlowStoreFactory
    {
        FlowStore Create(L4Conversation l4Conversation, TimeSpan flowMatchingTimeSpan);
    }
}