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
using Netfox.Framework.Models;
using Netfox.Framework.Models.Services;
using Netfox.Nbar;
using Netfox.NBARDatabase;
using Netfox.RTP;

namespace Netfox.Framework.CaptureProcessor.ApplicationRecognizer
{
    /// <summary>
    ///     Abstract Application recognizer class encapsulates InbarProtocolPortDatabase which has to be
    ///     alwayes provided to help ConversationTracker determine select which communication participant
    ///     is client and which is server.
    ///     Please inherit this class and follow example provided by ApplicationRecognizerPortBased.
    /// </summary>
    public class ApplicationRecognizerDefault : ApplicationRecognizerBase
    {
        public ApplicationRecognizerNBAR ApplicationRecognizerNBAR { get; }
        public ApplicationRecognizerRTP ApplicationRecognizerRTP { get; }

        /// <summary>
        ///     Encapsulates basic Protocol  to Port mapping and manipulation Instance is shared through one
        ///     investigation so changes is global to all If some changes like to add well-known protocol to
        ///     port mapping are required, please feel free to update it in code of InbarProtocolPortDatabase If
        ///     change is localy significant it is required to do it in the constructor of
        ///     ApplicationRecognizer which is called before ConversationTracker it self.
        /// </summary>

        public ApplicationRecognizerDefault(NBARProtocolPortDatabase nbarProtocolPortDatabase, ApplicationRecognizerNBAR applicationRecognizerNBAR, ApplicationRecognizerRTP applicationRecognizerRTP) : base(nbarProtocolPortDatabase)
        {
            this.ApplicationRecognizerNBAR = applicationRecognizerNBAR;
            this.ApplicationRecognizerRTP = applicationRecognizerRTP;
        }

        public override String Name => @"Default";

        public override String Description => @"Used as a default application recognizer, using a combination of NBAR and RTP recognizers.";

        public override UInt32 Priority => 1;

        public override String Type => "Port, DPI combination";

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
        public override IEnumerable<L7Conversation> RecognizeAndUpdateConversation(L7Conversation conversation)
        {
            var recognizeAndUpdateConversation = this.ApplicationRecognizerNBAR.RecognizeAndUpdateConversation(conversation).ToArray();
            if ((recognizeAndUpdateConversation.FirstOrDefault()?.ApplicationProtocols.Any()).GetValueOrDefault()
                && !conversation.ApplicationProtocols.Contains(this.ApplicationRecognizerRTP.NbarRtpTaxonomy.First())) { return recognizeAndUpdateConversation; }
            return this.ApplicationRecognizerRTP.RecognizeAndUpdateConversation(conversation);
        }

        public override IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation) => this.ApplicationRecognizerNBAR.RecognizeConversation(conversation)
                                                                                                                   ?? this.ApplicationRecognizerRTP.RecognizeConversation(conversation);
    }
}