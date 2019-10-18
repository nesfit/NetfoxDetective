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

namespace Netfox.Framework.CaptureProcessor.L7Tracking
{
    public class L7ConversationTrackerBlock : ITargetBlock<PmFrameBase>, ITargetBlock<L4ConversationExtended>, ISourceBlock<L7Conversation>, ISourceBlock<PmFrameBase>,
        ISourceBlock<L4Conversation>, ISourceBlock<L7PDU>, ISourceBlock<L7ConversationStatistics>
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
            });
        }

        private BufferBlock<PmFrameBase> Frames { get; } = new BufferBlock<PmFrameBase>();
        private ISourceBlock<PmFrameBase> _Frames => this.Frames;

        private BufferBlock<L4Conversation> L4Conversations { get; } = new BufferBlock<L4Conversation>();
        private ISourceBlock<L4Conversation> _L4Conversations => this.L4Conversations;

        private BufferBlock<L7Conversation> L7Conversations { get; } = new BufferBlock<L7Conversation>();
        private ISourceBlock<L7Conversation> _L7Conversations => this.L7Conversations;

        private BufferBlock<L7ConversationStatistics> L7ConversationStatistics { get; } = new BufferBlock<L7ConversationStatistics>();
        private ISourceBlock<L7ConversationStatistics> _L7ConversationStatistics => this.L7ConversationStatistics;

        private BufferBlock<L7PDU> L7Pdus { get; } = new BufferBlock<L7PDU>();
        private ISourceBlock<L7PDU> _L7Pdus => this.L7Pdus;

        private ActionBlock<PmFrameBase> ProcessFrame { get; }
        private ITargetBlock<PmFrameBase> _ProcessFrame => this.ProcessFrame;
        private ActionBlock<L4ConversationExtended> ProcessL4Conversation { get; }
        private ITargetBlock<L4ConversationExtended> _ProcessL4Conversation => this.ProcessL4Conversation;

        private ConcurrentObservableHashSet<L4ConversationExtended> L4ConversationsForFinishL7Tracking { get; } = new ConcurrentObservableHashSet<L4ConversationExtended>();

        public void Complete()
        {
            this.ProcessFrame.Complete();
            this.ProcessL4Conversation.Complete();
        }

        public Task Completion { get; }

        public void Fault(Exception exception) { this._ProcessFrame.Fault(exception); }

        public L4Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4Conversation> target, out bool messageConsumed)
        {
            return this._L4Conversations.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<L4Conversation> target, DataflowLinkOptions linkOptions) { return this._L4Conversations.LinkTo(target, linkOptions); }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L4Conversation> target)
        {
            this._L4Conversations.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4Conversation> target) { return this._L4Conversations.ReserveMessage(messageHeader, target); }

        public L7Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7Conversation> target, out bool messageConsumed)
        {
            return this._L7Conversations.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<L7Conversation> target, DataflowLinkOptions linkOptions) { return this._L7Conversations.LinkTo(target, linkOptions); }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L7Conversation> target)
        {
            this._L7Conversations.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7Conversation> target) { return this._L7Conversations.ReserveMessage(messageHeader, target); }

        public L7ConversationStatistics ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7ConversationStatistics> target, out bool messageConsumed)
        {
            return this._L7ConversationStatistics.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<L7ConversationStatistics> target, DataflowLinkOptions linkOptions)
        {
            return this._L7ConversationStatistics.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L7ConversationStatistics> target)
        {
            this._L7ConversationStatistics.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7ConversationStatistics> target)
        {
            return this._L7ConversationStatistics.ReserveMessage(messageHeader, target);
        }

        public L7PDU ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7PDU> target, out bool messageConsumed)
        {
            return this._L7Pdus.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<L7PDU> target, DataflowLinkOptions linkOptions) { return this._L7Pdus.LinkTo(target, linkOptions); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L7PDU> target) { this._L7Pdus.ReleaseReservation(messageHeader, target); }
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L7PDU> target) { return this._L7Pdus.ReserveMessage(messageHeader, target); }

        public PmFrameBase ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target, out bool messageConsumed)
        {
            return this._Frames.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<PmFrameBase> target, DataflowLinkOptions linkOptions) { return this._Frames.LinkTo(target, linkOptions); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target) { this._Frames.ReleaseReservation(messageHeader, target); }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target) { return this._Frames.ReserveMessage(messageHeader, target); }

        public DataflowMessageStatus OfferMessage(
            DataflowMessageHeader messageHeader,
            L4ConversationExtended messageValue,
            ISourceBlock<L4ConversationExtended> source,
            bool consumeToAccept)
        {
            return this._ProcessL4Conversation.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, PmFrameBase messageValue, ISourceBlock<PmFrameBase> source, bool consumeToAccept)
        {
            return this._ProcessFrame.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }
        
        private async Task<bool> PostL7ConversationsIfAny(IEnumerable<L7Conversation> l7Conversations)
        {
            if(l7Conversations == null) return true;
            foreach(var l7Conversation in l7Conversations)
            {
                await this.L7Conversations.SendAsync(l7Conversation);
                foreach(var l7ConversationCapture in l7Conversation.Captures)
                {
                    l7ConversationCapture.L7Conversations.Add(l7Conversation);
                }
                await this.L7ConversationStatistics.SendAsync(l7Conversation.UpConversationStatistic);
                await this.L7ConversationStatistics.SendAsync(l7Conversation.DownConversationStatistic);

                foreach(var l7PDU in l7Conversation.L7PDUs) { await this.L7Pdus.SendAsync(l7PDU); }

                var framConvDict = new Dictionary<Guid, PmFrameBase>();
                foreach(var l7ConversationFrame in l7Conversation.Frames) { await this.Frames.SendAsync(l7ConversationFrame); }
            }
            return false;
        }
    }
}