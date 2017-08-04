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

using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace Netfox.Core.Interfaces.Model
{
    public interface IPduStreamBasedProvider
    {
        ///// <summary>
        ///// When overridden in a derived class, clears all buffers for this stream and causes any
        ///// buffered data to be written to the underlying device.
        ///// </summary>
        ///// <remarks> Pluskal, 2/10/2014.</remarks>
        ///// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        //void Flush();

        ///// <summary> When overridden in a derived class, sets the length of the current stream.</summary>
        ///// <remarks> Pluskal, 2/10/2014.</remarks>
        ///// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        ///// <param name="value"> The desired length of the current stream in bytes. </param>
        //void SetLength(long value);

        ///// <summary>
        ///// When overridden in a derived class, writes a sequence of bytes to the current stream and
        ///// advances the current position within this stream by the number of bytes written.
        ///// </summary>
        ///// <remarks> Pluskal, 2/10/2014.</remarks>
        ///// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        ///// <param name="buffer"> An array of bytes. This method copies <paramref name="count" /> bytes
        ///// from <paramref name="buffer" /> to the current stream. </param>
        ///// <param name="offset"> The zero-based byte offset in <paramref name="buffer" /> at which to
        ///// begin copying bytes to the current stream. </param>
        ///// <param name="count">  The number of bytes to be written to the current stream. </param>
        //void Write(byte[] buffer, int offset, int count);

        ///// <summary>
        ///// When overridden in a derived class, gets the length in bytes of the stream.
        ///// </summary>
        ///// <exception cref="NotSupportedException"> Thrown when the requested operation is not supported. </exception>
        ///// <value> A long value representing the length of the stream in bytes.</value>
        //long Length { get; }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports reading.
        /// </summary>
        /// <value> true if the stream supports reading; otherwise, false.</value>
        Boolean CanRead { get; }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports seeking.
        /// </summary>
        /// <value> true if the stream supports seeking; otherwise, false.</value>
        Boolean CanSeek { get; }

        /// <summary>
        ///     When overridden in a derived class, gets a value indicating whether the current stream
        ///     supports writing.
        /// </summary>
        /// <value> true if the stream supports writing; otherwise, false.</value>
        Boolean CanWrite { get; }

        /// <summary>
        ///     When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <value> The current position within the stream.</value>
        Int64 Position { get; set; }

        ///// <summary> Gets the current PDU.</summary>
        ///// <value> The current PDU.</value>
        ///// ### <exception cref="InvalidOperationException"> Thrown when the requested operation is
        ///// invalid. </exception>
        //PDU Current { get; }

        ///// <summary> Gets the current peeked PDU.</summary>
        ///// <value> The current peeked PDU.</value>
        ///// ### <exception cref="InvalidOperationException"> Thrown when the requested operation is
        ///// invalid. </exception>
        //PDU CurrentPeek { get; }

        Boolean CanTimeout { get; }
        Int32 ReadTimeout { get; set; }
        Int32 WriteTimeout { get; set; }

        /// <summary> Init stream to begin reading a new application message.</summary>
        /// <remarks> Pluskal, 2/11/2014.</remarks>
        /// <returns> true if it succeeds, false is there ano reamaining messages.</returns>
        Boolean NewMessage();

        /// <summary>
        ///     Reads a sequence of bytes from the current stream without advancing the position within the
        ///     stream.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotImplementedException">
        ///     Thrown when the requested operation is
        ///     unimplemented.
        /// </exception>
        /// <param name="buffer">         The buffer. </param>
        /// <param name="bufferOffset">   The buffer offset. </param>
        /// <param name="streamPosition"> The offset in stream. </param>
        /// <param name="requestedCount"> Number of requested bytes. </param>
        /// <param name="origin">         The origin point from where the streamPosition is referenced. </param>
        /// <returns> The current top-of-stack object.</returns>
        Int32 Peek(Byte[] buffer, Int32 bufferOffset, Int32 streamPosition, Int32 requestedCount, SeekOrigin origin);

        /// <summary>
        ///     When overridden in a derived class, reads a sequence of bytes from the current stream and
        ///     advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the buffer contains
        ///     the specified byte array with the values between <paramref name="offset" /> and
        ///     (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from
        ///     the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in <paramref name="buffer" /> at which
        ///     to begin storing the data read from the current stream.
        /// </param>
        /// <param name="requestedCount"> Number of requested bytes. </param>
        /// <returns>
        ///     The total number of bytes read into the buffer. This can be less than the number of bytes
        ///     requested if that many bytes are not currently available, or zero (0) if the end of the
        ///     stream has been reached.
        /// </returns>
        /// ###
        /// <param name="count"> The maximum number of bytes to be read from the current stream. </param>
        Int32 Read(Byte[] buffer, Int32 offset, Int32 requestedCount);

        /// <summary> Resets the stream to begin position.</summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <returns> True if reset was successful and there are some PDUs remaining.</returns>
        Boolean Reset();

        /// <summary>
        ///     When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <remarks> Pluskal, 2/10/2014.</remarks>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the requested operation is not
        ///     supported.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when origin is not supported. </exception>
        /// <param name="offset"> A byte offset relative to the <paramref name="origin" /> parameter. </param>
        /// <param name="origin">
        ///     A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the
        ///     reference point used to obtain the new position.
        /// </param>
        /// <returns> The new position within the current stream.</returns>
        Int64 Seek(Int64 offset, SeekOrigin origin);

        #region NotSupported yet -- will be implemented on demand
        Task CopyToAsync(Stream destination);
        Task CopyToAsync(Stream destination, Int32 bufferSize);
        Task CopyToAsync(Stream destination, Int32 bufferSize, CancellationToken cancellationToken);
        void CopyTo(Stream destination);
        void CopyTo(Stream destination, Int32 bufferSize);
        void Close();
        void Dispose();
        //Task FlushAsync();
        //Task FlushAsync(CancellationToken cancellationToken);
        IAsyncResult BeginRead(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state);
        Int32 EndRead(IAsyncResult asyncResult);
        Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count);
        Task<Int32> ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken);
        //IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
        //void EndWrite(IAsyncResult asyncResult);
        //Task WriteAsync(byte[] buffer, int offset, int count);
        //Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        Int32 ReadByte();
        //void WriteByte(byte value);
        Object GetLifetimeService();
        Object InitializeLifetimeService();
        ObjRef CreateObjRef(Type requestedType);
        #endregion
    }
}