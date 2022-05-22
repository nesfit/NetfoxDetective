using System;
using System.Collections.Generic;
using Castle.Windsor;
using Netfox.Framework.Models.Interfaces;
using Netfox.NBARDatabase;

namespace Netfox.Framework.Models.Services
{
    public abstract class ApplicationRecognizerBase : IApplicationRecognizer
    {
        protected ApplicationRecognizerBase(NBARProtocolPortDatabase nbarProtocolPortDatabase)
        {
            this.NBARProtocolPortDatabase = nbarProtocolPortDatabase;
        }

        public NBARProtocolPortDatabase NBARProtocolPortDatabase { get; set; }
        public WindsorContainer ControllerCaptureProcessorWindsorContainer { get; set; }
        public abstract String Description { get; }

        public NBAR2TaxonomyProtocol GetNbar2TaxonomyProtocol(String protocolName) =>
            this.NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(protocolName);

        public abstract String Name { get; }
        public abstract UInt32 Priority { get; }

        public virtual IEnumerable<L7Conversation> RecognizeAndUpdateConversation(L7Conversation conversation)
        {
            conversation.ApplicationProtocols = this.RecognizeConversation(conversation);
            return new[]
            {
                conversation
            };
        }

        public abstract String Type { get; }

        public abstract IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation);
    }
}