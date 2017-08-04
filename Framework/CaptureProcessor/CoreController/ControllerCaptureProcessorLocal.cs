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
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EntityFramework.BulkInsert.Extensions;
using Netfox.Core.Collections;
using Netfox.Framework.CaptureProcessor.Captures;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Persistence.JunctionTypes;

namespace Netfox.Framework.CaptureProcessor.CoreController
{
    internal  class ControllerCaptureProcessorLocal : ControllerCaptureProcessorBase
    {
        public IL7ConversationTrackerBlockFactory L7ConversationTrackerBlockFactory { get; }
        public ICaptureProcessorBlockFactory CaptureProcessorBlockFactory { get; }
        public INetfoxDBContextFactory NetfoxDBContextFactory { get; }
        public IL3L4ConversationTrackerBlockFactory L3L4ConversationTrackerBlockFactory { get; }

        public  ControllerCaptureProcessorLocal(IL7ConversationTrackerBlockFactory l7ConversationTrackerBlockFactory, ICaptureProcessorBlockFactory captureProcessorBlockFactory, INetfoxDBContextFactory netfoxDBContextFactory, IL3L4ConversationTrackerBlockFactory l3L4ConversationTrackerBlockFactory  ) : base()
        {
            this.L7ConversationTrackerBlockFactory = l7ConversationTrackerBlockFactory;
            this.CaptureProcessorBlockFactory = captureProcessorBlockFactory;
            this.NetfoxDBContextFactory = netfoxDBContextFactory;
            this.L3L4ConversationTrackerBlockFactory = l3L4ConversationTrackerBlockFactory;
        }

        private CaptureProcessorBlock CaptureProcessorBlock { get; set; }
        
        public override void ProcessCapturesInternal(IEnumerable<FileInfo> captureFiles)
        {
            this.CaptureProcessorBlock = this.CaptureProcessorBlockFactory.Create();
            Task storeCapturesAwait;
            Task storeL3Await;
            Task storeL3StatisticsAwait;
            Task storeL4Await;
            Task storeL4StatisticsAwait;
            Task storeL7Await;
            Task storeL7StatisticsAwait;
            Task storeFramesAwait;
            Task storeL7PdusAwait;
            Task pmCaptureL3ConversationAwait;
            Task pmCaptureL4ConversationAwait;
            Task pmCaptureL7ConversationAwait;
            var storeCapturesBlock = this.CreateStoreActionBlock<PmCaptureBase>(out storeCapturesAwait);
            var storeL3ConversationBlock = this.CreateStoreActionBlock<L3Conversation>(out storeL3Await);
            var storeL3StatisticsBlock = this.CreateStoreActionBlock<L3ConversationStatistics>(out storeL3StatisticsAwait);
            var storeL4ConversationBlock = this.CreateStoreActionBlock<L4Conversation>(out storeL4Await);
            var storeL4StatisticsBlock = this.CreateStoreActionBlock<L4ConversationStatistics>(out storeL4StatisticsAwait);
            var storeL7ConversationBlock = this.CreateStoreActionBlock<L7Conversation>(out storeL7Await);
            var storeL7StatisticsBlock = this.CreateStoreActionBlock<L7ConversationStatistics>(out storeL7StatisticsAwait);
            var storeFramesBlock = this.CreateStoreActionBlock<PmFrameBase>(out storeFramesAwait);
            var storeL7PdusBlock = this.CreateStoreActionBlock<L7PDU>(out storeL7PdusAwait);
            var storePmCaptureL3ConversationBlock = this.CreateStoreToJunctionActionBlock<PmCaptureL3Conversation>(out pmCaptureL3ConversationAwait);
            var storePmCaptureL4ConversationBlock = this.CreateStoreToJunctionActionBlock<PmCaptureL4Conversation>(out pmCaptureL4ConversationAwait);
            var storePmCaptureL7ConversationBlock = this.CreateStoreToJunctionActionBlock<PmCaptureL7Conversation>(out pmCaptureL7ConversationAwait);

            this.CaptureProcessorBlock.LinkTo(storeCapturesBlock, new DataflowLinkOptions(){PropagateCompletion = true});

            var l3L4ConversationTracker = this.L3L4ConversationTrackerBlockFactory.Create();
            this.CaptureProcessorBlock.LinkTo(l3L4ConversationTracker, new DataflowLinkOptions() { PropagateCompletion = true });
            l3L4ConversationTracker.LinkTo(storeL3ConversationBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l3L4ConversationTracker.LinkTo(storePmCaptureL3ConversationBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l3L4ConversationTracker.LinkTo(storeL3StatisticsBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l3L4ConversationTracker.LinkTo(storePmCaptureL4ConversationBlock, new DataflowLinkOptions() { PropagateCompletion = true });

            var l7ConversationTrackerBlock = this.L7ConversationTrackerBlockFactory.Create();
            l3L4ConversationTracker.LinkTo<PmFrameBase>(l7ConversationTrackerBlock);
            l3L4ConversationTracker.LinkTo(storeL4StatisticsBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l3L4ConversationTracker.LinkTo<L4ConversationExtended>(l7ConversationTrackerBlock);
            
            l3L4ConversationTracker.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock) l7ConversationTrackerBlock).Fault(t.Exception);
                else l7ConversationTrackerBlock.Complete();
            });
            
            l7ConversationTrackerBlock.LinkTo(storeFramesBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l7ConversationTrackerBlock.LinkTo(storeL7ConversationBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l7ConversationTrackerBlock.LinkTo(storeL7StatisticsBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l7ConversationTrackerBlock.LinkTo(storeL4ConversationBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l7ConversationTrackerBlock.LinkTo(storeL7PdusBlock, new DataflowLinkOptions() { PropagateCompletion = true });
            l7ConversationTrackerBlock.LinkTo(storePmCaptureL7ConversationBlock, new DataflowLinkOptions() { PropagateCompletion = true });

            foreach (var captureFile in captureFiles) { this.CaptureProcessorBlock.SendAsync(captureFile).Wait(); }
            this.CaptureProcessorBlock.Complete();

            Task.WhenAll(
                storeCapturesAwait, 
                storeL3Await,
                storeL3StatisticsAwait, 
                storeL4Await, 
                storeL4StatisticsAwait, 
                storeL7Await, 
                storeL7StatisticsAwait,
                storeFramesAwait, 
                storeL7PdusAwait,
                pmCaptureL3ConversationAwait,
                pmCaptureL4ConversationAwait,
                pmCaptureL7ConversationAwait
                ).Wait();
        }
        protected BatchBlock<T> CreateStoreToJunctionActionBlock<T>(out Task dbxStore) where T : class
        {

           var buffer = new BatchBlock<T>(5000);
            var store = new ActionBlock<T[]>(async batch =>
            {
                using (var dbx = this.NetfoxDBContextFactory.Create())
                {
                    dbx.InsertToJunctionTable(batch);
                    await dbx.SaveChangesAsync();
                }
            });
            buffer.LinkTo(store, new DataflowLinkOptions(){PropagateCompletion = true});
            dbxStore = store.Completion;
            return buffer;
        }

        protected ActionBlock<T> CreateStoreActionBlock<T>(out Task dbxStore) where T : class
        {
            var buffer = new SynchronizedBlockingCollection<T>();
            var store = new ActionBlock<T>(item => { buffer.Add(item); });
            store.Completion.ContinueWith(_ => buffer.CompleteAdding());
            dbxStore = this.CreateDbxStore(buffer);
            return store;
        }


        private Task CreateDbxStore<T>(SynchronizedBlockingCollection<T> buffer) where T : class
        {
            return Task.Run(async () =>
            {
                using(var dbx = this.NetfoxDBContextFactory.Create())
                {
                    await dbx.BulkInsertBuffered(buffer.GetConsumingEnumerable(), this.GetBulkInsertOptions());
                    await dbx.SaveChangesAsync();
                }
            });
        }

        private BulkInsertOptions GetBulkInsertOptions()
        {
            return new BulkInsertOptions
            {
                EnableStreaming = true,
                BatchSize = 50000,
                SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity //| SqlBulkCopyOptions.TableLock //| SqlBulkCopyOptions.KeepNulls
            };
        }
        
    }
}