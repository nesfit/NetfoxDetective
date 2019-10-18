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
using Netfox.Framework.CaptureProcessor.CoreController;
using Netfox.Framework.CaptureProcessor.Interfaces;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.CaptureProcessor.Captures
{
    /// <remarks>
    ///     Base class for all supported file types
    /// </remarks>
    public class CaptureProcessorBlock : ISourceBlock<PmFrameBase>, ITargetBlock<FileInfo>, ISourceBlock<PmCaptureBase>
    {
        public IPmCaptureProcessorBlockFactory PmCaptureProcessorBlockFactory { get; }

        public CaptureProcessorBlock(IPmCaptureProcessorBlockFactory pmCaptureProcessorBlockFactory)
        {
            this.PmCaptureProcessorBlockFactory = pmCaptureProcessorBlockFactory;
            this.ProcessCaptureFileActionBlock = this.CreateProcessCaptureFileActionBlock();
            this.ProcessCaptureFileActionBlock.Completion.ContinueWith(t =>
            {
                if(t.IsFaulted)
                {
                    ((IDataflowBlock) this.PmCapturesBufferBlock).Fault(t.Exception);
                    ((IDataflowBlock) this.PmParsedFramesBufferBlock).Fault(t.Exception);
                }
                else
                {
                    this.PmCapturesBufferBlock.Complete();
                    this.PmParsedFramesBufferBlock.Complete();
                }
            });

            this.Completion = Task.Run(async () =>
            {
                await this.PmCapturesBufferBlock.Completion;
                await this.PmParsedFramesBufferBlock.Completion;
            });
        }
        
        protected BufferBlock<PmCaptureBase> PmCapturesBufferBlock { get; } = new BufferBlock<PmCaptureBase>();
        protected BufferBlock<PmFrameBase> PmParsedFramesBufferBlock { get; } = new BufferBlock<PmFrameBase>();
        private ISourceBlock<PmCaptureBase> _pmCapturesBufferBlock => this.PmCapturesBufferBlock;
        private ITargetBlock<FileInfo> _ProcessCaptureFileActionBlock => this.ProcessCaptureFileActionBlock;
        private ISourceBlock<PmFrameBase> _pmParsedFramesBufferBlock => this.PmParsedFramesBufferBlock;

        private ActionBlock<FileInfo> ProcessCaptureFileActionBlock { get; }

        private ActionBlock<FileInfo> CreateProcessCaptureFileActionBlock()
        {
            return new ActionBlock<FileInfo>(async captureFileInfo =>
            {
                var pmCapture = this.ProcessCaptureFile(captureFileInfo);
                await this.PmCapturesBufferBlock.SendAsync(pmCapture);
            });
        }

        private PmCaptureFileType DetermineType(BinaryReader fileReader)
        {
            if(fileReader == null) { throw new ArgumentException("Parameter cannot be null", nameof(fileReader)); }

            fileReader.BaseStream.Seek(0, SeekOrigin.Begin);

            var signature = fileReader.ReadUInt32();
            switch(signature)
            {
                /* Begins with GMBU which is significant for MNM */
                case 0x55424d47:
                case 0x474d4255:
                    return PmCaptureFileType.MicrosoftNetworkMonitor;
                /* Begins with magic number 0xA1B2C3D4 or 0x4D3C2B1A significant for TCPDump/LibPCAP ver 2.3*/
                case 0x4d3c2b1a:
                case 0xa1b2c3d4:
                    return PmCaptureFileType.LibPcap;
                case 0x0A0D0D0A:
                    return PmCaptureFileType.PcapNextGen;
            }
            return PmCaptureFileType.Unknown;
        }

        private PmCaptureProcessorBlockBase GetPmCaptureProcessor(FileInfo captureFileInfo)
        {
            //Create binary reader and determine PCAP format type
            using(var fileBinaryReader = new BinaryReader(Stream.Synchronized(new FileStream(captureFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))))
            {
                var fileType = this.DetermineType(fileBinaryReader);

                //Base on detected PCAP format create instance of appropriate children class
                PmCaptureProcessorBlockBase captureProcessorBlock;
                switch(fileType)
                {
                    case PmCaptureFileType.MicrosoftNetworkMonitor:
                        captureProcessorBlock = this.PmCaptureProcessorBlockFactory.CreateMnm(captureFileInfo);
                        break;
                    case PmCaptureFileType.LibPcap:
                        captureProcessorBlock = this.PmCaptureProcessorBlockFactory.CreatePcap(captureFileInfo);
                        break;
                    case PmCaptureFileType.PcapNextGen:
                        captureProcessorBlock = this.PmCaptureProcessorBlockFactory.CreatePcapNg(captureFileInfo);
                        break;
                    // ReSharper disable RedundantCaseLabel
                    case PmCaptureFileType.Unknown:
                    // ReSharper restore RedundantCaseLabel
                    default:
                        throw new ControllerCaptureProcessorBase.UnknownFileType("CaptureProcessor file format can not be determined for " + captureFileInfo.FullName);
                }
                return captureProcessorBlock;
            }
        }

        private PmCaptureBase ProcessCaptureFile(FileInfo captureFileInfo)
        {
            var captureProcessor = this.GetPmCaptureProcessor(captureFileInfo);
            captureProcessor.LinkTo(this.PmParsedFramesBufferBlock);
            return captureProcessor.ProcessCapture().Result;
        }

        #region Implementation of IDataflowBlock
        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, FileInfo messageValue, ISourceBlock<FileInfo> source, bool consumeToAccept)
        {
            return this._ProcessCaptureFileActionBlock.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete() { this._ProcessCaptureFileActionBlock.Complete(); }
        public void Fault(Exception exception) { this._ProcessCaptureFileActionBlock.Fault(exception); }
        public Task Completion { get; }
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

        #region Implementation of ISourceBlock<out PmCaptureBase>
        public IDisposable LinkTo(ITargetBlock<PmCaptureBase> target, DataflowLinkOptions linkOptions) { return this._pmCapturesBufferBlock.LinkTo(target, linkOptions); }

        public PmCaptureBase ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureBase> target, out bool messageConsumed)
        {
            return this._pmCapturesBufferBlock.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureBase> target)
        {
            return this._pmCapturesBufferBlock.ReserveMessage(messageHeader, target);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmCaptureBase> target)
        {
            this._pmCapturesBufferBlock.ReleaseReservation(messageHeader, target);
        }
        #endregion
    }
}