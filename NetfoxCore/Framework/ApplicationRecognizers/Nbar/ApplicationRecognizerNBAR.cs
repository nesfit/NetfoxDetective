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
using Netfox.Framework.Models;
using Netfox.Framework.Models.Services;
using Netfox.NBARDatabase;
using PacketDotNet;

namespace Netfox.Nbar
{
    public class ApplicationRecognizerNBAR : ApplicationRecognizerBase
    {
        private static readonly IReadOnlyList<NBAR2TaxonomyProtocol> emptyNbar2TaxonomyProtocols = new List<NBAR2TaxonomyProtocol>();
       
        public ApplicationRecognizerNBAR(NBARProtocolPortDatabase nbarProtocolPortDatabase):base(nbarProtocolPortDatabase) { }

        public override String Name => @"NBAR";

        public override String Description => @"Using a large protocol taxonomy database, currently recognizes by a protocol's known ports.";

        public override UInt32 Priority => 2;

        public override String Type => "Ports";
        
        public override IReadOnlyList<NBAR2TaxonomyProtocol> RecognizeConversation(L7Conversation conversation)
        {
            var minPort = (UInt32) Math.Min(conversation.DestinationEndPoint.Port, conversation.SourceEndPoint.Port);
            var maxPort = (UInt32) Math.Max(conversation.DestinationEndPoint.Port, conversation.SourceEndPoint.Port);
            switch(conversation.L4ProtocolType)
            {
                case IPProtocolType.TCP:
                    if(this.NBARProtocolPortDatabase.TCPProtocolsAndPorts.ContainsKey(minPort)) { return this.NBARProtocolPortDatabase.TCPProtocolsAndPorts[minPort].Item2; }
                    if(this.NBARProtocolPortDatabase.TCPProtocolsAndPorts.ContainsKey(maxPort)) { return this.NBARProtocolPortDatabase.TCPProtocolsAndPorts[maxPort].Item2; }
                    return emptyNbar2TaxonomyProtocols;

                case IPProtocolType.UDP:
                    if(this.NBARProtocolPortDatabase.UDPProtocolsAndPorts.ContainsKey(minPort)) { return this.NBARProtocolPortDatabase.UDPProtocolsAndPorts[minPort].Item2; }
                    if(this.NBARProtocolPortDatabase.UDPProtocolsAndPorts.ContainsKey(maxPort)) { return this.NBARProtocolPortDatabase.UDPProtocolsAndPorts[maxPort].Item2; }
                    return emptyNbar2TaxonomyProtocols;

                default:
                    return emptyNbar2TaxonomyProtocols;
            }
        }
    }
}