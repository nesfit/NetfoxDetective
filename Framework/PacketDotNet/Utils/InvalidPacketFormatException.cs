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

namespace PacketDotNet.Utils
{
    /// <summary>
    /// Represents error when packet could not be parsed due to invalid format.
    /// <para>Invalid format is considered invalid combination of values in individual header fields or mismatch
    /// between read value and expected fixed value such as start/stop byte.</para>
    /// </summary>
    public class InvalidPacketFormatException : Exception
    {
        /// <inheritdoc />
        public InvalidPacketFormatException(string message) : base(message) { }
    }
}
