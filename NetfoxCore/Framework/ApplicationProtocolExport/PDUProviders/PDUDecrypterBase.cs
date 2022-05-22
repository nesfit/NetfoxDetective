using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Netfox.Framework.ApplicationProtocolExport.Comparers;
using Netfox.Framework.Models;
using Netfox.Framework.Models.Enums;
using Netfox.Framework.Models.PmLib;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    public class PDUDecrypterBase : PDUStreamBasedProvider
    {
        #region Constructors, Constants and Private Properties

        // Version Major
        public const Byte Ssl3VerMajor = 3;

        // SSL Protocols identifications
        public const Byte ChangeCipherSpec = 20;
        public const Byte Alert = 21;
        public const Byte Handshake = 22;
        public const Byte Record = 23;

        // Handshake messages identifications
        public const Byte ClientHello = 1;
        public const Byte ServerHello = 2;
        public const Byte ClientKeyExchange = 16;
        public const Byte Finished = 20;

        // Change Cipher Spec messages identifications
        public const Byte ChangeCipherSpecMessage = 1;

        public enum SslSessionState
        {
            NegotiationInit,
            Negotiation,
            NegotiationChangeCipherSpec,
            NegotiationFinished,
            Intermezzo,
            DataExchange
        }

        public List<DecryptedData> DataList { get; } = new List<DecryptedData>();

        private Int32 _decryptedDataCnt;

        private Int32 _decryptedDataOffset;

        private SslSessionState _state;

        private Int32 _msgUnderReview;

        private readonly L7Conversation _conversation;

        private L7PDU[] Pdus { get; set; }

        private Byte[] _decryptedBytes;

        private readonly List<L7PDU> _processedPdus = new List<L7PDU>();

        private PmFrameBase _previousFrame;

        private Boolean _dataDecrypted;

        private Boolean _lastPDU;

        private static readonly Dictionary<Byte[], Byte[]> SessionsMasterKeys =
            new Dictionary<Byte[], Byte[]>(new ByteArrayComparer());

        private readonly Byte[] _currentSessionID = new byte[32];

        private DecryptedData Current => this.DataList.Any() ? this.DataList.ElementAt(this._decryptedDataCnt) : null;

        public PDUDecrypterBase(L7Conversation conversation, EfcPDUProviderType pduProviderType) : base(conversation,
            pduProviderType)
        {
            this._conversation = conversation;
            //this._frames = this._conversation.Frames.OrderBy(f => f.FrameIndex);


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
                    throw new ArgumentOutOfRangeException("pduProviderType");
            }

            this.Reset();
        }

        #endregion

        #region Public Properties

        public override Int64 Length
        {
            get { throw new NotImplementedException(); }
        }

        public override Boolean CanRead
        {
            get { return true; }
        }

        public override Boolean CanSeek
        {
            get { return true; }
        }

        public override Boolean CanWrite
        {
            get { return false; }
        }

        public override Int64 Position
        {
            get { return this.TotalReadedPDUbytes + ((this.DataList.Any()) ? this.Current?.DataOffset ?? 0 : 0); }
            set { this.Seek(value, SeekOrigin.Begin); }
        }

        public PDUDecrypter Decrypter { get; private set; }

        #endregion

        #region Stream Manipulation

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override Int64 Seek(Int64 offset, SeekOrigin origin)
        {
            var seeked = 0;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset <= 0)
                    {
                        this.ResetSoft();
                        return 0;
                    }

                    return this.Seek(offset - this.Position, SeekOrigin.Current);
                case SeekOrigin.Current:
                    if (offset < 0)
                    {
                        this.EndOfStream = false;
                        while (-offset > seeked)
                        {
                            if (this.Current == null) return 0;
                            if (-offset > this.Current.DataOffset + seeked)
                            {
                                seeked += (this.Current.DataOffset == 0 && this._decryptedDataCnt != 0)
                                    ? this.Current.DataLength
                                    : this.Current.DataOffset;
                                if (!this.MovePrevious())
                                {
                                    return this.Position;
                                }
                            }
                            else
                            {
                                this.Current.DataOffset = this.Current.DataOffset - (Int32) (-offset) - seeked;
                                return this.Position;
                            }
                        }
                    }
                    else
                    {
                        while (offset > seeked)
                        {
                            if (this.Current == null) return 0;
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
                                this.Current.DataOffset = this.Current.DataOffset + (Int32) (offset - seeked);
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
        }

        public override Int32 Read(Byte[] buffer, Int32 offset, Int32 requestedCount)
        {
            if (this._dataDecrypted == false)
            {
                this.DecryptPdus();
            }

            if (!this.DataList.Any())
            {
                this.EndOfStream = true;
                return 0;
            }

            Int64 bufferOffset = offset;
            Int64 count = requestedCount;

            do
            {
                var decryptedPDU = this.DataList[this._decryptedDataCnt];

                var remainLenInPDU = decryptedPDU.DataLength - decryptedPDU.DataOffset;
                var currentCount = remainLenInPDU > count ? count : remainLenInPDU;

                Array.Copy(decryptedPDU.Data, decryptedPDU.DataOffset, buffer, bufferOffset, currentCount);
                bufferOffset += currentCount;
                count -= currentCount;
                decryptedPDU.DataOffset += (Int32) currentCount;
                // if (decryptedPDU.RemainingBytes == 0) { this.EndOfPDU = true; }
                if (count != 0)
                {
                    this.EndOfStream = !this.MoveNext();
                }
            } while (!this.EndOfStream && count > 0);

            return (Int32) bufferOffset - offset;
        }

        public override Int32 Peek(Byte[] buffer, Int32 bufferOffset, Int32 streamPosition, Int32 requestedCount,
            SeekOrigin origin)
        {
            /* save current position */
            var dataCnt = this._decryptedDataCnt;
            var dataOffset = this._decryptedDataOffset;
            var sPos = this.Position;

            /* goto requested position */
            this.Seek(streamPosition, origin);

            /* read from that position */
            var len = this.Read(buffer, bufferOffset, requestedCount);

            /* reset position */
            this._decryptedDataCnt = dataCnt;
            this._decryptedDataOffset = dataOffset;
            this.Position = sPos;

            return len;
        }

        public override Boolean Reset()
        {
            this._state = SslSessionState.NegotiationInit;
            this._msgUnderReview = 0;
            this._decryptedDataCnt = 0;
            this._decryptedDataOffset = 0;
            this.DataList.Clear();
            this._decryptedBytes = null;
            this._processedPdus.Clear();
            this._dataDecrypted = false;
            this._lastPDU = false;
            this.Pdus = this._conversation.L7PDUs.ToArray();
            this.Decrypter = new PDUDecrypter();
            SessionsMasterKeys.Clear();

            return this.ResetSoft();
        }

        public Boolean ResetSoft()
        {
            this.TotalReadedPDUbytes = 0;
            this._decryptedDataCnt = 0;
            this._decryptedDataOffset = 0;
            this._lastPDU = false;
            this.EndOfStream = !this.DataList.Any();
            return !this.EndOfStream;
        }

        public override L7PDU GetCurrentPDU() => this._decryptedDataCnt < this.DataList.Count
            ? this.DataList.ElementAt(this._decryptedDataCnt).PDU
            : null;

        public override void SetLength(Int64 value)
        {
            throw new NotImplementedException();
        }

        public override void Write(Byte[] buffer, Int32 offset, Int32 count)
        {
            throw new NotImplementedException();
        }

        public override Boolean NewMessage()
        {
            if (this._dataDecrypted == false)
            {
                this.DecryptPdus();
            }

            if (!this.DataList.Any())
            {
                return false;
            }

            if (this.Current.DataOffset != 0 || this._decryptedDataCnt == 0)
            {
                this.DataList.RemoveAt(this._decryptedDataCnt);
            }

            for (var i = this._decryptedDataCnt - 1; i >= 0; i--)
            {
                var decryptedData = this.DataList[i];
                if (decryptedData.IsAllRead)
                {
                    this.DataList.RemoveAt(i);
                }
            }

            return this.ResetSoft();
        }

        protected Boolean DecryptPdus()
        {
            if (!this.Pdus.Any()) return false;

            // decrypt next message
            for (this._msgUnderReview = 0; this._msgUnderReview < this.Pdus.Count(); this._msgUnderReview++)
            {
                //if (this._msgUnderReview == this._pdus.Count())
                //    return false;

                var pduElement = this.Pdus.ElementAt(this._msgUnderReview);
                this._processedPdus.Add(pduElement);
                for (var frameNum = 0; frameNum < pduElement.FrameList.Count(); frameNum++)
                {
                    //Console.WriteLine("Frame > "+frame.FrameIndex);

                    var frame = pduElement.FrameList.ElementAt(frameNum);
                    if (frame is PmFrameVirtualBlank) continue;

                    var l7Data = new Byte[frame.PmPacket.SegmentPayloadLength];
                    Array.Copy(frame.L7Data(), 0, l7Data, 0, l7Data.Length);

                    if (this.Decrypter.ContinuationData != null && l7Data.Length != 0 && this._previousFrame != null)
                    {
                        // Check that the frame is in the same direction as previous in processing
                        // when continuation of data happend last decryption ended with false and state change to INTERMEZZO
                        if (Enumerable.SequenceEqual<byte>(frame.SourceEndPoint.Address.GetAddressBytes(),
                            this._previousFrame.SourceEndPoint.Address.GetAddressBytes()))
                        {
                            var tmp = l7Data;
                            l7Data = new Byte[tmp.Length + this.Decrypter.ContinuationData.Length];
                            Array.Copy(this.Decrypter.ContinuationData, 0, l7Data, 0,
                                this.Decrypter.ContinuationData.Length);
                            Array.Copy(tmp, 0, l7Data, this.Decrypter.ContinuationData.Length, tmp.Length);
                            this.Decrypter.ContinuationData = null;
                        }
                        else
                        {
                            // If not same direction try to decrypt what is left in _continuationData
                            this._msgUnderReview--;
                            frame = this._previousFrame;
                            l7Data = new Byte[this.Decrypter.ContinuationData.Length];
                            Array.Copy(this.Decrypter.ContinuationData, 0, l7Data, 0,
                                this.Decrypter.ContinuationData.Length);
                            this.Decrypter.ContinuationData = null;
                        }
                    }

                    // Here it is safe to set previous frame
                    this._previousFrame = frame;

                    try
                    {
                        switch (this._state)
                        {
                            case SslSessionState.NegotiationInit:
                                this.AwaitHelloMessages(l7Data, frame);
                                break;
                            case SslSessionState.Negotiation:
                                this.AwaitClientKeyExchange(l7Data, frame);
                                break;
                            case SslSessionState.NegotiationChangeCipherSpec:
                                this.AwaitChangeCipherSpecMessage(l7Data, frame);
                                break;
                            case SslSessionState.NegotiationFinished:
                                this.AwaitFinishedMessage(l7Data, frame);
                                break;
                            case SslSessionState.Intermezzo:
                                this.AwaitEncryptedMessage(l7Data, frame);
                                break;
                            case SslSessionState.DataExchange:
                                this.AwaitEncryptedMessage(l7Data, frame);
                                break;
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        // No L7 data
                        PmConsolePrinter.PrintError("PDUDecrypter : " + e.Message + " : in frame " + frame.FrameIndex);
                    }
                    catch (NotImplementedException e)
                    {
                        ("PDUDecrypterCipher : " + e.Message).PrintInfo();
                        //return false;
                    }
                }

                if (this._decryptedBytes != null)
                {
                    //this.DataList.Add(new DecryptedData { Data = this._decryptedBytes, pdu = this._processedPdus.Last() });
                    this.DataList.Add(new DecryptedData
                    {
                        Data = this._decryptedBytes,
                        PDU = this.Pdus.ElementAt(this._msgUnderReview)
                    });
                    this._decryptedBytes = null;
                    this.EndOfStream = false;
                }
            }

            while (this.Decrypter.ContinuationData != null) //Process remaining data in buffer
            {
                var l7Data = new byte[this.Decrypter.ContinuationData.Length];
                Array.Copy(this.Decrypter.ContinuationData, l7Data, this.Decrypter.ContinuationData.Length);
                this.Decrypter.ContinuationData = null;
                try
                {
                    this.AwaitEncryptedMessage(l7Data, this._previousFrame);
                }
                catch (NullReferenceException e)
                {
                    // No L7 data
                    PmConsolePrinter.PrintError("PDUDecrypter : " + e.Message + " : in frame " +
                                                this._previousFrame.FrameIndex);
                }
                catch (NotImplementedException e)
                {
                    ("PDUDecrypterCipher : " + e.Message).PrintInfo();
                    //return false;
                }

                if (this._decryptedBytes != null)
                {
                    this.DataList.Add(new DecryptedData
                    {
                        Data = this._decryptedBytes,
                        PDU = this._processedPdus.Last()
                    });
                }

                this._decryptedBytes = null;
                this.EndOfStream = false;
            }

            this._dataDecrypted = true;

            return true;
        }

        private void AwaitEncryptedMessage(Byte[] l7Data, PmFrameBase pdu)
        {
            if (l7Data[0] == Record)
            {
                // record protocol
                this._state = this.Decrypter.DoDecryption(l7Data, pdu, ref this._decryptedBytes)
                    ? SslSessionState.DataExchange
                    : SslSessionState.Intermezzo;
            }
            else if (l7Data[0] == Handshake)
            {
                // tls handshaking protocol
                // renegotiation
                // TODO could be Finished?
                this._msgUnderReview--;
                // TODO isn't renegotiation encrypted??
                this._state = SslSessionState.NegotiationInit;
            }
            else if (l7Data[0] == ChangeCipherSpec && l7Data.Length > 6) // ChangeCipherSpec message has 6 bytes
            {
                // 'Finished' is in this pdu - TODO can be in next pdu?
                var finished = new Byte[l7Data.Length - 6];
                Array.Copy(l7Data, 6, finished, 0, finished.Length);
                this.Decrypter.DoDecryption(finished, pdu, ref this._decryptedBytes);
                this._decryptedBytes = null;

                this._state = SslSessionState.Intermezzo;
            }
            else if (l7Data[0] == Alert)
            {
                // alert protocol
                // ignore
                this._state = SslSessionState.Intermezzo;
            }

            // else something wrong 
        }

        private void AwaitClientKeyExchange(Byte[] l7Data, PmFrameBase pdu)
        {
            if (l7Data[0] == Handshake && l7Data[1] == Ssl3VerMajor && l7Data[5] == ClientKeyExchange)
            {
                this.Decrypter.ClientDirection = pdu.SrcAddress.GetAddressBytes();
                this.Decrypter.KeyDecrypter.ParseClientKeyExchange(l7Data);
                var preMaster = this.Decrypter.KeyDecrypter.DecryptKey();

                // use premaster to get ciphering key
                var master = this.Decrypter.Prf(preMaster, "master secret", this.Decrypter.ClientRnd,
                    this.Decrypter.ServerRnd, 48, l7Data[2]);

                // Save Master key for current session
                SessionsMasterKeys.Add(this._currentSessionID, master);


                var keyBlock = this.Decrypter.Prf(master, "key expansion", this.Decrypter.ServerRnd,
                    this.Decrypter.ClientRnd,
                    this.Decrypter.DataDecrypter.MacKeyLength * 2 + this.Decrypter.DataDecrypter.KeyLength * 2 +
                    this.Decrypter.DataDecrypter.IvLength * 2, l7Data[2]);

                this.Decrypter.ClientHMacKey = new Byte[this.Decrypter.DataDecrypter.MacKeyLength];
                this.Decrypter.ServerHMacKey = new Byte[this.Decrypter.DataDecrypter.MacKeyLength];

                Array.Copy(keyBlock, 0, this.Decrypter.ClientHMacKey, 0, this.Decrypter.DataDecrypter.MacKeyLength);
                Array.Copy(keyBlock, this.Decrypter.DataDecrypter.MacKeyLength, this.Decrypter.ServerHMacKey, 0,
                    this.Decrypter.DataDecrypter.MacKeyLength);

                // Only part of key_block is used to encrypt data
                this.Decrypter.DataDecrypter.Init(keyBlock);

                // Look for finished - sent immediately after change cipher spec
                var totalLen = 0;
                do
                {
                    var t = new Byte[2];
                    Array.Copy(l7Data, 3, t, 0, 2);
                    Array.Reverse(t);
                    totalLen += 5;
                    totalLen += BitConverter.ToInt16(t, 0);
                } while (totalLen < l7Data.Length && l7Data[totalLen] != ChangeCipherSpec);

                if (totalLen < l7Data.Length)
                {
                    // 'Finished' is in this pdu
                    totalLen += 6; // ChangeCipherSpec message has 6 bytes
                    var finished = new Byte[l7Data.Length - totalLen];
                    Array.Copy(l7Data, totalLen, finished, 0, finished.Length);
                    this.Decrypter.DoDecryption(finished, pdu, ref this._decryptedBytes);
                    this._decryptedBytes = null;

                    this._state = SslSessionState.Intermezzo;
                }
                else
                {
                    this._state = SslSessionState.NegotiationChangeCipherSpec;
                }
            }
        }

        private void AwaitChangeCipherSpecMessage(Byte[] l7Data, PmFrameBase pdu)
        {
            if (l7Data[0] == ChangeCipherSpec && l7Data[1] == Ssl3VerMajor && l7Data[5] == ChangeCipherSpecMessage)
            {
                //1 byte for content type
                //2 bytes for protocol version
                //2 bytes for message length
                //1 byte for change cypher spec message
                if (l7Data.Length > 6) //If finished message follows directly after Change Cipher Spec
                {
                    var l7FinishedMsg = new byte[l7Data.Length - 6];
                    Array.Copy(l7Data, 6, l7FinishedMsg, 0, l7Data.Length - 6);
                    this.AwaitFinishedMessage(l7FinishedMsg, pdu);
                }
                else this._state = SslSessionState.NegotiationFinished;
            }
        }

        private void AwaitFinishedMessage(Byte[] l7Data, PmFrameBase pdu)
        {
            if (l7Data[0] == Handshake && l7Data[1] == Ssl3VerMajor)
            {
                this.Decrypter.DoDecryption(l7Data, pdu, ref this._decryptedBytes);
                this._decryptedBytes = null;

                this._state = SslSessionState.Intermezzo;
            }
        }

        private void AwaitHelloMessages(Byte[] l7Data, PmFrameBase pdu)
        {
            // Get server hello message
            if (l7Data[0] == Handshake && l7Data[1] == Ssl3VerMajor && l7Data[5] == ServerHello)
            {
                this.Decrypter.ServerRnd = new Byte[32];

                Array.Copy(l7Data, 11, this.Decrypter.ServerRnd, 0, this.Decrypter.ServerRnd.Length);

                ConversationCipherSuite conversationCipherSuite;
                KeyDecrypter keyDecrypter;
                DataDecrypter dataDecrypter;

                this.Decrypter.ChangeCipherSuite(l7Data, out conversationCipherSuite);
                this._conversation.CipherSuite = conversationCipherSuite;

                CipherSuiteInitializer.PrepareDecryptingAlgorithms(this._conversation.Key.ServerPrivateKey,
                    this._conversation.CipherSuite, out keyDecrypter, out dataDecrypter);

                this.Decrypter.KeyDecrypter = keyDecrypter;
                this.Decrypter.DataDecrypter = dataDecrypter;

                // Get SSL Session ID and save it
                var sessionID = new Byte[32];
                Array.Copy(l7Data, 44, sessionID, 0, 32);
                Array.Copy(sessionID, this._currentSessionID, this._currentSessionID.Length);

                // Get ServerHello content length
                var serverHelloContentLengthBytes = new Byte[2];
                Array.Copy(l7Data, 3, serverHelloContentLengthBytes, 0, 2);
                Array.Reverse(serverHelloContentLengthBytes);
                var serverHelloContentLength = BitConverter.ToUInt16(serverHelloContentLengthBytes, 0);

                // Get total ServerHello length
                // ContentType(1B) Version(2B) Length(2B) Content
                var serverHelloLength = 1 + 2 + 2 + serverHelloContentLength;

                // Check whether there is something else after ServerHello message and whether it is ChangeCipherSpec message
                // If yes, session resumption is used 
                if (serverHelloLength < l7Data.Length && l7Data[serverHelloLength + 0] == ChangeCipherSpec &&
                    SessionsMasterKeys.ContainsKey(sessionID))
                {
                    this.Decrypter.ClientDirection = pdu.DstAddress.GetAddressBytes();

                    // Load saved Master key for given Session ID 
                    var master = SessionsMasterKeys[sessionID];


                    var keyBlock = this.Decrypter.Prf(master, "key expansion", this.Decrypter.ServerRnd,
                        this.Decrypter.ClientRnd,
                        this.Decrypter.DataDecrypter.MacKeyLength * 2 + this.Decrypter.DataDecrypter.KeyLength * 2 +
                        this.Decrypter.DataDecrypter.IvLength * 2, l7Data[2]);

                    this.Decrypter.ClientHMacKey = new Byte[this.Decrypter.DataDecrypter.MacKeyLength];
                    this.Decrypter.ServerHMacKey = new Byte[this.Decrypter.DataDecrypter.MacKeyLength];

                    Array.Copy(keyBlock, 0, this.Decrypter.ClientHMacKey, 0, this.Decrypter.DataDecrypter.MacKeyLength);
                    Array.Copy(keyBlock, this.Decrypter.DataDecrypter.MacKeyLength, this.Decrypter.ServerHMacKey, 0,
                        this.Decrypter.DataDecrypter.MacKeyLength);

                    // Only part of key_block is used to encrypt data
                    this.Decrypter.DataDecrypter.Init(keyBlock);

                    var changeCipherSpecLength = 6;
                    if (serverHelloLength + changeCipherSpecLength < l7Data.Length)
                    {
                        var finishedOffset = serverHelloLength + changeCipherSpecLength;
                        var finished = new Byte[l7Data.Length - finishedOffset];
                        Array.Copy(l7Data, finishedOffset, finished, 0, finished.Length);
                        this.Decrypter.DoDecryption(finished, pdu, ref this._decryptedBytes);
                        this._decryptedBytes = null;
                    }

                    this._state = SslSessionState.NegotiationChangeCipherSpec;
                }
                // Otherwise, new session is made
                else
                {
                    this._state = SslSessionState.Negotiation;
                }
            }
            else if (l7Data[0] == Handshake && l7Data[1] == Ssl3VerMajor && l7Data[5] == ClientHello)
            {
                // client hello - should be found before server hello message
                this.Decrypter.ClientRnd = new Byte[32];
                Array.Copy(l7Data, 11, this.Decrypter.ClientRnd, 0, this.Decrypter.ClientRnd.Length);
            }
        }

        /// <summary> Moves the given index PDU.</summary>
        /// <param name="indexPDU"> [in,out] The index PDU. </param>
        /// <returns> A bool.</returns>
        private delegate Boolean Move(ref Int32 indexPDU);

        /// <summary> The next.</summary>
        private readonly Move _next;

        private readonly Move _previous;

        private Boolean MovePrevious()
        {
            if (this._decryptedDataCnt <= 0)
            {
                return false;
            }

            this.DataList[this._decryptedDataCnt].DataOffset = 0;
            this._lastPDU = false;
            return this._previous(ref this._decryptedDataCnt);
        }

        private Boolean MoveNext()
        {
            if (this._lastPDU)
            {
                return false;
            }

            var readedPDUbytes = this.Current.DataLength;
            this.Current.DataOffset = this.Current.DataLength;

            this._lastPDU = !this._next(ref this._decryptedDataCnt);
            if (this._lastPDU)
            {
                return false;
            }

            this.TotalReadedPDUbytes += readedPDUbytes;
            this.Current.DataOffset = 0;
            return true;
        }

        /// <summary>
        ///     Override in derivated classes to reached differant functionality in serving PDUs to stream.
        /// </summary>
        /// <param name="indexPDU"> [in,out] The index PDU. </param>
        /// <returns> true if it succeeds, false if it fails.</returns>
        private Boolean NextMixed(ref Int32 indexPDU)
        {
            Debug.Assert(indexPDU >= 0);
            if (indexPDU >= this.DataList.Count - 1) return false;

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
            while (indexpdu != this.DataList.Count - 1)
            {
                indexpdu++;
                if (this.DataList.ElementAt(0).PDU.FlowDirection ==
                    this.DataList.ElementAt(indexpdu).PDU.FlowDirection) return true;
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
                if (this.DataList.ElementAt(0).PDU.FlowDirection == this.DataList.ElementAt(indexpdu).PDU.FlowDirection)
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
            while (indexpdu != this.DataList.Count - 1)
            {
                indexpdu++;
                if (this.DataList.ElementAt(0).PDU.FlowDirection ==
                    this.DataList.ElementAt(indexpdu).PDU.FlowDirection) return true;
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
                if (this.DataList.ElementAt(0).PDU.FlowDirection == this.DataList.ElementAt(indexpdu).PDU.FlowDirection)
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

        public new Boolean EndOfStream { get; private set; }

        #endregion
    }
}