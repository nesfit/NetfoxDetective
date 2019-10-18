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
using Netfox.Core.Database;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.NBARDatabase;
using PacketDotNet;
using UnitOfWork.BaseDataEntity;

namespace Netfox.Framework.Models.Interfaces
{
    public interface ILxConversation : IExportSource, IEntity
    {
        IEnumerable<PmFrameBase> Frames { get; }
        IPProtocolType ProtocolType { get; }
        //DateTime FirstSeen { get; } //Note Implemented in IEntity
        DateTime LastSeen { get; }
        IReadOnlyList<NBAR2TaxonomyProtocol> ApplicationProtocols { get; }
        string Name { get; }
        //IPEndPoint SourceEndPoint { get; } //defined in IExportSource
        //IPEndPoint DestinationEndPoint { get; } //defined in IExportSource

        ILxConversationStatistics UpConversationStatistic { get; }
        ILxConversationStatistics DownConversationStatistic { get; }
        ILxConversationStatistics ConversationStats { get; }

        /// <summary> Gets or sets the up flow frames.</summary>
        /// <value> The up flow frames.</value>
        IEnumerable<PmFrameBase> UpFlowFrames { get; }

        /// <summary> Gets or sets the down flow frames.</summary>
        /// <value> The down flow frames.</value>
        IEnumerable<PmFrameBase> DownFlowFrames { get; }
    }
}
