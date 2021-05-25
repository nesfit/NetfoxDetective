using System;
using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections
{
    internal class FramesSequenceNumberSortedCollection : SortedDictionary<long, PmFrameBase>
    {
        public FramesSequenceNumberSortedCollection() : base(new DuplicateKeyComparer<long>())
        {
        }

        public DateTime? LastSeen { get; private set; }

        public void Add(PmFrameBase frame)
        {
            if (this.LastSeen < frame.TimeStamp)
            {
                this.LastSeen = frame.TimeStamp;
            }

            this.Add(frame.TcpSequenceNumber, frame);
        }
    }
}