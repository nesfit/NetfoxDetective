using System.Net;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.Models;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.Interfaces
{
    internal interface IL4ConversationFactory
    {
        L4ConversationExtended Create(IPProtocolType ipProtocol, L3Conversation l3Conversation,
            IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint, long l4FlowMTU);
    }
}