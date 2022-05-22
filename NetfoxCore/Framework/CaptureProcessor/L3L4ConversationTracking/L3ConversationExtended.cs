using System.Net;
using Castle.Windsor;
using Netfox.Core.Collections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;

namespace Netfox.Framework.CaptureProcessor.L3L4ConversationTracking
{
    internal class L3ConversationExtended : L3Conversation
    {
        public L3ConversationExtended(IWindsorContainer container, IPAddress ipAddress1, IPAddress ipAddress2) : base(
            container, ipAddress1, ipAddress2)
        {
        }

        public L3ConversationExtended(IWindsorContainer container, L3Conversation conversationKey) : base(container,
            conversationKey)
        {
        }

        public ConcurrentObservableHashSet<PmCaptureBase> CapturesHashSet { get; } =
            new ConcurrentObservableHashSet<PmCaptureBase>();
    }
}