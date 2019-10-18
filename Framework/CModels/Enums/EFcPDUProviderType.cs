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
    public enum EfcPDUProviderType
    {
        /// <summary>
        ///     PDUProviderMixed provides PDUs ordered by TIMESTAMP of first packet arrival to the client.
        ///     When Application message is spread to more PDUs, then next PDU is selected by TIMESTAMP so it
        ///     can be from same or other flow direction
        /// </summary>
        Mixed,

        /// <summary>
        ///     PDUProviderBreakedInterlay provides PDUs ordered by TIMESTAMP of first packet arrival to the
        ///     client. When Application message is spread to more PDUs, then next PDU is selected from the
        ///     SAME FLOW as the first PDU, but if next PDU in row is not with same flowDirection, update
        ///     fails. Same interpetation as Wireshark`s follow TCP stream.
        /// </summary>
        Breaked,

        /// <summary>
        ///     PDUProviderContinuingInterlay provides PDUs ordered by TIMESTAMP of first packet arrival to
        ///     the client. When Application message is spread to more PDUs, then next PDU is selected from
        ///     the SAME FLOW as the first PDU
        ///     Same interpetation as Wireshark`s follow TCP stream.
        /// </summary>
        ContinueInterlay,

        /// <summary>
        ///     PDUProviderSingleMessage provides ony one PDU ordered by TIMESTAMP of first packet arrival to
        ///     the client.
        /// </summary>
        SingleMessage
    }
}