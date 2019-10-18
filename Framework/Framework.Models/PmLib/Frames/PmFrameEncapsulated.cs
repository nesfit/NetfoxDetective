// Copyright (c) 2017 Martin Vondracek
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
using System.Collections.Immutable;
using System.Linq;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    /// <summary>
    /// Frame encapsulated in one or more carrier datagrams.
    /// Carrier datagrams can be either base band frames or encapsulation packets.
    /// <para><b>Current implementation uses copied data instead of offsets in capture file.</b></para>
    /// </summary>
    public class PmFrameEncapsulated : PmFrameBase
    {
        /// <summary>Fragments which form this fragmented frame.</summary>
        public readonly ImmutableList<IFragment> Fragments;  // NOTE: Current implementation uses copied data instead of offsets in capture file.

        private PmFrameEncapsulated(
            PmCaptureBase pmCapture,
            PmLinkType pmLinkType,
            DateTime timeStamp,
            long frameIndex,
            long incLength,
            long originalLength,
            ImmutableList<IFragment> fragments) : base(pmCapture, pmLinkType, timeStamp, incLength, PmFrameType.Encapsulated, frameIndex, originalLength)
        {
            this.Fragments = fragments ?? throw new ArgumentNullException(nameof(fragments));
            // copy data from fragments to array in memory
            // NOTE: Current implementation uses copied data instead of offsets in capture file.
            var data = new List<Byte>();
            foreach(var fragment in fragments) { data.AddRange(fragment.Payload); }

            this.L2DataEncapsulated = data.ToArray();
        }

        public PmFrameEncapsulated() { }

        /// <summary>
        /// Factory method to create <see cref="PmFrameEncapsulated"/> from provided <see cref="IFragment"/>s.
        /// </summary>
        /// <returns></returns>
        public static PmFrameEncapsulated Create(IEnumerable<IFragment> fragments)
        {
            if(fragments == null) { throw new ArgumentNullException(nameof(fragments)); }

            var fs = ImmutableList.CreateRange(fragments);

            // NOTE: Current implementation uses copied data instead of offsets in capture file.
            var incLength = fs.Sum(fragment => fragment.Payload.Length);
            // "IncludedLength: Length of frame in bytes that is actually included, might be lesser than OriginalLength when
            // snapping have been used during capture" from PmFrameBase
            var originalLength = incLength;

            var f = fs.Last();
            return new PmFrameEncapsulated(f.LastFrame.PmCapture, PmLinkType.Raw, f.LastFrame.TimeStamp, f.LastFrame.FrameIndex, incLength, originalLength, fs);
        }
    }
}
