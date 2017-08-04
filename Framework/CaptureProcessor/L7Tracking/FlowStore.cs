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
using Netfox.Framework.Models.Interfaces;

namespace Netfox.Framework.CaptureProcessor.L7Tracking
{
    internal class FlowStore
    {
        /// <summary> The down flows.</summary>
        private readonly LinkedIterableList<FsUnidirectionalFlow> _downFlows = new LinkedIterableList<FsUnidirectionalFlow>();

        /// <summary> The up flows.</summary>
        private readonly LinkedIterableList<FsUnidirectionalFlow> _upFlows = new LinkedIterableList<FsUnidirectionalFlow>();

        public FlowStore(IL7ConversationFactory l7ConversationFactory, IApplicationRecognizer applicationRecognizer, L4Conversation l4Conversation, TimeSpan flowMatchingTimeSpan)
        {
            this.L7ConversationFactory = l7ConversationFactory;
            this.ApplicationRecognizer = applicationRecognizer;
            this.L4Conversation = l4Conversation;
            this.FlowMatchingTimeSpan = flowMatchingTimeSpan;
        }

        public IL7ConversationFactory L7ConversationFactory { get; }
        public IApplicationRecognizer ApplicationRecognizer { get; }
        public L4Conversation L4Conversation { get; }
        public TimeSpan FlowMatchingTimeSpan { get; }
        
        public FsUnidirectionalFlow CreateAndAddFlow(DaRFlowDirection flowDirection)
        {
            lock(this)
            {
                var flow = new FsUnidirectionalFlow(this.L4Conversation, flowDirection);
                switch(flowDirection)
                {
                    case DaRFlowDirection.up:
                        this._upFlows.AddLast(flow);
                        break;
                    case DaRFlowDirection.down:
                        this._downFlows.AddLast(flow);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(flowDirection));
                }
                return flow;
            }
        }

        public List<L7Conversation> PairFlowsAndCreateAndAddConversations(bool createUnidirectional = false)
        {
            lock(this)
            {
                var l7Conversations = new List<L7Conversation>();
                using(var upFlowEnum = this._upFlows.GetEnumerator()) //match by FlowIdentifier
                {
                    while(upFlowEnum.MoveNext())
                    {
                        var upFlow = upFlowEnum.Current;
                        var downFlow = this._downFlows.FirstOrDefault(flow => flow.FlowIdentifier == upFlow.FlowIdentifier); //there can be only one
                        // create a conversation with specific upFlow and downFlow

                        if(downFlow == null) continue;
                        
                        l7Conversations.Add(this.L7ConversationFactory.Create(upFlow, downFlow));

                        upFlowEnum.RemoveCurrent();
                        this._downFlows.Remove(downFlow);
                    }
                }

                using(var upFlowEnum = this._upFlows.GetEnumerator()) //Match by time window
                {
                    while(upFlowEnum.MoveNext())
                    {
                        var upFlow = upFlowEnum.Current;
                        var downFlow =
                            this._downFlows.Where(flow => flow.FirstSeen.Subtract(upFlow.FirstSeen).Duration() < this.FlowMatchingTimeSpan)
                                .OrderBy(flow => flow.FirstSeen)
                                .FirstOrDefault(); //there can be only one
                        // create a conversation with specific upFlow and downFlow

                        if(downFlow == null) continue;
                        l7Conversations.Add(this.L7ConversationFactory.Create(upFlow, downFlow));

                        upFlowEnum.RemoveCurrent();
                        this._downFlows.Remove(downFlow);
                    }
                }

                if(createUnidirectional) //unidirectional conversations
                {
                    using (var flow = this._upFlows.GetEnumerator())
                    {
                        while (flow.MoveNext()) { l7Conversations.Add(this.L7ConversationFactory.Create(flow.Current, DaRFlowDirection.up)); }
                    }
                
                using (var flow = this._downFlows.GetEnumerator())
                    {
                        while(flow.MoveNext()) { l7Conversations.Add(this.L7ConversationFactory.Create(flow.Current,DaRFlowDirection.down)); }
                    }
                }

                lock(debugLock)
                {
                    var l7ConversationsRecognized = new List<L7Conversation>();
                    foreach(var l7Conversation in l7Conversations)
                    {
                        l7ConversationsRecognized.AddRange(this.ApplicationRecognizer.RecognizeAndUpdateConversation(l7Conversation));
                        
                    }
                    return l7ConversationsRecognized;
                }
            }
        }

        private static object debugLock = new object();

        public void RemoveFlow(FsUnidirectionalFlow flow)
        {
            switch(flow.FlowDirection)
            {
                case DaRFlowDirection.up:
                    this._upFlows.Remove(flow);
                    break;
                case DaRFlowDirection.down:
                    this._downFlows.Remove(flow);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flow));
            }
        }
    }
}