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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.NBARDatabase;
using Netfox.Persistence.JunctionTypes;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.L3L4ConversationTracking
{
    internal class L3L4ConversationTrackerBlock : ITargetBlock<PmFrameBase>, ISourceBlock<L3Conversation>, ISourceBlock<PmCaptureL3Conversation>,ISourceBlock<PmCaptureL4Conversation>, ISourceBlock<L4ConversationExtended>, ISourceBlock<PmFrameBase>, ISourceBlock<L3ConversationStatistics>, ISourceBlock<L4ConversationStatistics>
    {
        public IL3ConversationFactory L3ConversationFactory { get; }
        public IL4ConversationFactory L4ConversationFactory { get; }
        public NBARProtocolPortDatabase NBARProtocolPortDatabase { get; }
        private readonly ConcurrentDictionary<L3ConversationExtended, L3ConversationExtended> _l3FlowsTable = new ConcurrentDictionary<L3ConversationExtended, L3ConversationExtended>();
        private readonly ConcurrentDictionary<L4FlowKey, L4ConversationExtended> _l4ConversationTable = new ConcurrentDictionary<L4FlowKey, L4ConversationExtended>();

        public L3L4ConversationTrackerBlock(IL3ConversationFactory l3ConversationFactory, IL4ConversationFactory l4ConversationFactory, NBARProtocolPortDatabase nbarProtocolPortDatabase)
        {
            this.L3ConversationFactory = l3ConversationFactory;
            this.L4ConversationFactory = l4ConversationFactory;
            this.NBARProtocolPortDatabase = nbarProtocolPortDatabase;

            this.NullL3Conversation = this.L3ConversationFactory.Create(new IPAddress(0), new IPAddress(0));
            this.ProcessPacketActionBlock = new ActionBlock<PmFrameBase>(async parsedFrame =>
            {
                await this.ProcessPacket(parsedFrame);
                await this.ProcessedFrames.SendAsync(parsedFrame);
            });

            this.ProcessPacketActionBlock.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    ((IDataflowBlock) this.L3Conversations).Fault(t.Exception);
                    ((IDataflowBlock) this.L4Conversations).Fault(t.Exception);
                    ((IDataflowBlock) this.ProcessedFrames).Fault(t.Exception);
                }
                else
                {
                    if(this.NullL3Conversation.Frames.Any())
                    {
                        this.L3Conversations.SendAsync(this.NullL3Conversation).Wait();
                        this.L3ConversationStatistics.SendAsync(this.NullL3Conversation.UpConversationStatistic).Wait();
                        this.L3ConversationStatistics.SendAsync(this.NullL3Conversation.DownConversationStatistic).Wait();
                    }
                    this.L3Conversations.Complete();
                    this.PmCaptureL3Conversations.Complete();
                    this.L3ConversationStatistics.Complete();
                    this.L4Conversations.Complete();
                    this.PmCaptureL4Conversations.Complete();
                    this.L4ConversationStatistics.Complete();
                    this.ProcessedFrames.Complete();
                }
            });

            this.Completion = Task.Run(async () =>
            {
                await this.L3Conversations.Completion;
                await this.PmCaptureL3Conversations.Completion;
                await this.L3ConversationStatistics.Completion;
                await this.L4Conversations.Completion;
                await this.PmCaptureL4Conversations.Completion;
                await this.L4ConversationStatistics.Completion;
                await this.ProcessedFrames.Completion;
            });
        }

        private BufferBlock<PmFrameBase> ProcessedFrames { get; } = new BufferBlock<PmFrameBase>();
        private ISourceBlock<PmFrameBase> _ProcessedFrames => this.ProcessedFrames;
        private BufferBlock<L3Conversation> L3Conversations { get; } = new BufferBlock<L3Conversation>();
        private ISourceBlock<L3Conversation> _L3Conversations => this.L3Conversations;
        private BufferBlock<PmCaptureL3Conversation> PmCaptureL3Conversations { get; } = new BufferBlock<PmCaptureL3Conversation>();
        private ISourceBlock<PmCaptureL3Conversation> _PmCaptureL3Conversations => this.PmCaptureL3Conversations;

        private BufferBlock<L3ConversationStatistics> L3ConversationStatistics { get; } = new BufferBlock<L3ConversationStatistics>();
        private ISourceBlock<L3ConversationStatistics> _L3ConversationStatistics => this.L3ConversationStatistics;
        private BufferBlock<L4ConversationExtended> L4Conversations { get; } = new BufferBlock<L4ConversationExtended>();
        private ISourceBlock<L4ConversationExtended> _L4Conversations => this.L4Conversations;
        private BufferBlock<PmCaptureL4Conversation> PmCaptureL4Conversations { get; } = new BufferBlock<PmCaptureL4Conversation>();
        private ISourceBlock<PmCaptureL4Conversation> _PmCaptureL4Conversations => this.PmCaptureL4Conversations;

        private BufferBlock<L4ConversationStatistics> L4ConversationStatistics { get; } = new BufferBlock<L4ConversationStatistics>();
        private ISourceBlock<L4ConversationStatistics> _L4ConversationStatistics => this.L4ConversationStatistics;
        private ActionBlock<PmFrameBase> ProcessPacketActionBlock { get; }
        private ITargetBlock<PmFrameBase> _processPacketActionBlock => this.ProcessPacketActionBlock;

        private L3ConversationExtended NullL3Conversation { get; } 
        

        private L4ConversationExtended CreateL4Conversation(PmFrameBase baseConversationFrame, L3Conversation l3Conversation, long l4FlowMTU)
        {
            IPEndPoint sourceEndPoint, destinationEndPoint;
            if(this.NBARProtocolPortDatabase.IsTCPServerPort(baseConversationFrame.DstPort))
            {
                sourceEndPoint = baseConversationFrame.SourceEndPoint;
                destinationEndPoint = baseConversationFrame.DestinationEndPoint;
            }
            else if(this.NBARProtocolPortDatabase.IsTCPServerPort(baseConversationFrame.SrcPort))
            {
                sourceEndPoint = baseConversationFrame.DestinationEndPoint;
                destinationEndPoint = baseConversationFrame.SourceEndPoint;
            }
            else if(baseConversationFrame.SrcPort > baseConversationFrame.DstPort)
            {
                sourceEndPoint = baseConversationFrame.SourceEndPoint;
                destinationEndPoint = baseConversationFrame.DestinationEndPoint;
            }
            else
            {
                sourceEndPoint = baseConversationFrame.DestinationEndPoint;
                destinationEndPoint = baseConversationFrame.SourceEndPoint;
            }
            return this.L4ConversationFactory.Create(baseConversationFrame.IpProtocol, l3Conversation, sourceEndPoint, destinationEndPoint, l4FlowMTU);
        }

        private L3ConversationExtended L3Provider(PmFrameBase packet)
        {
            switch(packet.IpProtocol)
            {
                case IPProtocolType.IP:
                case IPProtocolType.IPV6:
                case IPProtocolType.TCP:
                case IPProtocolType.UDP:
                    return this.L3ConversationFactory.Create(packet.SrcAddress, packet.DstAddress);
                default:
                    return this.L3ConversationFactory.Create(PmFrameBase.NullEndPoint.Address, PmFrameBase.NullEndPoint.Address);
            }
        }

        private L4FlowKey L4Provider(PmFrameBase packet)
        {
            L4FlowKey l4FlowKey;
            if(packet.IpProtocol == IPProtocolType.IP || packet.IpProtocol == IPProtocolType.IPV6 || packet.IpProtocol == IPProtocolType.TCP
               || packet.IpProtocol == IPProtocolType.UDP)
            {
                l4FlowKey = new L4FlowKey
                {
                    L4ProtocolType = packet.IpProtocol,
                    EndPoints =
                    {
                        [0] = packet.SourceEndPoint,
                        [1] = packet.DestinationEndPoint
                    }
                };
                return l4FlowKey;
            }

            l4FlowKey = new L4FlowKey
            {
                L4ProtocolType = IPProtocolType.NONE
            };
            l4FlowKey.EndPoints[0] = l4FlowKey.EndPoints[1] = PmFrameBase.NullEndPoint;
            return l4FlowKey;
        }

        private async Task ProcessPacket(PmFrameBase packet)
        {
            if(
                !(packet.IpProtocol == IPProtocolType.TCP || packet.IpProtocol == IPProtocolType.UDP || packet.IpProtocol == IPProtocolType.IP
                  || packet.IpProtocol == IPProtocolType.IPV6)) //skip non IP frames
            {
                await this.L3Updater(this.NullL3Conversation,packet);
                return;
            }
            var l4FlowKey = this.L4Provider(packet);
            L4ConversationExtended l4Conversation;
            L3ConversationExtended l3Conversation;
            if(!this._l4ConversationTable.TryGetValue(l4FlowKey, out l4Conversation))
            {
                var l3FlowKey = this.L3Provider(packet);

                if(!this._l3FlowsTable.TryGetValue(l3FlowKey, out l3Conversation))
                {
                    l3Conversation = this.L3ConversationFactory.Create(l3FlowKey);
                    var addl3Conversation = true;
                    this._l3FlowsTable.AddOrUpdate(l3Conversation, l3Conversation, (key, inDictinaryl3Conversation) =>
                    {
                        l3Conversation = inDictinaryl3Conversation;
                        addl3Conversation = false;
                        return inDictinaryl3Conversation;
                    });
                    if(addl3Conversation)
                    {
                        await this.L3Conversations.SendAsync(l3Conversation);
                        await this.L3ConversationStatistics.SendAsync(l3Conversation.UpConversationStatistic);
                        await this.L3ConversationStatistics.SendAsync(l3Conversation.DownConversationStatistic);
                    }
                }

                var addl4Conversation = true;
                if(packet.IsL4Frame) // packet with no L4 header
                {
                l4Conversation = this.CreateL4Conversation(packet, l3Conversation, packet.L7PayloadLength);
                this._l4ConversationTable.AddOrUpdate(l4FlowKey, l4Conversation, (key, inDictinaryl4Conversation) =>
                {
                    l4Conversation = inDictinaryl4Conversation;
                    addl4Conversation = false;
                    return inDictinaryl4Conversation;
                });
                if(addl4Conversation)
                {
                    await this.L4Conversations.SendAsync(l4Conversation);
                    await this.L4ConversationStatistics.SendAsync(l4Conversation.UpConversationStatistic);
                    await this.L4ConversationStatistics.SendAsync(l4Conversation.DownConversationStatistic);
                }
                }
            }
            else
            { l3Conversation = l4Conversation.L3Conversation as L3ConversationExtended; }
            await this.L3Updater(l3Conversation, packet);
            if (l4Conversation != null)
                await this.L4Updater(l4Conversation, packet);
        }

        private async Task L3Updater(L3ConversationExtended l3Conversation, PmFrameBase frame)
        {
            l3Conversation.ConversationStats.ProcessFrame(frame);
            l3Conversation.Frames.Add(frame);
            if (!frame.IsL4Frame) l3Conversation.NonL4Frames.Add(frame);
            frame.L3Conversation = l3Conversation;
            if(l3Conversation.CapturesHashSet.Add(frame.PmCapture))
            {
                l3Conversation.Captures.Add(frame.PmCapture);
                frame.PmCapture.L3Conversations.Add(l3Conversation);
                await this.PmCaptureL3Conversations.SendAsync(new PmCaptureL3Conversation(frame.PmCapture.Id, l3Conversation.Id));
            }
        }

        private async Task L4Updater(L4ConversationExtended l4Conversation, PmFrameBase frame)
        {
            try {
                l4Conversation.ConversationStats.ProcessFrame(frame);
                l4Conversation.Frames.Add(frame);
                frame.L4Conversation = l4Conversation;
                if (l4Conversation.CapturesHashSet.Add(frame.PmCapture))
                {
                    l4Conversation.Captures.Add(frame.PmCapture);
                    frame.PmCapture.L4Conversations.Add(l4Conversation);
                    await this.PmCaptureL4Conversations.SendAsync(new PmCaptureL4Conversation(frame.PmCapture.Id, l4Conversation.Id));
                }
            }
            catch(Exception e)
            {
                Debugger.Break();
                Console.WriteLine(e);
                throw;
            }
            
        }

        #region Implementation of IDataflowBlock
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, PmFrameBase messageValue, ISourceBlock<PmFrameBase> source, bool consumeToAccept)
        {
            return this._processPacketActionBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete() { this.ProcessPacketActionBlock.Complete(); }
        public void Fault(Exception exception) { ((IDataflowBlock) this.ProcessPacketActionBlock).Fault(exception); }
        public Task Completion { get; }
        #endregion

        #region Implementation of ISourceBlock<out L3Conversation>
        public IDisposable LinkTo(ITargetBlock<L3Conversation> target, DataflowLinkOptions linkOptions) { return this._L3Conversations.LinkTo(target, linkOptions); }

        public L3Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L3Conversation> target, out bool messageConsumed)
        {
            return this._L3Conversations.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L3Conversation> target) { return this._L3Conversations.ReserveMessage(messageHeader, target); }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L3Conversation> target)
        {
            this._L3Conversations.ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<out L4ConversationExtended>
        public IDisposable LinkTo(ITargetBlock<L4ConversationExtended> target, DataflowLinkOptions linkOptions) { return this._L4Conversations.LinkTo(target, linkOptions); }

        public L4ConversationExtended ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4ConversationExtended> target, out bool messageConsumed)
        {
            return this._L4Conversations.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4ConversationExtended> target)
        {
            return this._L4Conversations.ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L4ConversationExtended> target)
        {
            this._L4Conversations.ReleaseReservation(messageHeader, target);
        }
        #endregion

        #region Implementation of ISourceBlock<out PmFrameBase>
        public IDisposable LinkTo(ITargetBlock<PmFrameBase> target, DataflowLinkOptions linkOptions) { return this._ProcessedFrames.LinkTo(target, linkOptions); }

        public PmFrameBase ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target, out bool messageConsumed)
        {
            return this._ProcessedFrames.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target) { return this._ProcessedFrames.ReserveMessage(messageHeader, target); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target) { this._ProcessedFrames.ReleaseReservation(messageHeader, target); }
        #endregion

        #region Implementation of ISourceBlock<out L3ConversationStatistics>
        public IDisposable LinkTo(ITargetBlock<L3ConversationStatistics> target, DataflowLinkOptions linkOptions) { return this._L3ConversationStatistics.LinkTo(target, linkOptions); }
        public L3ConversationStatistics ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L3ConversationStatistics> target, out bool messageConsumed) { return this._L3ConversationStatistics.ConsumeMessage(messageHeader, target, out messageConsumed); }
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L3ConversationStatistics> target) { return this._L3ConversationStatistics.ReserveMessage(messageHeader, target); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L3ConversationStatistics> target) { this._L3ConversationStatistics.ReleaseReservation(messageHeader, target); }
        #endregion

        #region Implementation of ISourceBlock<out L4ConversationStatistics>
        public IDisposable LinkTo(ITargetBlock<L4ConversationStatistics> target, DataflowLinkOptions linkOptions) { return this._L4ConversationStatistics.LinkTo(target, linkOptions); }
        public L4ConversationStatistics ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4ConversationStatistics> target, out bool messageConsumed) { return this._L4ConversationStatistics.ConsumeMessage(messageHeader, target, out messageConsumed); }
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<L4ConversationStatistics> target) { return this._L4ConversationStatistics.ReserveMessage(messageHeader, target); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<L4ConversationStatistics> target) { this._L4ConversationStatistics.ReleaseReservation(messageHeader, target); }
        #endregion

        #region Implementation of ISourceBlock<out PmCaptureL3Conversation>
        public IDisposable LinkTo(ITargetBlock<PmCaptureL3Conversation> target, DataflowLinkOptions linkOptions) { return this._PmCaptureL3Conversations.LinkTo(target, linkOptions); }
        public PmCaptureL3Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL3Conversation> target, out bool messageConsumed) { return this._PmCaptureL3Conversations.ConsumeMessage(messageHeader, target, out messageConsumed); }
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL3Conversation> target) { return this._PmCaptureL3Conversations.ReserveMessage(messageHeader, target); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL3Conversation> target) { this._PmCaptureL3Conversations.ReleaseReservation(messageHeader, target); }
        #endregion

        #region Implementation of ISourceBlock<out PmCaptureL4Conversation>
        public IDisposable LinkTo(ITargetBlock<PmCaptureL4Conversation> target, DataflowLinkOptions linkOptions) { return this._PmCaptureL4Conversations.LinkTo(target, linkOptions); }
        public PmCaptureL4Conversation ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL4Conversation> target, out bool messageConsumed) { return this._PmCaptureL4Conversations.ConsumeMessage(messageHeader, target, out messageConsumed); }
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL4Conversation> target) { return this._PmCaptureL4Conversations.ReserveMessage(messageHeader, target); }
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureL4Conversation> target) { this._PmCaptureL4Conversations.ReleaseReservation(messageHeader, target); }
        #endregion
    }
}