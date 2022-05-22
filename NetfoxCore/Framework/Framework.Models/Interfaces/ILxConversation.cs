using System;
using System.Collections.Generic;
using Netfox.Core.Database;
using Netfox.Core.Interfaces.Model.Exports;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.NBARDatabase;
using PacketDotNet;

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