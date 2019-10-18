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

namespace Netfox.Framework.Models.PmLib.Frames
{
    /// <summary>
    /// Fragment of a fragmented datagram. 
    /// <para><b>Current implementation uses copied data instead of offsets in capture file.</b></para>
    /// </summary>
    public interface IFragment
    {
        /// <summary>
        /// Offset of payload in this fragment in capture file, measured in bytes.
        /// </summary>
        //long PayloadOffset { get; } // NOTE: Current implementation uses copied data instead of offsets in capture file.

        /// <summary>
        /// Length of payload in this fragment, measured in bytes.
        /// </summary>
        //long PayloadLength { get; } // NOTE: Current implementation uses copied data instead of offsets in capture file.

        byte[] Payload { get; } // NOTE: Current implementation uses copied data instead of offsets in capture file.
        
        PmFrameBase LastFrame { get; }
    }
}