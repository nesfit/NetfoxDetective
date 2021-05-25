using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Netfox.Core.Interfaces.Model;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public class PDUStreamBasedProvider : PDUDataProvider, IPduStreamBasedProvider
    {
        #region Constructor

        public PDUStreamBasedProvider(L7Conversation conversation, EfcPDUProviderType pduProviderType)
        {
            this.Conversation = conversation;
            this._conversationPDUs = Enumerable.ToList<PDU>(this.Conversation.L7PDUs.Select(pdu => new PDU
            {
                Pdu = pdu
            }));
            switch (pduProviderType)
            {
                case EfcPDUProviderType.Mixed:
                    this._next = this.NextMixed;
                    this._previous = this.PreviousMixed;
                    break;
                case EfcPDUProviderType.Breaked:
                    this._next = this.NextBreaked;
                    this._previous = this.PreviousBreaked;
                    break;
                case EfcPDUProviderType.ContinueInterlay:
                    this._next = this.NextContinueInterlay;
                    this._previous = this.PreviousContinueInterlay;
                    break;
                case EfcPDUProviderType.SingleMessage:
                    this._next = this.NextSingleMessage;
                    this._previous = this.PreviousSingleMessage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pduProviderType));
            }
        }

        #endregion

        #region MemberVariables

        /// <summary>
        ///     The Conversation
        /// </summary>
        public L7Conversation Conversation { get; }

        /// <summary>
        ///     The BidirectionalFlow PDUss.
        /// </summary>
        private readonly List<PDU> _conversationPDUs;

        /// <summary>
        ///     The total read pdu bytes.
        /// </summary>
        internal Int64 TotalReadedPDUbytes;

        /// <summary>
        ///     true to last PDU.
        /// </summary>
        private Boolean _lastPDU;

        /// <summary>
        ///     The current pdu index.
        /// </summary>
        private Int32 _currentPDUindex;

        /// <summary>
        ///     The current PDU peek index.
        /// </summary>
        private Int32 _currentPDUPeekIndex;

        #endregion

        #region NOTSupported

        /// <summary>
        ///     When overridden in a derived class, clears all buffers for this stream and causes any
        ///     buffered data to be written to the underlying device.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary> When overridden in a derived class, sets the length of the current stream.</summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="value"> The desired length of the current stream in bytes. </param>
        public override void SetLength(Int64 value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     When overridden in a derived class, writes a sequence of bytes to the current stream and
        ///     advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <param name="buffer">
        ///     An array of bytes. This method copies <paramref name="count" /> bytes
        ///     from <paramref name="buffer" /> to the current stream.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which to
        ///     begin copying bytes to the current stream.
        /// </param>
        /// <param name="count">  The number of bytes to be written to the current stream. </param>
        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        /// <value> A long value representing the length of the stream in bytes.</value>
        public override Int64 Length
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region STREAM options

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports reading.
        /// </summary>
        /// <value> true if the stream supports reading; otherwise, false.</value>
        public override Boolean CanRead => true;

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports seeking.
        /// </summary>
        /// <value> true if the stream supports seeking; otherwise, false.</value>
        public override Boolean CanSeek => true;

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports writing.
        /// </summary>
        /// <value> true if the stream supports writing; otherwise, false.</value>
        public override Boolean CanWrite => false;

        #endregion

        #region Stream manipulation methods

        /// <summary> Init stream to begin reading a new application message.</summary>
        /// <remarks> Pluskal, 2/11/2014.</remarks>
        /// <returns> true if it succeeds, false is there ano reamaining messages.</returns>
        public override Boolean NewMessage()
        {
            if (this._conversationPDUs.Count == 0) //TODO check when this happens, using SIP sleuth test
            {
                return this.Reset();
            }

            if (this.Current.Offset != 0 || this._currentPDUindex == 0)
            {
                this._conversationPDUs.RemoveAt(this._currentPDUindex);
            }

            // All pdus before current without current and in reverse order.
            // Reverse because List indexation.
            for (var i = this._currentPDUindex - 1; i >= 0; i--)
            {
                var pdu = this.Value(i);
                if (pdu.IsAllRead)
                {
                    this._conversationPDUs.RemoveAt(i);
                }
            }

            return this.Reset();
        }

        /// <summary>
        ///     When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the requested operation is not
        ///     supported.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when origin is not supported. </exception>
        /// <param name="offset"> A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">
        ///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the
        ///     reference point used to obtain the new position.
        /// </param>
        /// <returns> The new position within the current stream.</returns>
        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            var seeked = 0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset <= 0)
                    {
                        this.Reset();
                        return 0;
                    }

                    //if (offset <= Position)
                    //{
                    //    Reset();
                    //}
                    //while (offset > seeked)
                    //    {
                    //        if (offset > Current.RemainingBytes + seeked)
                    //        {
                    //            seeked += Current.RemainingBytes;
                    //            if (!MoveNext())
                    //                return Position;
                    //        }
                    //        else
                    //        {
                    //            Current.Offset = Current.Offset + (int)(offset - seeked);
                    //            return Position;
                    //        }
                    //    }
                    return this.Seek(offset - this.Position, SeekOrigin.Current);
                case SeekOrigin.Current:
                    if (offset < 0)
                    {
                        this.EndOfStream = false;
                        while (-offset > seeked)
                        {
                            if (-offset > this.Current.Offset + seeked)
                            {
                                seeked += (this.Current.Offset == 0 && this._currentPDUindex != 0)
                                    ? this.Current.Length
                                    : this.Current.Offset;
                                if (!this.MovePrevious())
                                {
                                    return this.Position;
                                }
                            }
                            else
                            {
                                this.Current.Offset = this.Current.Offset - (Int32) (-offset) - seeked;
                                return this.Position;
                            }
                        }
                    }
                    else
                    {
                        while (offset > seeked)
                        {
                            if (offset > this.Current.RemainingBytes + seeked)
                            {
                                seeked += this.Current.RemainingBytes;
                                if (!this.MoveNext())
                                {
                                    this.EndOfStream = true;
                                    return this.Position;
                                }
                            }
                            else
                            {
                                this.Current.Offset = this.Current.Offset + (Int32) (offset - seeked);
                                return this.Position;
                            }
                        }
                    }

                    return this.Position;
                case SeekOrigin.End:
                    throw new NotSupportedException("Seeking from the end of stream is not supported.");
                default:
                    throw new ArgumentOutOfRangeException("Not supported origin type.");
            }

            //return 0;
        }

        /// <summary>
        ///     When overridden in a derived class, reads a sequence of bytes from the current stream and
        ///     advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the buffer contains
        ///     the specified byte array with the values between <paramref name="offset" /> and
        ///     (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from
        ///     the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which
        ///     to begin storing the data read from the current stream.
        /// </param>
        /// <param name="requestedCount"> Number of requested bytes. </param>
        /// <returns>
        ///     The total number of bytes read into the buffer. This can be less than the number of bytes
        ///     requested if that many bytes are not currently available, or zero (0) if the end of the
        ///     stream has been reached.
        /// </returns>
        /// ###
        /// <param name="count"> The maximum number of bytes to be read from the current stream. </param>
        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 requestedCount)
        {
            if (!this._conversationPDUs.Any())
            {
                this.EndOfStream = true;
                return 0;
            }

            Int64 bufferOffset = offset;
            Int64 count = requestedCount;
            //var successfulUpdate = true;

            do
            {
                var remainLenInPDU = this.Current.Bytes.Count() - this.Current.Offset;
                var currentCount = remainLenInPDU > count ? count : remainLenInPDU;

                Array.Copy(this.Current.Bytes, this.Current.Offset, buffer, bufferOffset, currentCount);
                bufferOffset += currentCount;
                count -= currentCount;
                this.Current.Offset += (Int32) currentCount;
                if (this.Current.RemainingBytes == 0)
                {
                    this.EndOfPDU = true;
                }

                if (count != 0)
                {
                    this.EndOfStream = !this.MoveNext();
                }
            } while (!this.EndOfStream && count > 0);

            return (Int32) bufferOffset - offset;
        }

        public Boolean EndOfStream { get; private set; }

        public Boolean EndOfPDU { get; private set; }

        /// <summary>
        ///     Reads a sequence of bytes from the current stream without advancing the position within the
        ///     stream.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is
        ///     unimplemented.
        /// </exception>
        /// <param name="buffer">         The buffer. </param>
        /// <param name="bufferOffset">   The buffer offset. </param>
        /// <param name="streamPosition"> The offset in stream. </param>
        /// <param name="requestedCount"> Number of requested bytes. </param>
        /// <param name="origin">         The origin point from where the streamPosition is referenced. </param>
        /// <returns> The current top-of-stack object.</returns>
        public override Int32 Peek(Byte[] buffer, Int32 bufferOffset, Int32 streamPosition, Int32 requestedCount,
            SeekOrigin origin)
        {
            if (!this._conversationPDUs.Any())
            {
                return 0;
            }

            if (origin != SeekOrigin.Current || streamPosition < 0)
            {
                throw new NotImplementedException("Not implemented yet, request implementation.");
            }

            Int64 offset = bufferOffset;
            Int64 count = requestedCount;
            var successfulUpdate = false;
            this._currentPDUPeekIndex = this._currentPDUindex;

            streamPosition += this.Current.Offset;
            while (streamPosition > 0)
            {
                if (streamPosition > this.CurrentPeek.Length)
                {
                    streamPosition -= this.CurrentPeek.Length;
                    if (!this.PeekNext())
                    {
                        return (Int32) offset - bufferOffset;
                    }
                }
                else
                {
                    break;
                }
            }

            do
            {
                var remainLenInPDU = this.CurrentPeek.Bytes.Count() - streamPosition;
                var currentCount = remainLenInPDU > count ? count : remainLenInPDU;

                Array.Copy(this.CurrentPeek.Bytes, streamPosition, buffer, offset, currentCount);
                offset += currentCount;
                count -= currentCount;
                streamPosition = 0;
                if (count != 0)
                {
                    successfulUpdate = this.PeekNext();
                }
            } while (successfulUpdate && count > 0);

            this._currentPDUPeekIndex = -1;
            return (Int32) offset - bufferOffset;
        }

        /// <summary>
        ///     When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value> The current position within the stream.</value>
        public override Int64 Position
        {
            get { return this.TotalReadedPDUbytes + ((this._conversationPDUs.Any()) ? this.Current.Offset : 0); }
            set { this.Seek(value, SeekOrigin.Begin); }
        }

        #endregion

        #region Enumerator methods

        /// <summary> Gets PDU.</summary>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> The PDU on given index.</returns>
        private PDU Value(Int32 index)
        {
            if (index < 0 || index >= this._conversationPDUs.Count)
            {
                throw new InvalidOperationException();
            }

            return this._conversationPDUs[index];
        }

        /// <summary> Gets the current PDU.</summary>
        /// <value> The current PDU.</value>
        /// ###
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is
        ///     invalid.
        /// </exception>
        private PDU Current => this.Value(this._currentPDUindex);

        /// <summary> Gets the current peeked PDU.</summary>
        /// <value> The current peeked PDU.</value>
        /// ###
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is
        ///     invalid.
        /// </exception>
        private PDU CurrentPeek => this.Value(this._currentPDUPeekIndex);

        /// <summary> Resets the stream to begin position.</summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <returns> True if reset was successful and there are some PDUs remaining.</returns>
        public override Boolean Reset()
        {
            foreach (var conversationPdU in this._conversationPDUs
            ) //TODO maybe it would be enough to reset only PDUs with index < currentPDUindex
            {
                conversationPdU.Offset = 0;
            }

            this.TotalReadedPDUbytes = 0;
            this._lastPDU = false;
            this._currentPDUindex = 0;
            this.EndOfStream = !this._conversationPDUs.Any();
            if (this.EndOfStream || (this._conversationPDUs.Any() && this.Current.RemainingBytes == 0))
            {
                this.EndOfPDU = true;
            }
            else
            {
                this.EndOfPDU = false;
            }

            return !this.EndOfStream;
        }

        /// <summary> Move to previous PDU.</summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean MovePrevious()
        {
            if (this._currentPDUindex <= 0)
            {
                return false;
            }

            this.Current.Offset = 0;
            this._lastPDU = false;
            return this._previous(ref this._currentPDUindex);
        }

        /// <summary> Peek to next PDU.</summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean PeekNext() => this._next(ref this._currentPDUPeekIndex);

        /// <summary> Move to next PDU.</summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean MoveNext()
        {
            if (this._lastPDU)
            {
                return false;
            }

            var readedPDUbytes = this.Current.Length;
            this.Current.Offset = this.Current.Length;

            this._lastPDU = !this._next(ref this._currentPDUindex);
            if (this._lastPDU)
            {
                return false;
            }

            this.TotalReadedPDUbytes += readedPDUbytes;
            this.Current.Offset = 0;
            return true;
        }

        /// <summary> Moves the given index PDU.</summary>
        /// <param name="indexPDU"> [in,out] The index PDU. </param>
        /// <returns> A bool.</returns>
        private delegate Boolean Move(ref Int32 indexPDU);

        /// <summary> The next.</summary>
        private readonly Move _next;

        /// <summary> The previous.</summary>
        private readonly Move _previous;

        #endregion

        #region Enumeration methods depends on PDU provider method

        /// <summary>
        ///     Override in derivated classes to reached differant functionality in serving PDUs to stream.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <param name="indexPDU"> [in,out] The index PDU. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean NextMixed(ref Int32 indexPDU)
        {
            Debug.Assert(indexPDU >= 0);
            if (indexPDU >= this._conversationPDUs.Count - 1)
            {
                return false;
            }

            indexPDU++;
            return true;
        }

        /// <summary>
        ///     Override in derivated classes to reached differant functionality in serving PDUs to stream.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <param name="indexPDU"> [in,out] The index PDU. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean PreviousMixed(ref Int32 indexPDU)
        {
            Debug.Assert(indexPDU >= 0);
            if (indexPDU == 0)
            {
                return false;
            }

            indexPDU--;
            return true;
        }

        /// <summary> Next continue interlay.</summary>
        /// <param name="indexpdu"> [in,out] The indexpdu. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean NextContinueInterlay(ref Int32 indexpdu)
        {
            var originalIndex = indexpdu;
            while (indexpdu != this._conversationPDUs.Count - 1)
            {
                indexpdu++;
                if (this.Value(0).Pdu.FlowDirection == this.Current.Pdu.FlowDirection)
                {
                    return true;
                }
            }

            indexpdu = originalIndex;
            return false;
        }

        /// <summary> Previous continue interlay.</summary>
        /// <param name="indexpdu"> [in,out] The indexpdu. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean PreviousContinueInterlay(ref Int32 indexpdu)
        {
            while (indexpdu > 0)
            {
                indexpdu--;
                if (this.Value(0).Pdu.FlowDirection == this.Current.Pdu.FlowDirection)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> Next breaked.</summary>
        /// <param name="indexpdu"> [in,out] The indexpdu. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean NextBreaked(ref Int32 indexpdu)
        {
            while (indexpdu != this._conversationPDUs.Count - 1)
            {
                indexpdu++;
                if (this.Value(0).Pdu.FlowDirection == this.Current.Pdu.FlowDirection)
                {
                    return true;
                }

                indexpdu--;
                return false;
            }

            return false;
        }

        /// <summary> Previous breaked.</summary>
        /// <param name="indexpdu"> [in,out] The indexpdu. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean PreviousBreaked(ref Int32 indexpdu)
        {
            while (indexpdu > 0)
            {
                indexpdu--;
                if (this.Value(0).Pdu.FlowDirection == this.Current.Pdu.FlowDirection)
                {
                    return true;
                }

                indexpdu--;
                return false;
            }

            return false;
        }

        /// <summary> Next SingleMessage.</summary>
        /// <param name="indexpdu"> [in,out] The indexpdu. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean NextSingleMessage(ref Int32 indexpdu) => false;

        /// <summary> Previous SingleMessage.</summary>
        /// <param name="indexpdu"> [in,out] The indexpdu. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean PreviousSingleMessage(ref Int32 indexpdu) => false;

        #endregion

        #region PDU Accessing

        public override L7PDU GetCurrentPDU()
        {
            return this._conversationPDUs.Count == 0 ? null : this._conversationPDUs[this._currentPDUindex].Pdu;
        }

        #endregion

        public IEnumerable<PmFrameBase> ProcessedFrames =>
            this._conversationPDUs.Take(this._currentPDUindex).SelectMany(p => p.Pdu.FrameList);

        public IEnumerable<PmFrameBase> ConversationFramesFrames => this.Conversation.Frames;
    }
}