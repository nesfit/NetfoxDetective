using System;
using System.Collections.Generic;
using Netfox.Framework.Models.PmLib.Frames;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections
{
    internal class FramesFirstSeenSortedCollection : SortedDictionary<DateTime, PmFrameBase>
    {
        public FramesFirstSeenSortedCollection() : base(new DuplicateKeyComparer<DateTime>())
        {
        }

        public DateTime? LastSeen { get; private set; }

        public void Add(PmFrameBase frame)
        {
            if (this.LastSeen < frame.TimeStamp)
            {
                this.LastSeen = frame.TimeStamp;
            }

            this.Add(frame.TimeStamp, frame);
        }
    }
}