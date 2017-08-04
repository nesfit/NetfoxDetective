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

using Netfox.Core.Database;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.NetfoxFrameworkAPI.Interfaces;

namespace Netfox.NetfoxFrameworkAPI.Tests
{
    public class FrameworkBaseTests : UnitTestBaseSetupFixture
    {
       
        public IFrameworkController FrameworkController { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<PmCaptureBase> PmCaptures { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<PmFrameBase> PmFrames { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<L3Conversation> L3Conversations { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<L4Conversation> L4Conversations { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<L7Conversation> L7Conversations { get; private set; }
        public VirtualizingObservableDBSetPagedCollection<PmCaptureBase> PmCapturesDb { get; set; }

      
        

        public override void SetUpSQL()
        {
            base.SetUpSQL(); 
            this.SetUp();
        }

        public override void SetUpInMemory()
        {
            base.SetUpInMemory();
            this.SetUp();
        }
        
        private void SetUp()
        {
            this.FrameworkController = this.WindsorContainer.Resolve<IFrameworkController>();
            this.PmCaptures = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmCaptureBase>>();
            this.PmFrames = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<PmFrameBase>>();
            this.L3Conversations = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L3Conversation>>();
            this.L4Conversations = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L4Conversation>>();
            this.L7Conversations = this.WindsorContainer.Resolve<VirtualizingObservableDBSetPagedCollection<L7Conversation>>(new
            {
                eagerLoadProperties = new[]
                {
                    nameof(L7Conversation.UnorderedL7PDUs),
                    nameof(L7Conversation.ConversationFlowStatistics),
                    nameof(L7Conversation.L4Conversation),
                    $"{nameof(L7Conversation.UnorderedL7PDUs)}.{nameof(L7PDU.UnorderedFrameList)}"
                }
            });
        }

       
    }
}