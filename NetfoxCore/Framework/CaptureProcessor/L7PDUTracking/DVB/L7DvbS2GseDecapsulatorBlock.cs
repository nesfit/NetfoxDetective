using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netfox.Framework.ApplicationProtocolExport.PDUProviders;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet.Utils;

namespace Netfox.Framework.CaptureProcessor.L7PDUTracking.DVB
{
    /// <summary>
    /// TPL Dataflow Block for decapsulation of <i>Generic Stream Encapsulation</i> (<i>GSE</i>) inside <i>DVB-S2 Base Band
    /// Frames</i> with <i>Mode Adaptation Header L.3</i> sent as Layer 7 PDU.
    /// </summary>
    /// <remarks>
    /// Inspired by <a href="https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/walkthrough-creating-a-custom-dataflow-block-type">
    /// Walkthrough: Creating a Custom Dataflow Block Type</a> and  
    /// <a href="https://blogs.msdn.microsoft.com/pfxteam/2011/11/09/exception-handling-in-tpl-dataflow-networks/">
    /// Exception Handling in TPL Dataflow Networks</a>
    /// </remarks>
    internal class L7DvbS2GseDecapsulatorBlock : ITargetBlock<L7Conversation>, ISourceBlock<PmFrameBase>
    {
        private readonly ActionBlock<L7Conversation> _decapsulator;
        private readonly BufferBlock<PmFrameBase> _outputBuffer;

        public L7DvbS2GseDecapsulatorBlock()
        {
            this._outputBuffer = new BufferBlock<PmFrameBase>();
            this._decapsulator = new ActionBlock<L7Conversation>(async l7Conversation =>
            {
                var stream = new PDUStreamBasedProvider(l7Conversation, EfcPDUProviderType.ContinueInterlay);
                var reader = new PDUStreamReader(stream, Encoding.GetEncoding(437), true) {ReadBigEndian = true};

                var gseReassembler = new GseReassemblingDecapsulator();
                while (!reader.EndOfStream)
                {
                    try
                    {
                        var bb = BaseBandFrame.Parse(reader);

                        // It's important to get `reader.PDUStreamBasedProvider.GetCurrentPDU()` after parsing Base-Band frame.
                        // During reading, its value is set to the last read PDU. If we would call it before Base-Band parsing,
                        // retrieved value would not be current PDU, but the previous one.
                        // frames encapsulating parsed Base-Band
                        var pdu = reader.PDUStreamBasedProvider.GetCurrentPDU();
                        if (pdu == null)
                        {
                            break;
                        }

                        var frames = ImmutableList.CreateRange(pdu.FrameList);

                        // Reassemble any fragmented GSE packets and obtain decapsulated frames, if any of them have been just finished.
                        var decapsulatedFrames = gseReassembler.Process(bb, frames);
                        foreach (var f in decapsulatedFrames)
                        {
                            this._outputBuffer.Post(f);
                        }
                    }
                    catch (InvalidPacketFormatException)
                    {
                        // Current PDU of l7Conversation does not contain DVB-S2 Base Band Frame.
                    }
                    catch (NotImplementedException)
                    {
                        // NOTE: Only Generic Continuous Stream Input is supported at this moment.
                    }

                    // move to the next message
                    if (!reader.NewMessage())
                    {
                        break;
                    }
                }
            });
            this._decapsulator.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock) this._outputBuffer).Fault(t.Exception);
                else this._outputBuffer.Complete();
            });
        }

        #region Implementation of ITargetBlock<L7Conversation> proxy methods

        /// <summary>
        /// Proxy to offer <see cref="L7Conversation"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public DataflowMessageStatus OfferMessage(
            DataflowMessageHeader messageHeader,
            L7Conversation messageValue,
            ISourceBlock<L7Conversation> source,
            bool consumeToAccept)
        {
            return ((ITargetBlock<L7Conversation>) this._decapsulator).OfferMessage(messageHeader, messageValue, source,
                consumeToAccept);
        }

        #endregion

        #region Implementation of IDataflowBlock

        public void Complete()
        {
            this._decapsulator.Complete();
        }

        public void Fault(Exception exception)
        {
            ((IDataflowBlock) this._decapsulator).Fault(exception);
        }

        public Task Completion => this._outputBuffer.Completion;

        #endregion

        #region Implementation of ISourceBlock<PmFrameBase> proxy methods

        /// <summary>
        /// Proxy to link <see cref="PmFrameBase"/> to target.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public IDisposable LinkTo(ITargetBlock<PmFrameBase> target, DataflowLinkOptions linkOptions)
        {
            return this._outputBuffer.LinkTo(target, linkOptions);
        }

        /// <summary>
        /// Proxy to consume <see cref="PmFrameBase"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public PmFrameBase ConsumeMessage(
            DataflowMessageHeader messageHeader,
            ITargetBlock<PmFrameBase> target,
            out bool messageConsumed)
        {
            return ((ISourceBlock<PmFrameBase>) this._outputBuffer).ConsumeMessage(messageHeader, target,
                out messageConsumed);
        }

        /// <summary>
        /// Proxy to reserve <see cref="PmFrameBase"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target)
        {
            return ((ISourceBlock<PmFrameBase>) this._outputBuffer).ReserveMessage(messageHeader, target);
        }

        /// <summary>
        /// Proxy to release reservation of <see cref="PmFrameBase"/>.
        /// <br/><inheritdoc/>
        /// </summary>
        /// <inheritdoc/>
        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<PmFrameBase> target)
        {
            ((ISourceBlock<PmFrameBase>) this._outputBuffer).ReleaseReservation(messageHeader, target);
        }

        #endregion
    }
}