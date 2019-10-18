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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks.Dataflow;
using Netfox.Framework.CaptureProcessor.Captures;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.CoreController
{
    public class ControllerCaptureProcessorLocal : ControllerCaptureProcessorBase
    {
        public IL7ConversationTrackerBlockFactory L7ConversationTrackerBlockFactory { get; }
        public ICaptureProcessorBlockFactory CaptureProcessorBlockFactory { get; }
        public IL3L4ConversationTrackerBlockFactory L3L4ConversationTrackerBlockFactory { get; }

        public  ControllerCaptureProcessorLocal(IL7ConversationTrackerBlockFactory l7ConversationTrackerBlockFactory, ICaptureProcessorBlockFactory captureProcessorBlockFactory, IL3L4ConversationTrackerBlockFactory l3L4ConversationTrackerBlockFactory  ) : base()
        {
            this.L7ConversationTrackerBlockFactory = l7ConversationTrackerBlockFactory;
            this.CaptureProcessorBlockFactory = captureProcessorBlockFactory;
            this.L3L4ConversationTrackerBlockFactory = l3L4ConversationTrackerBlockFactory;
        }

        private CaptureProcessorBlock CaptureProcessorBlock { get; set; }

        public override void ProcessCapturesInternal(IEnumerable<FileInfo> captureFiles)
        {
            this.CaptureProcessorBlock = this.CaptureProcessorBlockFactory.Create();


            var l3L4ConversationTracker = this.L3L4ConversationTrackerBlockFactory.Create();
            this.CaptureProcessorBlock.LinkTo(l3L4ConversationTracker, new DataflowLinkOptions() { PropagateCompletion = true });

            var l7ConversationTrackerBlock = this.L7ConversationTrackerBlockFactory.Create();
            l3L4ConversationTracker.LinkTo<PmFrameBase>(l7ConversationTrackerBlock);
            l3L4ConversationTracker.LinkTo<L4ConversationExtended>(l7ConversationTrackerBlock);

            l3L4ConversationTracker.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)l7ConversationTrackerBlock).Fault(t.Exception);
                else l7ConversationTrackerBlock.Complete();
            });


            foreach (var captureFile in captureFiles) { this.CaptureProcessorBlock.SendAsync(captureFile).Wait(); }
            this.CaptureProcessorBlock.Complete();
        }
    }
}