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
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Framework.Models.Interfaces;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;
using PacketDotNet.Utils;

namespace Netfox.Framework.CaptureProcessor.Captures
{
    public abstract class PmCaptureProcessorBlockBase : IPmCaptureProcessorBlockBase, ISourceBlock<PmFrameBase>
    {
        protected PmCaptureProcessorBlockBase(PmCaptureBase pmCapture)
        {
            this.PmCapture = pmCapture;
            this.IndexMetaFramesBlock = this.CreateIndexMetaFramesBlock();
            this.PmMetaFramesBufferBlock.LinkTo(this.IndexMetaFramesBlock);
            this.IndexMetaFramesBlock.LinkTo(this.PmParsedFramesBufferBlock);
            this.PmMetaFramesBufferBlock.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock) this.IndexMetaFramesBlock).Fault(t.Exception);
                else this.IndexMetaFramesBlock.Complete();
            });
            this.IndexMetaFramesBlock.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted) ((IDataflowBlock) this.PmParsedFramesBufferBlock).Fault(t.Exception);
                else this.PmParsedFramesBufferBlock.Complete();
            });
        }

        public TransformBlock<PmFrameBase, PmFrameBase> IndexMetaFramesBlock { get; }
        protected BufferBlock<PmFrameBase> PmMetaFramesBufferBlock { get; } = new BufferBlock<PmFrameBase>();
        protected BufferBlock<PmFrameBase> PmParsedFramesBufferBlock { get; } = new BufferBlock<PmFrameBase>();

        protected PmCaptureBase PmCapture { get; }

        /// <summary>
        ///     Used for capture read, only meta information, not acctual payload
        /// </summary>
        protected BinaryReader BinReader { get; set; }

        private ISourceBlock<PmFrameBase> _pmMetaFramesBufferBlock => this.PmMetaFramesBufferBlock;
        private ISourceBlock<PmFrameBase> _pmParsedFramesBufferBlock => this.PmParsedFramesBufferBlock;

        public async Task<PmCaptureBase> ProcessCapture()
        {
            await Task.WhenAll(Task.Run(async () => await this.ParseAndReateMetaFrames()), Task.Run(() => this.ComputeHash()));
            this.PmMetaFramesBufferBlock.Complete();
            await this.Completion;
            return this.PmCapture;
        }

        private void ComputeHash() { this.PmCapture.PcapHashOriginal = this.PmCapture.ComputePcapHash(); }

        #region RealFrames Manipulation
        /// <summary>
        /// BEAWARE Runns in parallel
        /// </summary>
        /// <param name="pmFrame"></param>
        /// <param name="parentPacket"></param>
        /// <returns></returns>
        public async Task CreateAndAddToMetaFramesVirtualFrame(PmFrameBase pmFrame, PmPacket parentPacket)
        {
            PmFrameVirtual virtualFrame = null;

            switch(pmFrame.IpProtocol)
            {
                case IPProtocolType.GRE:
                    var gre = new GrePacket(new ByteArraySegment(pmFrame.L4Data()));
                    virtualFrame = new PmFrameVirtual(pmFrame.PmCapture, pmFrame.FrameIndex, pmFrame.TimeStamp, pmFrame.IncludedLength, (pmFrame.L4Offset + gre.Header.Length));
                    break;

                case IPProtocolType.IPV6:
                    virtualFrame = new PmFrameVirtual(pmFrame.PmCapture, pmFrame.FrameIndex, pmFrame.TimeStamp, pmFrame.IncludedLength,  (pmFrame.L4Offset));
                    break;

                case IPProtocolType.UDP:
                    var udp = new UdpPacket(new ByteArraySegment(pmFrame.L4Data()));
                    //Port 3544 is used to communicate with Teredo Server. We need communication between host and Teredo Relay (temporarily solution)
                    if(udp.DestinationPort == 3544 || udp.SourcePort == 3544)
                    {
                        break;
                    }

                    virtualFrame = new PmFrameVirtual(pmFrame.PmCapture, pmFrame.FrameIndex, pmFrame.TimeStamp, pmFrame.IncludedLength,
                         (pmFrame.L4Offset + udp.Header.Length));
                    break;
            }
            if(virtualFrame != null) { await this.PmMetaFramesBufferBlock.SendAsync(virtualFrame); }
        }

        private TransformBlock<PmFrameBase, PmFrameBase> CreateIndexMetaFramesBlock()
        {
            return new TransformBlock<PmFrameBase, PmFrameBase>(async metaframe =>
            {
                await metaframe.IndexFrame(this);
                return metaframe;
            }
            , new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }
            );
        }

        private async Task ParseAndReateMetaFrames()
        {
            try
            {
                this.BinReader = this.PmCapture.BinaryReadersPool.GetReader();
                await this.ParseCaptureFile();
            }
            finally
            {
                this.PmCapture.BinaryReadersPool.PutReader(this.BinReader);
                this.BinReader = null;
            }
        }
        #endregion

        #region Implementation of IPmCaptureProcessorBase
        public abstract Boolean WriteCaptureFile(String outputFile);
        public abstract DateTime? GetFirstTimeStamp();
        protected abstract Task<Boolean> ParseCaptureFile();
        #endregion

        #region Implementation of IDataflowBlock
        public void Complete() { throw new NotImplementedException(); }
        public void Fault(Exception exception) { this._pmMetaFramesBufferBlock.Fault(exception); }
        public Task Completion => this._pmParsedFramesBufferBlock.Completion;
        #endregion

        #region Implementation of ISourceBlock<out PmFrameBase>
        public IDisposable LinkTo(ITargetBlock<PmFrameBase> target, DataflowLinkOptions linkOptions) { return this._pmParsedFramesBufferBlock.LinkTo(target, linkOptions); }

        public PmFrameBase ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target, out bool messageConsumed)
        {
            return this._pmParsedFramesBufferBlock.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target)
        {
            return this._pmParsedFramesBufferBlock.ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target)
        {
            this._pmParsedFramesBufferBlock.ReleaseReservation(messageHeader, target);
        }
        #endregion
    }
}