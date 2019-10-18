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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Core.Collections;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.CaptureProcessor.L3L4ConversationTracking;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Persistence.JunctionTypes;

namespace Netfox.Framework.CaptureProcessor.L7Tracking
{
    internal class L7ConversationTrackerBlock :
        ITargetBlock<PmFrameBase>,
        ITargetBlock<L4ConversationExtended>,
        ISourceBlock<PmFrameBase>,
        ISourceBlock<L4Conversation>,
        ISourceBlock<PmCaptureL7Conversation>,
        ISourceBlock<L7Conversation>,
        ISourceBlock<L7ConversationStatistics>,
        ISourceBlock<L7PDU>
    {
        public IL7ConversationTrackerFactory L7ConversationTrackerFactory { get; }

        public L7ConversationTrackerBlock(IL7ConversationTrackerFactory l7ConversationTrackerFactory)
        {
            this.L7ConversationTrackerFactory = l7ConversationTrackerFactory;
            this.ProcessFrame = new ActionBlock<PmFrameBase>(async frame =>
            {
                var l4ConversationExt = frame.L4Conversation as L4ConversationExtended;
                if(l4ConversationExt != null)
                {
                    var l7Conversations = l4ConversationExt.L7ConversationTracker.ProcessPmFrame(frame);
                    await this.PostL7ConversationsIfAny(l7Conversations);
                }
                else
                {
                    await this.Frames.SendAsync(frame);
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });

            this.ProcessL4Conversation = new ActionBlock<L4ConversationExtended>(async l4Conversation =>
            {
                this.L4ConversationsForFinishL7Tracking.Add(l4Conversation);
                await this.L4Conversations.SendAsync(l4Conversation);
            });


            this.ProcessFrame.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    ((IDataflowBlock) this.Frames).Fault(t.Exception);
                    ((IDataflowBlock) this.L7Conversations).Fault(t.Exception);
                }
                else
                {
                    Parallel.ForEach(this.L4ConversationsForFinishL7Tracking, async l4Conversation =>
                    {
                        var l7Conversations = l4Conversation.L7ConversationTracker.Complete();
                        await this.PostL7ConversationsIfAny(l7Conversations);
                    });
                    this.Frames.Complete();
                    this.L7Conversations.Complete();
                    this.L7ConversationStatistics.Complete();
                    this.L7Pdus.Complete();
                    this.PmCaptureL7Conversations.Complete();
                }
            });

            this.ProcessL4Conversation.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) {
                    ((IDataflowBlock) this.L4Conversations).Fault(t.Exception);
                }
                else
                {
                    this.L4Conversations.Complete();
                }
            });


            this.Completion = Task.Run(async () =>
            {
                await this.Frames.Completion;
                await this.L7Conversations.Completion;
                await this.L7ConversationStatistics.Completion;
                await this.L7Pdus.Completion;
                await this.L4Conversations.Completion;
                await this.PmCaptureL7Conversations.Completion;
            });
        }

        private BufferBlock<PmFrameBase> Frames { get; }                                = new BufferBlock<PmFrameBase>();
        private BufferBlock<L4Conversation> L4Conversations { get; }                    = new BufferBlock<L4Conversation>();
        private BufferBlock<L7Conversation> L7Conversations { get; }                    = new BufferBlock<L7Conversation>();
        private BufferBlock<L7ConversationStatistics> L7ConversationStatistics { get; } = new BufferBlock<L7ConversationStatistics>();
        private BufferBlock<L7PDU> L7Pdus { get; }                                      = new BufferBlock<L7PDU>();
        private BufferBlock<PmCaptureL7Conversation> PmCaptureL7Conversations { get; }  = new BufferBlock<PmCaptureL7Conversation>();

        private ActionBlock<PmFrameBase> ProcessFrame { get; }
        private ActionBlock<L4ConversationExtended> ProcessL4Conversation { get; }

        private ConcurrentObservableHashSet<L4ConversationExtended> L4ConversationsForFinishL7Tracking { get; } = new ConcurrentObservableHashSet<L4ConversationExtended>();

        #region Implementation of IDataflowBlock
        public void Complete()
        {
            this.ProcessFrame.Complete();
            this.ProcessL4Conversation.Complete();
        }

        public void Fault(Exception exception)
        {
            ((ITargetBlock<PmFrameBase>)this.ProcessFrame).Fault(exception);
            ((ITargetBlock<L4ConversationExtended>)this.ProcessL4Conversation).Fault(exception);
        }

        public Task Completion { get; }
        #endregion

        #region Implementation of ISourceBlock<L4Conversation> proxy methods
        /// <summary>
        /// Proxy to link <see cref="L4Conversations"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<L4Conversation> target, DataflowLinkOptions linkOptions)
        {
            return this.L4Conversations.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume from <see cref="L4Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public L4Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4Conversation> target, out bool messageConsumed)
        {
            return ((ISourceBlock<L4Conversation>)this.L4Conversations).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve message from <see cref="L4Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4Conversation> target)
        {
            return ((ISourceBlock<L4Conversation>)this.L4Conversations).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation to <see cref="L4Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L4Conversation> target)
        {
            ((ISourceBlock<L4Conversation>)this.L4Conversations).ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<L7Conversation> proxy methods
        /// <summary>
        /// Proxy to link <see cref="L7Conversations"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<L7Conversation> target, DataflowLinkOptions linkOptions)
        {
            return this.L7Conversations.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume from <see cref="L7Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public L7Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7Conversation> target, out bool messageConsumed)
        {
            return ((ISourceBlock<L7Conversation>)this.L7Conversations).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve message from <see cref="L7Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7Conversation> target)
        {
            return ((ISourceBlock<L7Conversation>)this.L7Conversations).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation to <see cref="L7Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L7Conversation> target)
        {
            ((ISourceBlock<L7Conversation>)this.L7Conversations).ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<L7ConversationStatistics> proxy methods
        /// <summary>
        /// Proxy to link <see cref="L7ConversationStatistics"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<L7ConversationStatistics> target, DataflowLinkOptions linkOptions)
        {
            return this.L7ConversationStatistics.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume from <see cref="L7ConversationStatistics"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public L7ConversationStatistics ConsumeMessage(
            DataflowMessageHeader messageHeader,
            ITargetBlock<L7ConversationStatistics> target,
            out bool messageConsumed)
        {
            return ((ISourceBlock<L7ConversationStatistics>)this.L7ConversationStatistics).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve message from <see cref="L7ConversationStatistics"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7ConversationStatistics> target)
        {
            return ((ISourceBlock<L7ConversationStatistics>)this.L7ConversationStatistics).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation to <see cref="L7ConversationStatistics"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L7ConversationStatistics> target)
        {
            ((ISourceBlock<L7ConversationStatistics>)this.L7ConversationStatistics).ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<L7PDU> proxy methods
        /// <summary>
        /// Proxy to link <see cref="L7Pdus"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<L7PDU> target, DataflowLinkOptions linkOptions)
        {
            return this.L7Pdus.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume from <see cref="L7Pdus"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public L7PDU ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7PDU> target, out bool messageConsumed)
        {
            return ((ISourceBlock<L7PDU>)this.L7Pdus).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve message from <see cref="L7Pdus"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7PDU> target)
        {
            return ((ISourceBlock<L7PDU>)this.L7Pdus).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation to <see cref="L7Pdus"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L7PDU> target)
        {
            ((ISourceBlock<L7PDU>)this.L7Pdus).ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<PmFrameBase> proxy methods
        /// <summary>
        /// Proxy to link <see cref="Frames"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<PmFrameBase> target, DataflowLinkOptions linkOptions)
        {
            return this.Frames.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume from <see cref="Frames"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public PmFrameBase ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target, out bool messageConsumed)
        {
            return ((ISourceBlock<PmFrameBase>)this.Frames).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve message from <see cref="Frames"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target)
        {
            return ((ISourceBlock<PmFrameBase>)this.Frames).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation to <see cref="Frames"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target)
        {
            ((ISourceBlock<PmFrameBase>)this.Frames).ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<PmCaptureL7Conversation> proxy methods
        /// <summary>
        /// Proxy to link <see cref="PmCaptureL7Conversations"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<PmCaptureL7Conversation> target, DataflowLinkOptions linkOptions)
        {
            return this.PmCaptureL7Conversations.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume from <see cref="PmCaptureL7Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public PmCaptureL7Conversation ConsumeMessage(
            DataflowMessageHeader messageHeader,
            ITargetBlock<PmCaptureL7Conversation> target,
            out bool messageConsumed)
        {
            return ((ISourceBlock<PmCaptureL7Conversation>)this.PmCaptureL7Conversations).ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve message from <see cref="PmCaptureL7Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL7Conversation> target)
        {
            return ((ISourceBlock<PmCaptureL7Conversation>)this.PmCaptureL7Conversations).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation to <see cref="PmCaptureL7Conversations"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL7Conversation> target)
        {
            ((ISourceBlock<PmCaptureL7Conversation>)this.PmCaptureL7Conversations).ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ITargetBlock<PmFrameBase> proxy methods
        /// <summary>
        /// Proxy to offer message to <see cref="ProcessFrame"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(
            DataflowMessageHeader messageHeader,
            PmFrameBase messageValue,
            ISourceBlock<PmFrameBase> source,
            bool consumeToAccept)
        {
            return ((ITargetBlock<PmFrameBase>)this.ProcessFrame)
                .OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
        #endregion

        #region Implementation of ITargetBlock<L4ConversationExtended> proxy methods
        /// <summary>
        /// Proxy to offer message to <see cref="ProcessL4Conversation"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(
            DataflowMessageHeader messageHeader,
            L4ConversationExtended messageValue,
            ISourceBlock<L4ConversationExtended> source,
            bool consumeToAccept)
        {
            return ((ITargetBlock<L4ConversationExtended>)this.ProcessL4Conversation)
                .OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
        #endregion

        private async Task<bool> PostL7ConversationsIfAny(IEnumerable<L7Conversation> l7Conversations)
        {
            if(l7Conversations == null) return true;
            foreach(var l7Conversation in l7Conversations)
            {
                await this.L7Conversations.SendAsync(l7Conversation);
                foreach (var l7ConversationCapture in l7Conversation.Captures)
                {
                    l7ConversationCapture.L7Conversations.Add(l7Conversation);
                    await this.PmCaptureL7Conversations.SendAsync(new PmCaptureL7Conversation(l7ConversationCapture.Id,l7Conversation.Id));
                }
                await this.L7ConversationStatistics.SendAsync(l7Conversation.UpConversationStatistic);
                await this.L7ConversationStatistics.SendAsync(l7Conversation.DownConversationStatistic);

                foreach(var l7PDU in l7Conversation.L7PDUs) { await this.L7Pdus.SendAsync(l7PDU); }

                foreach(var l7ConversationFrame in l7Conversation.Frames) { await this.Frames.SendAsync(l7ConversationFrame); }
            }
            return false;
        }
    }
}