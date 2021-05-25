using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Castle.Core.Internal;
using EntityFramework.BulkInsert.Extensions;
using Netfox.Core.Collections;
using Netfox.Core.Infrastructure;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.CaptureProcessor.L7PDUTracking.DVB;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Persistence.JunctionTypes;
using PostSharp.Patterns.Contracts;

namespace Netfox.Framework.CaptureProcessor.CoreController
{
    /// <remarks>
    /// Uses <a href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library">
    /// Dataflow components</a> of Task Parallel Library.
    /// </remarks>
    internal class ControllerCaptureProcessorLocal : ControllerCaptureProcessorBase
        {

        public IL7ConversationTrackerBlockFactory L7ConversationTrackerBlockFactory { get; }
        public ICaptureProcessorBlockFactory CaptureProcessorBlockFactory { get; }
        public INetfoxDBContextFactory NetfoxDBContextFactory { get; }
        public IL3L4ConversationTrackerBlockFactory L3L4ConversationTrackerBlockFactory { get; }

        private readonly INetfoxSettings _settings;
        
        public ControllerCaptureProcessorLocal(
            IL7ConversationTrackerBlockFactory l7ConversationTrackerBlockFactory,
            ICaptureProcessorBlockFactory captureProcessorBlockFactory,
            INetfoxDBContextFactory netfoxDBContextFactory,
            IL3L4ConversationTrackerBlockFactory l3L4ConversationTrackerBlockFactory,
            INetfoxSettings settings)
        {
            this.L7ConversationTrackerBlockFactory = l7ConversationTrackerBlockFactory;
            this.CaptureProcessorBlockFactory = captureProcessorBlockFactory;
            this.NetfoxDBContextFactory = netfoxDBContextFactory;
            this.L3L4ConversationTrackerBlockFactory = l3L4ConversationTrackerBlockFactory;
            _settings = settings;
        }

        public override void ProcessCapturesInternal(IEnumerable<FileInfo> captureFiles)
        {
            var storeCapturesBlock = this.CreateStoreActionBlock<PmCaptureBase>(out Task storeCapturesAwait);
            var storeL3ConversationBlock = this.CreateStoreActionBlock<L3Conversation>(out Task storeL3Await);
            var storeL3StatisticsBlock =
                this.CreateStoreActionBlock<L3ConversationStatistics>(out var storeL3StatisticsAwait);
            var storeL4ConversationBlock = this.CreateStoreActionBlock<L4Conversation>(out Task storeL4Await);
            var storeL4StatisticsBlock =
                this.CreateStoreActionBlock<L4ConversationStatistics>(out Task storeL4StatisticsAwait);
            var storeL7ConversationBlock = this.CreateStoreActionBlock<L7Conversation>(out Task storeL7Await);
            var storeL7StatisticsBlock =
                this.CreateStoreActionBlock<L7ConversationStatistics>(out Task storeL7StatisticsAwait);
            var storeFramesBlock = this.CreateStoreActionBlock<PmFrameBase>(out Task storeFramesAwait);
            var storeL7PdusBlock = this.CreateStoreActionBlock<L7PDU>(out Task storeL7PdusAwait);
            var storePmCaptureL3ConversationBlock =
                this.CreateStoreToJunctionActionBlock<PmCaptureL3Conversation>(out Task pmCaptureL3ConversationAwait);
            var storePmCaptureL4ConversationBlock =
                this.CreateStoreToJunctionActionBlock<PmCaptureL4Conversation>(out Task pmCaptureL4ConversationAwait);
            var storePmCaptureL7ConversationBlock =
                this.CreateStoreToJunctionActionBlock<PmCaptureL7Conversation>(out Task pmCaptureL7ConversationAwait);

            var captureProcessorBlock = this.CaptureProcessorBlockFactory.Create();
            captureProcessorBlock.LinkTo(storeCapturesBlock, new DataflowLinkOptions() {PropagateCompletion = true});

            var l3L4ConversationTracker = this.L3L4ConversationTrackerBlockFactory.Create();
            captureProcessorBlock.LinkTo(l3L4ConversationTracker,
                new DataflowLinkOptions() {PropagateCompletion = true});

            l3L4ConversationTracker.LinkTo(storeL3ConversationBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l3L4ConversationTracker.LinkTo(storePmCaptureL3ConversationBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l3L4ConversationTracker.LinkTo(storeL3StatisticsBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l3L4ConversationTracker.LinkTo(storePmCaptureL4ConversationBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l3L4ConversationTracker.LinkTo(storeL4StatisticsBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});

            var l7ConversationTrackerBlock = this.L7ConversationTrackerBlockFactory.Create();
            l3L4ConversationTracker.LinkTo<PmFrameBase>(l7ConversationTrackerBlock);
            l3L4ConversationTracker.LinkTo<L4ConversationExtended>(l7ConversationTrackerBlock);

            l3L4ConversationTracker.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock) l7ConversationTrackerBlock).Fault(t.Exception);
                else l7ConversationTrackerBlock.Complete();
            });

            var l7ConversationBroadcaster = new BroadcastBlock<L7Conversation>(null);
            l7ConversationTrackerBlock.LinkTo(l7ConversationBroadcaster,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l7ConversationTrackerBlock.LinkTo(storeFramesBlock, new DataflowLinkOptions() {PropagateCompletion = true});
            l7ConversationTrackerBlock.LinkTo(storeL7StatisticsBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l7ConversationTrackerBlock.LinkTo(storeL4ConversationBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            l7ConversationTrackerBlock.LinkTo(storeL7PdusBlock, new DataflowLinkOptions() {PropagateCompletion = true});
            l7ConversationTrackerBlock.LinkTo(storePmCaptureL7ConversationBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});

            l7ConversationBroadcaster.LinkTo(storeL7ConversationBlock,
                new DataflowLinkOptions() {PropagateCompletion = true});
            L7DvbS2GseDecapsulatorBlock l7DvbS2GseDecapsulator = null;
            BufferBlock<PmFrameBase> decapsulatedFrames = null;
            if (_settings.DecapsulateGseOverUdp)
            {
                l7DvbS2GseDecapsulator = new L7DvbS2GseDecapsulatorBlock();
                decapsulatedFrames = new BufferBlock<PmFrameBase>();
                l7ConversationBroadcaster.LinkTo(l7DvbS2GseDecapsulator,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l7DvbS2GseDecapsulator.LinkTo(decapsulatedFrames,
                    new DataflowLinkOptions() {PropagateCompletion = true});
            }

            foreach (var captureFile in captureFiles)
            {
                captureProcessorBlock.SendAsync(captureFile).Wait();
            }

            captureProcessorBlock.Complete();

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
            if (!_settings.DecapsulateGseOverUdp) return;

            l7DvbS2GseDecapsulator?.Completion.Wait();
            if (decapsulatedFrames != null && decapsulatedFrames.TryReceiveAll(out var frames))
            {
                this.ProcessFramesInternal(frames);
            }
        }

        /// <summary>
        /// Create pipeline of custom TPL blocks and process provided <paramref name="frames"/>.
        /// Results of parsing and tracking are stored using custom ActionBlocks obtained from
        /// <see cref="CreateStoreActionBlock{T}"/> and <see cref="CreateStoreToJunctionActionBlock{T}"/> methods. Should
        /// any encapsulated frames (tunnels) be found, they are decapsulated and stored in local variable
        /// <c>BufferBlock&lt;PmFrameBase&gt; decapsulatedFrames</c>. In case of traffic tunneled inside several
        /// layers of tunnels, body of this method iterates to process each tunnel.
        /// </summary>
        /// <param name="frames">Frames to be processed.</param>
        /// <seealso cref="ProcessCapturesInternal"/>
        private void ProcessFramesInternal([Required] IList<PmFrameBase> frames)
        {
            while (!frames.IsNullOrEmpty())
            {
                var storeL3ConversationBlock = this.CreateStoreActionBlock<L3Conversation>(out Task storeL3Await);
                var storeL3StatisticsBlock =
                    this.CreateStoreActionBlock<L3ConversationStatistics>(out var storeL3StatisticsAwait);
                var storeL4ConversationBlock = this.CreateStoreActionBlock<L4Conversation>(out Task storeL4Await);
                var storeL4StatisticsBlock =
                    this.CreateStoreActionBlock<L4ConversationStatistics>(out Task storeL4StatisticsAwait);
                var storeL7ConversationBlock = this.CreateStoreActionBlock<L7Conversation>(out Task storeL7Await);
                var storeL7StatisticsBlock =
                    this.CreateStoreActionBlock<L7ConversationStatistics>(out Task storeL7StatisticsAwait);
                var storeFramesBlock = this.CreateStoreActionBlock<PmFrameBase>(out Task storeFramesAwait);
                var storeL7PdusBlock = this.CreateStoreActionBlock<L7PDU>(out Task storeL7PdusAwait);
                var storePmCaptureL3ConversationBlock =
                    this.CreateStoreToJunctionActionBlock<PmCaptureL3Conversation>(
                        out Task pmCaptureL3ConversationAwait);
                var storePmCaptureL4ConversationBlock =
                    this.CreateStoreToJunctionActionBlock<PmCaptureL4Conversation>(
                        out Task pmCaptureL4ConversationAwait);
                var storePmCaptureL7ConversationBlock =
                    this.CreateStoreToJunctionActionBlock<PmCaptureL7Conversation>(
                        out Task pmCaptureL7ConversationAwait);

                var l3L4ConversationTracker = this.L3L4ConversationTrackerBlockFactory.Create();

                l3L4ConversationTracker.LinkTo(storeL3ConversationBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l3L4ConversationTracker.LinkTo(storePmCaptureL3ConversationBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l3L4ConversationTracker.LinkTo(storeL3StatisticsBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l3L4ConversationTracker.LinkTo(storePmCaptureL4ConversationBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});

                var l7ConversationTrackerBlock = this.L7ConversationTrackerBlockFactory.Create();
                l3L4ConversationTracker.LinkTo<PmFrameBase>(l7ConversationTrackerBlock);
                l3L4ConversationTracker.LinkTo(storeL4StatisticsBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l3L4ConversationTracker.LinkTo<L4ConversationExtended>(l7ConversationTrackerBlock);

                l3L4ConversationTracker.Completion.ContinueWith(t =>
                {
                    if (t.IsFaulted) ((IDataflowBlock) l7ConversationTrackerBlock).Fault(t.Exception);
                    else l7ConversationTrackerBlock.Complete();
                });

                var l7ConversationBroadcaster = new BroadcastBlock<L7Conversation>(null);
                l7ConversationTrackerBlock.LinkTo(l7ConversationBroadcaster,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l7ConversationTrackerBlock.LinkTo(storeFramesBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l7ConversationTrackerBlock.LinkTo(storeL7StatisticsBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l7ConversationTrackerBlock.LinkTo(storeL4ConversationBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l7ConversationTrackerBlock.LinkTo(storeL7PdusBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                l7ConversationTrackerBlock.LinkTo(storePmCaptureL7ConversationBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});

                l7ConversationBroadcaster.LinkTo(storeL7ConversationBlock,
                    new DataflowLinkOptions() {PropagateCompletion = true});
                L7DvbS2GseDecapsulatorBlock l7DvbS2GseDecapsulator = null;
                BufferBlock<PmFrameBase> decapsulatedFrames = null;
                if (_settings.DecapsulateGseOverUdp)
                {
                    l7DvbS2GseDecapsulator = new L7DvbS2GseDecapsulatorBlock();
                    decapsulatedFrames = new BufferBlock<PmFrameBase>();
                    l7ConversationBroadcaster.LinkTo(l7DvbS2GseDecapsulator,
                        new DataflowLinkOptions() {PropagateCompletion = true});
                    l7DvbS2GseDecapsulator.LinkTo(decapsulatedFrames,
                        new DataflowLinkOptions() {PropagateCompletion = true});
                }

                foreach (var frame in frames)
                {
                    frame.IndexFrame(null)
                        .Wait(); // NOTE: IndexFrame called with null is insecure. Parameter `IPmCaptureProcessorBlockBase captureProcessorBlockBase`
                    // is inside IndexFrame method currently used for parsing GRE and IP6in4. But here, after reassembling and decapsulation of GSE packets, we do not
                    // have capture processor available. In order to instantiate new one, we would need `FileInfo fileInfo` of capture file, which we do not have.
                    l3L4ConversationTracker.SendAsync(frame).Wait();
                }

                l3L4ConversationTracker.Complete();

                Task.WhenAll(
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

                if (!_settings.DecapsulateGseOverUdp) return;

                l7DvbS2GseDecapsulator?.Completion.Wait();
                decapsulatedFrames?.TryReceiveAll(out frames);
            }
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
            buffer.LinkTo(store, new DataflowLinkOptions() {PropagateCompletion = true});
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
                using (var dbx = this.NetfoxDBContextFactory.Create())
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
                SqlBulkCopyOptions =
                    SqlBulkCopyOptions.KeepIdentity //| SqlBulkCopyOptions.TableLock //| SqlBulkCopyOptions.KeepNulls
            };
        }
    }
}