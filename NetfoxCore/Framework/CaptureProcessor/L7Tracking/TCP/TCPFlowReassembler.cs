using System;
using System.Collections.Generic;
using System.Linq;
using Netfox.Core.Enums;
using Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.TCP
{
    internal class TCPFlowReassembler
    {
        public TCPFlowReassembler(L4Conversation l4Conversation, FlowStore flowStore, TimeSpan tcpSessionAliveTimeout,
            long tcpSessionMaxDataLooseOnTCPLoop)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStore;
            this.TCPSessionAliveTimeout = tcpSessionAliveTimeout;
            this.TCPSessionMaxDataLooseOnTCPLoop = tcpSessionMaxDataLooseOnTCPLoop;
        }

        public L4Conversation L4Conversation { get; }
        public FlowStore FlowStore { get; }

        public uint ExpectedSequenceNumber { get; set; }
        private FsUnidirectionalFlow CurrentTCPFlow { get; set; }
        private LinkedIterableList<PmFrameBase>.Enumerator TCPFlowEnumerator { get; set; }

        private DaRFlowDirection FlowDirection { get; set; }

        private DaRFrameCollection TCPSynOrderedFrames { get; set; }

        private L7PDU CurrentPDU { get; set; }

        private DateTime LastTimestamp { get; set; }

        private PmFrameBase CurrentFrame { get; set; }

        private TimeSpan TCPSessionAliveTimeout { get; }

        private long TCPSessionMaxDataLooseOnTCPLoop { get; }

        public void ProcessTCPSession(FramesSequenceNumberSortedCollection tcpFlow, DaRFlowDirection flowDirection)
        {
            this.TCPSynOrderedFrames = new DaRFrameCollection(tcpFlow.Values);
            this.FlowDirection = flowDirection;
            foreach (var synFrame in this.TCPSynOrderedFrames)
            {
                this.ProcessTcpSession(synFrame);
            }

            while (this.TCPSynOrderedFrames.Any())
            {
                this.ProcessTcpSession(this.TCPSynOrderedFrames.First.Value);
            }
        }

        /// <summary> Adds a non data frame.</summary>
        private void AddNonDataFrame() => this.CurrentTCPFlow.NonDataFrames.Add(this.CurrentFrame);

        /// <summary> Adds a non data frame.</summary>
        /// <param name="frame"> The frame. </param>
        private void AddNonDataFrame(PmFrameBase frame) => this.CurrentTCPFlow.NonDataFrames.Add(frame);

        private IEnumerable<PmFrameBase> FindFragments(PmFrameBase firstFrame, out Int64 extractedBytes)
        {
            // _log.Info(String.Format("Located IPv4 fragmented frame {0}", firstFrame));

            extractedBytes = 0;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var limit = 2 * 60 * 1000; // max difference in timestamps of two IPv4 fragments

            //Select framew with the same Ipv4FIdentification
            var list = from frame in this.L4Conversation.L3Conversation.NonL4Frames
                //CaptureProcessor.CaptureProcessor.GetRealFramesEnumerable()
                where frame.Ipv4FIdentification == firstFrame.Ipv4FIdentification
                orderby frame.Ipv4FragmentOffset
                select frame;

            //IOrderedEnumerable<PmFrameBase> list = from frame in _currentFlow.RealFrames
            //                                    where frame.Ipv4FIdentification == firstFrame.Ipv4FIdentification
            //                                    orderby frame.Ipv4FragmentOffset
            //                                    select frame;

            //List of ordered fragments
            var fragmentList = new List<PmFrameBase>
            {
                firstFrame
            };

            //Process of defragmentation
            var expectedOffset =
                (Int64) Math.Ceiling((firstFrame.IncludedLength - (firstFrame.L7Offset - firstFrame.L2Offset)) /
                                     (Double) 8) + 1;
            while (true)
            {
                var currentFrameList = list.Where(frame => frame.Ipv4FragmentOffset == expectedOffset);
                if (!currentFrameList.Any())
                {
                    return null;
                }

                var bestFrame =
                    currentFrameList.Aggregate(
                        (curmin, x) =>
                            (curmin == null || Math.Abs((x.TimeStamp - firstFrame.TimeStamp).TotalMilliseconds) <
                                (curmin.TimeStamp - epoch).TotalMilliseconds
                                    ? x
                                    : curmin));
                if (Math.Abs((bestFrame.TimeStamp - firstFrame.TimeStamp).TotalMilliseconds) > limit)
                {
                    return null;
                }

                fragmentList.Add(bestFrame);

                if (bestFrame.Ipv4FMf == false)
                {
                    extractedBytes = expectedOffset * 8 + bestFrame.L7PayloadLength - 8; //8 - UDP header size
                    break;
                }

                expectedOffset += (Int64) Math.Ceiling(bestFrame.L7PayloadLength / (Double) 8);
            }

            //foreach(var frame in fragmentList) { this.L4Conversation.L3Conversation.NonL4Frames.Remove(frame); } //todo! release references to already used frames


            return fragmentList;
        } // TODO accepted: Ducplicit code with UDPFLowReassembler.

        private void FiNorRstProcess()
        {
            var finorRstSeq = this.CurrentFrame.TcpSequenceNumber;
            UInt32 nextAck;
            UInt32
                spuriousRetransmissions; //http://blog.packet-foo.com/2013/06/spurious-retransmissions/ example icq1.cap, frame 1056 
            if (this.CurrentFrame.L7PayloadLength <= 0)
            {
                this.AddNonDataFrame();
                nextAck = ((UInt32) this.CurrentFrame.TcpSequenceNumber) + 1;
                spuriousRetransmissions = nextAck; //cannot occur;
            }
            else
            {
                nextAck = ((UInt32) this.CurrentFrame.TcpSequenceNumber) +
                          ((UInt32) this.CurrentFrame.L7PayloadLength) + 1;
                spuriousRetransmissions = ((UInt32) this.CurrentFrame.TcpSequenceNumber) +
                                          ((UInt32) this.CurrentFrame.L7PayloadLength);
            }

            this.NextFrame();
            //to consume possible rest of communication ACKs and/or retransmits 
            while (this.CurrentFrame != null
                   && (this.CurrentFrame.TcpSequenceNumber == finorRstSeq || this.CurrentFrame.TcpSequenceNumber ==
                                                                          nextAck
                                                                          || this.CurrentFrame.TcpSequenceNumber ==
                                                                          spuriousRetransmissions))
            {
                this.AddNonDataFrame();
                this.NextFrame();
            }
        }

        private void NextFrame()
        {
            this.LastTimestamp = this.CurrentFrame.TimeStamp;
            this.TCPFlowEnumerator.RemoveCurrent();
            this.TCPFlowEnumerator.MoveNext();
            this.CurrentFrame = this.TCPFlowEnumerator.Current;
        }

        private void ProcessTcpSession(PmFrameBase synFrame)
        {
            var wasTCPSeqLoop = false;
            this.TCPFlowEnumerator = this.TCPSynOrderedFrames.GetEnumerator();

            //find first frame frame of session
            while (this.TCPFlowEnumerator.Current != synFrame)
            {
                if (!this.TCPFlowEnumerator.MoveNext())
                {
                    throw new InvalidOperationException("Init frame does not exists!");
                }
            }

            this.CurrentFrame = this.TCPFlowEnumerator.Current;
            this.CurrentTCPFlow = this.FlowStore.CreateAndAddFlow(this.FlowDirection);
            this.LastTimestamp = this.CurrentFrame.TimeStamp;

            this.CurrentPDU = null;

            //Init flow direction identifier
            if (this.CurrentFrame.TcpFSyn && !this.CurrentFrame.TcpFAck) //SYN packet
            {
                this.CurrentTCPFlow.FlowIdentifier = this.CurrentFrame.TcpSequenceNumber + 1;
            }
            else if (this.CurrentFrame.TcpFSyn && this.CurrentFrame.TcpFAck) //SYN+ACK packet
            {
                this.CurrentTCPFlow.FlowIdentifier = this.CurrentFrame.TcpAcknowledgementNumber;
            }


            if (this.CurrentFrame.TcpFSyn)
            {
                // while in a case that there are more retransmitted frames with SYN flag
                while (this.CurrentFrame != null && this.CurrentFrame.TcpFSyn &&
                       this.CurrentFrame.TcpSequenceNumber == synFrame.TcpSequenceNumber
                       && this.CurrentFrame.TcpAcknowledgementNumber == synFrame.TcpAcknowledgementNumber)
                {
                    this.ExpectedSequenceNumber = (UInt32) this.CurrentFrame.TcpSequenceNumber + 1;
                    this.AddNonDataFrame();
                    this.NextFrame();
                }

                // in a case that there are captured only a SYN frames and not the rest of the session
                if (this.CurrentFrame == null)
                {
                    return;
                }
            }
            else
            {
                this.ExpectedSequenceNumber = (UInt32) this.CurrentFrame.TcpSequenceNumber;
            }

            while (this.TestTCPSessionEnd(ref wasTCPSeqLoop))
            {
                if (this.CurrentFrame.IsMalformed)
                {
                    this.AddNonDataFrame();
                    this.NextFrame();
                    continue;
                }

                if (this.CurrentFrame.TimeStamp.Subtract(this.LastTimestamp).Duration() > this.TCPSessionAliveTimeout)
                {
                    this.SkipFrame();
                    continue;
                }

                if (this.CurrentPDU == null)
                {
                    this.CurrentPDU = this.CurrentTCPFlow.CreateL7PDU();
                }

                var x = this.CurrentFrame.TcpSequenceNumber - this.ExpectedSequenceNumber;

                if (x > 0)
                {
                    this.TCPMissingSegment(x);
                }
                else if (x < 0)
                {
                    //in case that packet is TCPSyn retranssmit
                    //if (_currentFrame.TcpFSyn || _currentFrame.TcpFFin || _currentFrame.TcpFRst)
                    //{
                    //    NextFrame();
                    //    continue;
                    //}


                    // TCP keep live packet, TCP ACK
                    if (this.CurrentFrame.TcpFAck && (this.CurrentFrame.L7PayloadLength == -1 ||
                                                      this.CurrentFrame.L7PayloadLength == 1))
                        //if (x == -1 && _currentFrame.TcpFAck && (_currentFrame.L7PayloadLength == -1 || _currentFrame.L7PayloadLength == 1))
                        // && _currentFrame.L7PayloadLength == -1 //could be also  _currentFrame.L7PayloadLength == 1
                    {
                        this.AddNonDataFrame();
                        this.NextFrame(); //skip keepalive frame
                        continue;
                    }

                    ////TODO find what causes this
                    //if (_currentFrame.L7PayloadLength == -1)
                    //{
                    //    AddNonDataFrame();
                    //    NextFrame(); //skip keepalive frame
                    //    continue;
                    //}


                    //in case of retransmission, TCP Checksum must be computed
                    var parsedFrame = this.CurrentFrame.PmPacket;
                    if (parsedFrame.IsValidChecksum) // frame is valid, let's use it
                    {
                        //end of current TCP session
                        if (this.CurrentFrame.TcpFFin || this.CurrentFrame.TcpFRst)
                        {
                            this.FiNorRstProcess();
                            break;
                        }

                        if (!this.CurrentPDU.FrameList.Any() && // _currentPDU has no frames
                            this.CurrentTCPFlow.PDUs.Any()) // CurrentTCPFlow has some PDUs
                        {
                            this.CurrentPDU = this.CurrentTCPFlow.GetLastPDU();
                        }

                        if (this.CurrentPDU.FrameList.Any()) // _currentPDU has some frames
                        {
                            var lastPduFrame = this.CurrentPDU.FrameList.Last();


                            if (this.CurrentFrame.TcpSequenceNumber == lastPduFrame.TcpSequenceNumber &&
                                lastPduFrame.PmPacket.IsValidChecksum)
                            {
                                this.AddNonDataFrame();
                                //end of current L7PDU
                                if ((this.CurrentFrame.TcpFPsh ||
                                     this.CurrentFrame.L7PayloadLength < this.L4Conversation.L4FlowMTU) &&
                                    this.CurrentPDU.FrameList.Any())
                                {
                                    this.CurrentPDU = null;
                                }

                                this.NextFrame();
                                continue;
                            }

                            //todo SACK http://superuser.com/questions/598574/if-a-tcp-packet-got-partially-acknowledged-how-will-the-retransmission-mechanis //example tcp_retr1.pcapng 7,8,11,12 frames
                            //this bypass and ignores SACK ... assuming that correct packet have been captred and lost occured on the way
                            if (this.CurrentFrame.TcpSequenceNumber > lastPduFrame.TcpSequenceNumber &&
                                x < lastPduFrame.L7PayloadLength && lastPduFrame.PmPacket.IsValidChecksum)
                            {
                                this.AddNonDataFrame();
                                //end of current L7PDU
                                if ((this.CurrentFrame.TcpFPsh ||
                                     this.CurrentFrame.L7PayloadLength < this.L4Conversation.L4FlowMTU) &&
                                    this.CurrentPDU.FrameList.Any())
                                {
                                    this.CurrentPDU = null;
                                }

                                this.NextFrame();
                                continue;
                            }

                            var removedFrame = this.CurrentPDU.RemoveLastFrame();
                            this.AddNonDataFrame(removedFrame);
                        }
                        else // _currentPDU has no frames, get rid of it
                        {
                            this.CurrentTCPFlow.RemoveL7PDU(this.CurrentPDU);
                            this.CurrentPDU = this.CurrentTCPFlow.CreateL7PDU();
                        }
                    }
                    else
                        //if checksum is not valid frame is skipped
                    {
                        this.AddNonDataFrame();
                        //end of current L7PDU
                        if ((this.CurrentFrame.TcpFPsh ||
                             this.CurrentFrame.L7PayloadLength < this.L4Conversation.L4FlowMTU) &&
                            this.CurrentPDU.FrameList.Any())
                        {
                            this.CurrentPDU = null;
                        }

                        this.NextFrame();
                        continue;
                    }
                }
                //else //x == 0{//normal state}

                //L4PDU add frame to list
                if (this.CurrentFrame.L7PayloadLength > 0)
                {
                    if (this.CurrentPDU.LowestTCPSeq == null)
                    {
                        this.CurrentPDU.LowestTCPSeq = this.CurrentFrame.TcpSequenceNumber;
                    }

                    this.CurrentPDU.ExtractedBytes += this.CurrentFrame.L7PayloadLength;
                    this.CurrentPDU.AddFrame(this.CurrentFrame);

                    //If frame is IPv4 fragmented find remaining fragments
                    if (this.CurrentFrame.Ipv4FMf)
                    {
                        Int64 extractedBytes;
                        var defragmentedFrames = this.FindFragments(this.CurrentFrame, out extractedBytes);

                        this.CurrentPDU.RemoveLastFrame();
                        this.CurrentPDU.ExtractedBytes -= this.CurrentFrame.L7PayloadLength;

                        this.CurrentPDU.AddFrameRange(defragmentedFrames);
                        this.CurrentPDU.ExtractedBytes += extractedBytes;
                        this.ExpectedSequenceNumber = (UInt32) (this.CurrentFrame.TcpSequenceNumber + extractedBytes);
                    }
                    else
                    {
                        //Normal state, increment expected sequence number
                        this.ExpectedSequenceNumber =
                            (UInt32) (this.CurrentFrame.TcpSequenceNumber + this.CurrentFrame.L7PayloadLength);
                    }
                }

                //end of current TCP session
                if (this.CurrentFrame.TcpFSyn)
                {
                    break;
                }

                if (this.CurrentFrame.TcpFFin || this.CurrentFrame.TcpFRst)
                {
                    this.FiNorRstProcess();
                    break;
                }

                //end of current L7PDU
                if ((this.CurrentFrame.TcpFPsh || this.CurrentFrame.L7PayloadLength < this.L4Conversation.L4FlowMTU) &&
                    this.CurrentPDU.FrameList.Any())
                {
                    this.CurrentPDU = null;
                }

                if (this.CurrentFrame.L7PayloadLength <= 0)
                {
                    this.AddNonDataFrame();
                }

                this.NextFrame();
            }
        }

        /// <summary> Skip frame.</summary>
        private void SkipFrame()
        {
            this.TCPFlowEnumerator.MoveNext();
            this.CurrentFrame = this.TCPFlowEnumerator.Current;
        }

        /// <summary> TCP missing segment.</summary>
        /// <param name="x"> The x coordinate. </param>
        private void TCPMissingSegment(Int64 x)
        {
            // we missed segment - insert 0 instead of data:
            this.CurrentPDU.MissingBytes += x;
            this.CurrentPDU.MissingFrameSequences++;

            var vfId = new PmFrameVirtualBlank(this.CurrentFrame, x, this.CurrentFrame.TimeStamp.AddTicks(-1));
            while (this.CurrentTCPFlow.VirtualFrames.Contains(vfId))
            {
                vfId.TimeStamp = new DateTime().AddTicks(vfId.TimeStamp.Ticks - 1);
            }

            this.CurrentTCPFlow.VirtualFrames.Add(vfId);
            this.CurrentPDU.AddFrame(vfId);

            this.CurrentPDU.IsContainingCorruptedData = true;
            if (this.CurrentPDU.LowestTCPSeq == null)
            {
                this.CurrentPDU.LowestTCPSeq = this.ExpectedSequenceNumber;
            }
        }

        private Boolean TestTCPSessionEnd(ref Boolean wasTCPSeqLoop)
        {
            //normal state, tcpFlowEnumerator.Current == currentFrame
            if (this.TCPFlowEnumerator.Current != null && !this.TCPFlowEnumerator.Current.TcpFSyn)
            {
                return true;
            }

            if (this.TCPFlowEnumerator.Current != null && this.TCPFlowEnumerator.Current.TcpFSyn)
            {
                return false;
            }

            if (wasTCPSeqLoop)
            {
                return false;
            }

            wasTCPSeqLoop = true;
            //at the end of seq numbers, reset structure 
            this.TCPFlowEnumerator.Reset();
            this.SkipFrame();
            if (this.TCPFlowEnumerator.Current == null || this.TCPFlowEnumerator.Current.TcpFSyn)
            {
                return false;
            }
            //return tcpFlowEnumerator.Current.TcpSequenceNumber - _expectedSequenceNumber < TCP_SESSION_MAX_DATA_LOOSE_ON_TCP_LOOP;

            //looking for exact match, frame that is next in tcp seq number sequence
            while (this.TCPFlowEnumerator.Current != null &&
                   this.TCPFlowEnumerator.Current.TcpSequenceNumber - this.ExpectedSequenceNumber <
                   this.TCPSessionMaxDataLooseOnTCPLoop)
            {
                if (this.TCPFlowEnumerator.Current.TcpSequenceNumber - this.ExpectedSequenceNumber == 0)
                {
                    return true;
                }

                this.SkipFrame();
            }

            //at the end of possible seq numbers, reset structure 
            this.TCPFlowEnumerator.Reset();
            this.SkipFrame();
            //looking for tolerable match, some data are missing but in tolrance of TCP_SESSION_MAX_DATA_LOOSE_ON_TCP_LOOP
            //frame is identify by occurrence in TCP_SESSION_ALIVE_TIMEOUT interval
            while (this.TCPFlowEnumerator.Current != null &&
                   this.TCPFlowEnumerator.Current.TcpSequenceNumber - this.ExpectedSequenceNumber <
                   this.TCPSessionMaxDataLooseOnTCPLoop)
            {
                if (this.CurrentFrame.TimeStamp.Subtract(this.LastTimestamp).Duration() < this.TCPSessionAliveTimeout)
                {
                    return true;
                }

                this.SkipFrame();
            }

            return false;
        }
    }
}