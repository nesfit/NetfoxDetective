using System;
using System.Collections.Generic;
using Netfox.Core.Database.Wrappers;

namespace Netfox.Core.Interfaces.Model.Exports
{
    public interface ICall : IExportBase
    {
        // who is calling
        string From { get; }

        // who is being called
        string To { get; }

        // when did the call start
        DateTime? Start { get; }

        // when did the call end
        DateTime? End { get; }

        // how long did the call take
        TimeSpan? Duration { get; }
        IEnumerable<string> PossibleCodecs { get; }

        IEnumerable<IPEndPointEF> CallStreamAddresses { get; }
        //IPEndPoint SourceEndPoint { get; }
        //IPEndPoint DestinationEndPoint { get; }

        IList<ICallStream> CallStreams { get; }
        IList<ICallStream> PossibleCallStreams { get; }
    }
}