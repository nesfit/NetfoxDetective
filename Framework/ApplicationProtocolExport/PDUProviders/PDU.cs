// Copyright (c) 2017 Jan Pluskal, Miroslav Slivka, Viliam Letavay
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
using Netfox.Framework.Models;

namespace Netfox.Framework.ApplicationProtocolExport.PDUProviders
{
    /// <summary>
    ///     A pdu.
    /// </summary>
    /// <remarks>
    ///     Pluskal, 2/10/2014.
    /// </remarks>
    public class PDU
    {
        private Byte[] _bytes;

        /// <summary>
        ///     Gets or sets the PDU.
        /// </summary>
        /// <value>
        ///     The PDU.
        /// </value>
        public L7PDU Pdu { get; set; }

        /// <summary>
        ///     Gets the bytes.
        /// </summary>
        /// <value>
        ///     The bytes contained in PDU.
        /// </value>
        public Byte[] Bytes => this._bytes ?? (this._bytes = this.Pdu.PDUByteArr);

        /// <summary>
        ///     Gets the length.
        /// </summary>
        /// <value>
        ///     The length of PDU data.
        /// </value>
        public Int32 Length => this.Bytes.Length;

        /// <summary>
        ///     Gets or sets the offset.
        /// </summary>
        /// <value>
        ///     The offset point to PDU data.
        /// </value>
        public Int32 Offset { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this object is all readed.
        /// </summary>
        /// <value>
        ///     true if this object is all readed, false if not.
        /// </value>
        public Boolean IsAllRead => this.Length == this.Offset;

        /// <summary>
        ///     Gets the remaining bytes.
        /// </summary>
        /// <value>
        ///     The remains bytes.
        /// </value>
        public Int32 RemainingBytes => this.Length - this.Offset;
    }
}