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

using System.Collections.Generic;
using System.Linq;
using Netfox.Framework.Models.PmLib.Frames;
using PacketDotNet;

namespace Netfox.Framework.CaptureProcessor.L7Tracking.CustomCollections
{
    /// <summary> Collection of da r frames.</summary>
    internal class DaRFrameCollection : LinkedIterableList<PmFrameBase>
    {
        /// <summary> Constructor.</summary>
        /// <param name="collection"> The collection. </param>
        public DaRFrameCollection(IEnumerable<PmFrameBase> pmFrames)
        {
            //var pmFrames = collection as PmFrameBase[] ?? collection.ToArray();
            if(!pmFrames.Any()) { return; }
            switch(pmFrames.First().IpProtocol)
            {
                case IPProtocolType.TCP:
                    this.InitCollection(pmFrames.OrderBy(frame => frame.TcpSequenceNumber).ThenBy(frame => frame.L7PayloadLength));
                    break;
                default:
                    this.InitCollection(pmFrames);
                    break;
            }
        }

        /// <summary> Initialises the collection.</summary>
        /// <param name="collection"> The collection. </param>
        private void InitCollection(IEnumerable<PmFrameBase> collection)
        {
            foreach(var pmFrame in collection) { this.AddLast(pmFrame); }
        }
    }
}