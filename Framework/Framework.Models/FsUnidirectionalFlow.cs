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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Netfox.Core.Enums;
using Netfox.Framework.Models.PmLib.Frames;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models
{
    /// <summary> A file system unidirectional flow.</summary>
    [NotMapped]
    [DataContract]
    public class FsUnidirectionalFlow //: IFsUnidirectionalFlow
    {
        /// <summary> The default.</summary>
        internal static FsUnidirectionalFlow Default { get; }= new FsUnidirectionalFlow();

        /// <summary> The non data frames.</summary>
        private List<PmFrameBase> _nonDataFrames;

        /// <summary> The pd us.</summary>
        private List<L7PDU> _pdUs = new List<L7PDU>();

        /// <summary> The virtual frames.</summary>
        private List<PmFrameBase> _virtualFrames;
        public L4Conversation L4Conversation { get; }

        /// <summary>
        ///     Constructor that prevents a default instance of this class from being created.
        /// </summary>
        private FsUnidirectionalFlow() {}

        public FsUnidirectionalFlow(L4Conversation l4Conversation, DaRFlowDirection flowDirection)
        {
            this.L4Conversation = l4Conversation;
            this.FlowDirection = flowDirection;
        }

        /// <summary> Gets or sets the flow direction.</summary>
        /// <value> The flow direction.</value>
        public DaRFlowDirection FlowDirection { get; set; } = DaRFlowDirection.non;

        /// <summary>
        ///     Flow identifier For example: TCP session in one direction the SEQ+1 num of packet with SYN
        ///     flag
        ///     in other direction the ACK num of packet with SYN+ACK flags.
        /// </summary>
        /// <value> The identifier of the flow.</value>
        [DataMember]
        public Int64 FlowIdentifier { get; set; }

        /// <summary> Gets all frames.</summary>
        /// <value> all frames.</value>
        public IEnumerable<PmFrameBase> Frames
        {
            get { return (this._pdUs.Select(pdu => pdu.FrameList).SelectMany(frame => frame)); }
        }

        /// <summary> Gets the real frames.</summary>
        /// <value> The real frames.</value>
        public IEnumerable<PmFrameBase> RealFrames
        {
            get
            {
                return
                    this.Frames.Where(f => f.PmFrameType != PmFrameType.VirtualBlank && f.PmFrameType != PmFrameType.VirutalNoise)
                        .Concat(this.NonDataFrames);
            }
        }

        /// <summary> Gets the Date/Time of the first seen.</summary>
        /// <value> The first seen.</value>
        public DateTime FirstSeen
        {
            get { return this.Frames.Any()? this.Frames.Min(frame => frame.TimeStamp) : this.NonDataFrames.Min(frame => frame.TimeStamp); }
        }

        /// <summary> Gets the Date/Time of the last seen.</summary>
        /// <value> The last seen.</value>
        public DateTime LastSeen
        {
            get { return this.Frames.Any()? this.Frames.Max(frame => frame.TimeStamp) : this.NonDataFrames.Max(frame => frame.TimeStamp); }
        }

        /// <summary> Gets the pd us.</summary>
        /// <value> The pd us.</value>
        //public IEnumerable<IDaRL7PDU> PDUs { get { return _PDUs.OrderBy(f => f.OrderingKey); } }
        public IEnumerable<L7PDU> PDUs => this._pdUs;

        /// <summary> GETs All virtual frames.</summary>
        /// <value> The virtual frames.</value>
        public List<PmFrameBase> VirtualFrames => this._virtualFrames ?? (this._virtualFrames = new List<PmFrameBase>());

        /// <summary> Gets the non data frames.</summary>
        /// <value> The non data frames.</value>
        public List<PmFrameBase> NonDataFrames => this._nonDataFrames ?? (this._nonDataFrames = new List<PmFrameBase>());

        public L7Conversation L7Conversation { get; set; }

        /// <summary> Adds a l 7 PDU.</summary>
        /// <param name="pdu"> The PDU. </param>
        public void AddL7PDU(L7PDU pdu) => this._pdUs.Add(pdu);

        /// <summary> Creates l 7 PDU.</summary>
        /// <returns> The new l 7 PDU.</returns>
        public L7PDU CreateL7PDU() => new L7PDU(this)
        {
            FlowDirection = this.FlowDirection
        };
        

        /// <summary> Gets the last PDU.</summary>
        /// <returns> The last PDU.</returns>
        public L7PDU GetLastPDU() => this._pdUs.Last();

        /// <summary> Removes the l 7 PDU described by PDU.</summary>
        /// <param name="pdu"> The PDU. </param>
        public void RemoveL7PDU(L7PDU pdu) {
            this._pdUs.Remove(pdu);
        }

        /// <summary> Adds a l 7 PDU.</summary>
        /// <param name="pdu"> The PDU. </param>
        /// <remarks>All PDUs in this flow will be lost!</remarks>
        public void SubstituteL7PDU(List<L7PDU> pdusList)
        {
            this._pdUs = pdusList.Select(pdu => new L7PDU(this,pdu)).ToList();
        }
    }
}