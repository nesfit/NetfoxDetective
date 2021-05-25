using System;
using System.Diagnostics;
using System.Net;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.L3L4ConversationTracking
{
    internal class L4ConversationExtended : L4Conversation
    {
        public L4ConversationExtended(IWindsorContainer container,
            IL7ConversationTrackerFactory conversationTrackerFactory, IPProtocolType ipProtocol,
            L3Conversation l3Conversation, IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint, long l4FlowMTU)
            : base(container, ipProtocol, l3Conversation, sourceEndPoint, destinationEndPoint, l4FlowMTU)
        {
            switch (this.L4ProtocolType)
            {
                case IPProtocolType.TCP:
                    this.L7ConversationTracker = conversationTrackerFactory.CreateTCPTracker(this);
                    break;
                case IPProtocolType.UDP:
                    this.L7ConversationTracker = conversationTrackerFactory.CreateUDPTracker(this);
                    break;
                default:
                    Debugger.Break();
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IL7ConversationTracker L7ConversationTracker { get; }

        public ConcurrentObservableHashSet<PmCaptureBase> CapturesHashSet { get; } =
            new ConcurrentObservableHashSet<PmCaptureBase>();
    }
}