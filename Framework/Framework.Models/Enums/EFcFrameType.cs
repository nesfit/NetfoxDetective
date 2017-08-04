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

namespace Netfox.Framework.Models.Enums
{
    /// <summary> More general frame type.</summary>
    public enum EFcFrameType
    {
        /// <summary>
        ///     Frame will be parsed as LibPCAP
        /// </summary>
        Pcap = 1,

        /// <summary>
        ///     Frame will be parsed as PCAPng
        /// </summary>
        PcapNg = 2,

        /// <summary>
        ///     Frame will be parsed as Microsoft Network Monitor
        /// </summary>
        Mnm = 3,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content all zeros for DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirtualBlank = 4,

        /// <summary>
        ///     Physically non-existing frame in PCAP file, just as stuffing with content of predefined noise DaR. Not intended for
        ///     parsing!
        /// </summary>
        VirutalNoise = 5
    }
}