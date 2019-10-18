// Copyright (c) 2017 Jan Pluskal
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using Castle.Windsor;
using Netfox.Framework.Models.Interfaces;
using Netfox.NBARDatabase;

namespace Netfox.Framework.Models.Services
{
    public abstract class ApplicationRecognizerBase : IApplicationRecognizer
    {
        protected ApplicationRecognizerBase(NBARProtocolPortDatabase nbarProtocolPortDatabase) { this.NBARProtocolPortDatabase = nbarProtocolPortDatabase; }

        public NBARProtocolPortDatabase NBARProtocolPortDatabase { get; set; }
        public WindsorContainer ControllerCaptureProcessorWindsorContainer { get; set; }
        public abstract String Description { get; }
        public NBAR2TaxonomyProtocol GetNbar2TaxonomyProtocol(String protocolName) => this.NBARProtocolPortDatabase.GetNbar2TaxonomyProtocol(protocolName);

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