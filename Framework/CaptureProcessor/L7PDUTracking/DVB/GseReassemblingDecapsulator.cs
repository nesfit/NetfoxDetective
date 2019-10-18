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
using System.Linq;
using Netfox.Framework.Models.PmLib.Frames;
using PostSharp.Patterns.Contracts;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace Netfox.Framework.CaptureProcessor.L7PDUTracking.DVB
{
    /// <summary>
    /// GseReassemblingDecapsulator manages decapsulation of frames encapsulated inside GSE packets, which are carried in Base-Band frames.
    /// The decapsulator is capable of reassembly procedure according to the
    /// <a href="http://www.etsi.org/deliver/etsi_ts/102600_102699/10260601/01.02.01_60/ts_10260601v010201p.pdf#page=21">ETSI TS 102 606-1 V1.2.1</a>.
    /// Reassembling distinguishes single input stream and individual multiple input stream based on <see cref="SisMis"/> and <see cref="BaseBandHeader.InputStreamIdentifier"/>.
    /// </summary>
    internal class GseReassemblingDecapsulator
    {
        /// <summary>
        /// Special key used in <see cref="_reassemblyBuffers"/> for single input stream. It is used to distinguish between:
        /// <list type="bullet">
        /// <item><description>
        ///     single input stream (<see cref="SisMis"/>==<see cref="SisMis.Single"/>)
        ///     with <see cref="BaseBandHeader.Matype2"/>(<see cref="BaseBandHeader.InputStreamIdentifier"/>)==0x00
        /// </description></item>
        /// <item><description>
        ///     multiple input stream (<see cref="SisMis"/>==<see cref="SisMis.Multiple"/>)
        ///     also with <see cref="BaseBandHeader.Matype2"/>(<see cref="BaseBandHeader.InputStreamIdentifier"/>)==0x00.
        /// </description></item>
        /// </list> 
        /// </summary>
        /// Therefore multiple input stream with <see cref="BaseBandHeader.InputStreamIdentifier"/>==0x00 would have 0x00 as a key in <see cref="_reassemblyBuffers"/>,
        /// on the other hand, single input stream with the same <see cref="BaseBandHeader.InputStreamIdentifier"/> would have this value as a key in <see cref="_reassemblyBuffers"/>. 
        private const short SingleInputStreamReservedIdentifier = -1; 

        /// <summary>
        /// Reasembly buffers as a dictionary of dictionaries (for each stream) with <see cref="GseReassemblyBuffer"/>s for each fragment ID being processed.
        /// First key is streamIdentifier (<see cref="BaseBandHeader.InputStreamIdentifier"/>) from <see cref="BaseBandHeader"/>
        /// or <see cref="SingleInputStreamReservedIdentifier"/> in case of single input stream (<see cref="SisMis"/>==<see cref="SisMis.Single"/>).
        /// </summary>
        private readonly Dictionary<short, Dictionary<byte, GseReassemblyBuffer>> _reassemblyBuffers = new Dictionary<short, Dictionary<byte, GseReassemblyBuffer>>();

        /// <summary>
        /// Process provided <paramref name="baseBandFrame"/>. Decapsulate frames from <see cref="GsePacket"/>s in <paramref name="baseBandFrame"/>. In case of GSE fragmentation,
        /// add <see cref="GsePacket"/>s to corresponding <see cref="GseReassemblyBuffer"/>s and perform one step of thr GSE reassembly procedure. Successfully reassembled GSE is
        /// then decapsulated, too.
        /// </summary>
        /// <param name="baseBandFrame">Base-Band frame from which the <see cref="GsePacket"/>s (<see cref="BaseBandFrame.UserPackets"/>) should be decapsulated.</param>
        /// <param name="encapsulatingFrames">Frames carrying provided <paramref name="baseBandFrame"/>.</param>
        /// <returns>Decapsualted frames, which were eventually reassembled if it was required.</returns>
        public IEnumerable<PmFrameBase> Process([Required] BaseBandFrame baseBandFrame, [Required] ICollection<PmFrameBase> encapsulatingFrames)
        {
            var decapsulatedFrames = new List<PmFrameBase>();

            // key for identification of a stream used in this._reassemblyBuffers
            var si = baseBandFrame.BaseBandHeader.InputStreamIdentifier ?? SingleInputStreamReservedIdentifier;

            // lazy instantiation of reassembling structures for given stream (si)
            if(!this._reassemblyBuffers.ContainsKey(si))
            {
                this._reassemblyBuffers.Add(si, new Dictionary<byte, GseReassemblyBuffer>());
            }

            var stop = false;
            foreach(var gsePacket in baseBandFrame.UserPackets)
            {
                if(stop) { break; }
                switch(gsePacket.Type)
                {
                    case PacketType.Complete:
                        // complete packet is here considered one fragment itself
                        var completeFrame = PmFrameEncapsulated.Create(new List<IFragment>(1){gsePacket});
                        completeFrame.DecapsulatedFromFrames.AddRange(encapsulatingFrames);
                        foreach(var encapsulatingFrame in encapsulatingFrames){encapsulatingFrame.EncapsulatedFrames.Add(completeFrame);}

                        decapsulatedFrames.Add(completeFrame);
                        continue;

                    case PacketType.Padding:
                        // "(...) padding is detected and the Receiver shall discard all the following bytes until the end of the Base
                        // Band frame." http://www.etsi.org/deliver/etsi_ts/102600_102699/10260601/01.02.01_60/ts_10260601v010201p.pdf#page=22
                        stop = true;
                        break;

                    case PacketType.Start:
                        if(!gsePacket.Header.FragmentID.HasValue){throw new ArgumentException("Fragmented packet does not contain FragmentID.");}

                        // lazy instantiation of GseReassemblyBuffer for given FragmentID
                        if(!this._reassemblyBuffers[si].ContainsKey(gsePacket.Header.FragmentID.Value))
                        {
                            this._reassemblyBuffers[si].Add(gsePacket.Header.FragmentID.Value, new GseReassemblyBuffer());
                        }

                        if(this._reassemblyBuffers[si][gsePacket.Header.FragmentID.Value].Count> 0)
                        {
                            // "(...) the receiver has fragments for that Frag ID in a reassembly buffer. If the Frag ID is already in use,
                            // the receiver shall first discard the already buffered fragments corresponding to this Frag ID;"
                            // http://www.etsi.org/deliver/etsi_ts/102600_102699/10260601/01.02.01_60/ts_10260601v010201p.pdf#page=21
                            this._reassemblyBuffers[si][gsePacket.Header.FragmentID.Value].Clear();
                            // Buffered GSE fragments were discarded.
                        }

                        this._reassemblyBuffers[si][gsePacket.Header.FragmentID.Value].Add(gsePacket, encapsulatingFrames);
                        break;

                    case PacketType.Intermediate:
                        if(!gsePacket.Header.FragmentID.HasValue){throw new ArgumentException("Fragmented packet does not contain FragmentID.");}

                        if(this._reassemblyBuffers[si].ContainsKey(gsePacket.Header.FragmentID.Value))
                        {
                            this._reassemblyBuffers[si][gsePacket.Header.FragmentID.Value].Add(gsePacket, encapsulatingFrames);
                        }
                        // else: Intermediate GSE fragment was discarded because there was not Start GSE fragment before.
                        break;

                    case PacketType.End:
                        if(!gsePacket.Header.FragmentID.HasValue){throw new ArgumentException("Fragmented packet does not contain FragmentID.");}

                        if(this._reassemblyBuffers[si].ContainsKey(gsePacket.Header.FragmentID.Value))
                        {
                            this._reassemblyBuffers[si][gsePacket.Header.FragmentID.Value].Add(gsePacket, encapsulatingFrames);

                            var decapsulated = this._reassemblyBuffers[si][gsePacket.Header.FragmentID.Value].Retrieve();
                            decapsulatedFrames.Add(decapsulated);
                        }
                        // else: End GSE fragment was discarded because there was not Start GSE fragment before.
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Unexpected PacketType ({gsePacket.Type}).");
                }
            }

            foreach(var buffer in this._reassemblyBuffers[si]) { buffer.Value.BaseBandProcessed(); }

            return decapsulatedFrames;
        }

        /// <summary>
        /// Buffer of fragmented <see cref="GsePacket"/>s used during reassembly procedure.
        /// All stored fragments have to be from the same stream (<see cref="SisMis"/>, <see cref="BaseBandHeader.InputStreamIdentifier"/>)
        /// </summary>
        private class GseReassemblyBuffer
        {
            /// <summary>Counter of processed Base-Bands used to detect a PDU reassembly time-out error.</summary>
            private byte _baseBandsSinceFirstFragment; // = 0

            /// <summary>Stored fragmented <see cref="GsePacket"/>s</summary>
            private readonly List<GsePacket> _buffer = new List<GsePacket>();

            /// <summary>Frames, in which fragmented <see cref="GsePacket"/>s were carried.</summary>
            private readonly List<PmFrameBase> _encapsulatingFrames = new List<PmFrameBase>();
            
            /// <summary>
            /// Add fragmented <paramref name="gsePacket"/>, which was carried in <paramref name="encapsulatingFrames"/>, to this buffer.
            /// </summary>
            /// <param name="gsePacket">Fragmetned GSE packet</param>
            /// <param name="encapsulatingFrames">Frames which carried provided <paramref name="gsePacket"/></param>
            /// <exception cref="ArgumentException">If <paramref name="gsePacket"/> is not a GSE fragment packet (<see cref="GsePacket.IsGseFragment"/>==false).</exception>
            public void Add([Required] GsePacket gsePacket, [Required] IEnumerable<PmFrameBase> encapsulatingFrames)
            {
                if(!gsePacket.IsGseFragment){throw new ArgumentException("GSE packet is not a GSE fragment packet.", nameof(gsePacket));}

                // check if gsePacket can be added to this buffer based on the sequence of types of GSE packets in this buffer
                // (Start Intermediate* End)
                switch(gsePacket.Type)
                {
                    case PacketType.Padding:
                    case PacketType.Complete:
                        throw new ArgumentException($"GSE packet with type {gsePacket.Type} cannot be added to GseReassemblyBuffer.", nameof(gsePacket));
                    case PacketType.Start:
                        if(this._buffer.Any())
                        {
                            throw new ArgumentException($"GSE packet with type {gsePacket.Type} cannot be added to not empty GseReassemblyBuffer.", nameof(gsePacket));
                        }
                        break;
                    case PacketType.Intermediate:
                    case PacketType.End:
                        if(this._buffer.Any())
                        {
                            if(!(this._buffer.Last().Type == PacketType.Start || this._buffer.Last().Type == PacketType.Intermediate))
                            {
                                throw new ArgumentException($"GSE packet with type {gsePacket.Type} cannot be added to GseReassemblyBuffer after packet with type {this._buffer.Last().Type}.", nameof(gsePacket));
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"GSE packet with type {gsePacket.Type} cannot be added to empty GseReassemblyBuffer.", nameof(gsePacket));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this._buffer.Add(gsePacket);
                foreach(var ef in encapsulatingFrames)
                {
                    if(!this._encapsulatingFrames.Contains(ef)) { this._encapsulatingFrames.Add(ef); }
                }
            }

            /// <summary>
            /// One whole Base-Band was processed, either increase <see cref="_baseBandsSinceFirstFragment"/> or discrard content
            /// of this buffer as a PDU reassembly time-out error occurred. 
            /// </summary>
            public void BaseBandProcessed()
            {
                if(this._baseBandsSinceFirstFragment < 255)
                {
                    if(this._buffer.Any()) { this._baseBandsSinceFirstFragment++; }
                }
                else
                {
                    // "If a PDU belonging to a given Frag ID cannot be re-assembled within 255 consecutive frames, the receiver shall discard
                    // the buffer and free the Frag ID. This error event should be recorded as a PDU reassembly time-out error."
                    // http://www.etsi.org/deliver/etsi_ts/102600_102699/10260601/01.02.01_60/ts_10260601v010201p.pdf#page=22
                    this.Clear();
                }
            }

            /// <summary>
            /// Discard content of this buffer.
            /// </summary>
            public void Clear()
            {
                this._baseBandsSinceFirstFragment = 0;
                this._buffer.Clear();
                this._encapsulatingFrames.Clear();
            }

            /// <summary>Number of stored fragments.</summary>
            public int Count => this._buffer.Count;

            /// <summary>
            /// Retrieve content of this buffer as a decapsulated frame and clear this buffer.
            /// </summary>
            /// <exception cref="InvalidOperationException">If buffer does not contain completed sequence of GSE fragments.</exception>
            /// <returns>Reassembled and decapsulated frame.</returns>
            public PmFrameBase Retrieve()
            {
                if(!this.IsCompleted){throw new InvalidOperationException("Buffer does not contain completed sequence of GSE fragments.");}
                var decapsulatedFrame = PmFrameEncapsulated.Create(this._buffer);
                foreach(var encapsulatingFrame in this._encapsulatingFrames)
                {
                    decapsulatedFrame.DecapsulatedFromFrames.Add(encapsulatingFrame);
                    encapsulatingFrame.EncapsulatedFrames.Add(decapsulatedFrame);
                }
                this.Clear();
                return decapsulatedFrame;
            }

            public bool IsCompleted
            {
                get
                {
                    // Start Intermediate* End
                    return this._buffer.Any()
                           && this._buffer.First().Type == PacketType.Start && this._buffer.Last().Type == PacketType.End
                           && (this._buffer.Count == 2 || this._buffer.GetRange(1, this._buffer.Count - 2).TrueForAll(a => a.Type == PacketType.Intermediate));
                }
            }
        }
    }
}
