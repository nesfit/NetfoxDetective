using System;
using Netfox.Core.Database;
using Netfox.Core.Enums;

namespace Netfox.Framework.Models.Interfaces
{
    public interface ILxConversationStatistics : IEntity
    {
        /// <summary> Gets or sets the bytes.</summary>
        /// <value> The bytes.</value>
        Int64 Bytes { get; }

        /// <summary> Gets or sets the Date/Time of the first seen.</summary>
        /// <value> The first seen.</value>
        // DateTime FirstSeen { get;  } //note implemented in IEntity

        /// <summary> Gets or sets the frames.</summary>
        /// <value> The frames.</value>
        Int64 Frames { get; }

        /// <summary> Gets or sets the Date/Time of the last seen.</summary>
        /// <value> The last seen.</value>
        DateTime LastSeen { get; }

        /// <summary> Gets or sets the malformed frames.</summary>
        /// <value> The malformed frames.</value>
        Int64 MalformedFrames { get; }

        TimeSpan Duration { get; }

        /// <summary> The flow direction.</summary>
        DaRFlowDirection FlowDirection { get; }
    }
}