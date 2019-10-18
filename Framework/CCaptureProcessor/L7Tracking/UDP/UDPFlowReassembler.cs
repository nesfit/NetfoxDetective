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
using System.Linq;
using Netfox.Core.Enums;
using Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections;
using Netfox.Framework.Models;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.UDP
{
    public class UDPFlowReassembler
    {
        public UDPFlowReassembler(L4Conversation l4Conversation, FlowStore flowStore, DaRFlowDirection flowDirection, TimeSpan udpSessionAliveTimeout)
        {
            this.L4Conversation = l4Conversation;
            this.FlowStore = flowStore;
            this.FlowDirection = flowDirection;
            this.UdpSessionAliveTimeout = udpSessionAliveTimeout;
        }

        public FlowStore FlowStore { get; }
        public DaRFlowDirection FlowDirection { get; }
        private L4Conversation L4Conversation { get; }
        private TimeSpan UdpSessionAliveTimeout { get; }

        public void ProcessUDPFlow(FramesFirstSeenSortedCollection udpFlowFrames)
        {
            var udpFlow = this.FlowStore.CreateAndAddFlow(this.FlowDirection);

            if(!udpFlowFrames.Any()) { return; }

            foreach(var frame in udpFlowFrames.Values) //.Where(frame => frame.L7PayloadLength > 0)
            {
                if(frame.L7PayloadLength <= 0)
                {
                    udpFlow.NonDataFrames.Add(frame);
                    continue;
                }
                var l7PDU = udpFlow.CreateL7PDU();
                // var currentPDU = new L7PDU(bidirectionalFlow, frame.TimeStamp, frame.L7PayloadLength, flowDirection);

                //In case that frame is IPv4 fragmented
                if(!frame.Ipv4FMf)
                {
                    l7PDU.AddFrame(frame);
                    l7PDU.ExtractedBytes = frame.L7PayloadLength;
                    continue;
                }

                Int64 extractedBytes;

                var defragmentedFrames = this.FindFragments(frame, out extractedBytes);
                if(defragmentedFrames == null)
                {
                    continue;
                }
                l7PDU.AddFrameRange(defragmentedFrames);
                l7PDU.ExtractedBytes = extractedBytes;
            }
        }

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
            var expectedOffset = (Int64) Math.Ceiling((firstFrame.IncludedLength - (firstFrame.L7Offset - firstFrame.L2Offset)) / (Double) 8) + 1;
            while(true)
            {
                var currentFrameList = list.Where(frame => frame.Ipv4FragmentOffset == expectedOffset);
                if(!currentFrameList.Any()) { return null; }
                var bestFrame =
                    currentFrameList.Aggregate(
                        (curmin, x) =>
                            (curmin == null || Math.Abs((x.TimeStamp - firstFrame.TimeStamp).TotalMilliseconds) < (curmin.TimeStamp - epoch).TotalMilliseconds? x : curmin));
                if(Math.Abs((bestFrame.TimeStamp - firstFrame.TimeStamp).TotalMilliseconds) > limit) { return null; }
                fragmentList.Add(bestFrame);

                if(bestFrame.Ipv4FMf == false)
                {
                    extractedBytes = expectedOffset * 8 + bestFrame.L7PayloadLength - 8; //8 - UDP header size
                    break;
                }

                expectedOffset += (Int64) Math.Ceiling(bestFrame.L7PayloadLength / (Double) 8);
            }

            //foreach(var frame in fragmentList) { this.L4Conversation.L3Conversation.NonL4Frames.Remove(frame); } //todo! release references to already used frames


            return fragmentList;
        }
    }
}