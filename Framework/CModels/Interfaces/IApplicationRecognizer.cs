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
using Castle.Windsor;
using Netfox.NBARDatabase;

namespace Netfox.Framework.Models.Interfaces
{
    public interface IApplicationRecognizer
    {
        String Name { get; }
        String Description { get; }
        UInt32 Priority { get; }
        String Type { get; }
        WindsorContainer ControllerCaptureProcessorWindsorContainer { get; set; }

        /// <summary>
        ///     This method is called after all conversations are tracked. You can access to metadata about
        ///     conversations providing basic information about BidirectionalFlow or if need be you may
        ///     access frames. RealFrames are stored only as frame numbers which serves as their identifier
        ///     in PCAP. To access frames use IBtBidirectionalFlowValue`s GetFrames() or
        ///     Get{Up|Down}FlowsFrames()
        ///     You can also access to raw reassembled and IPv4 defragmented data stream using
        ///     IBtBidirectionalFlowValue`s PreparePDUs()
        ///     and then you can access L7PDUs, {up|down}FlowPDUs, but pleas keep in mind that PreparePDUs()
        ///     is little bit time consuming so if you do not need it, do not use it.
        /// </summary>
        /// <param name="conversation"> The conversation. </param>
        /// <param name="conversationStore"></param>
        /// <summary>
        ///     Gets an Array of NBAR2TaxonomyProtocol containing given protocol name
        /// </summary>
        /// <param name="protocolName"></param>
        /// <param name="defaultTCPPort"></param>
        /// <returns>Array of NBAR2TaxonomyProtocol containing given protocol name</returns>
        NBAR2TaxonomyProtocol GetNbar2TaxonomyProtocol(String protocolName);

        IEnumerable<L7Conversation> RecognizeAndUpdateConversation(L7Conversation conversation);
    }
}