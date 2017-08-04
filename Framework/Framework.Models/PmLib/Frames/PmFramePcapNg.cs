/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2013 Brno University of Technology - Faculty of Information Technology (http://www.fit.vutbr.cz)
 * Author(s):
 * Vladimir Vesely (mailto:ivesely@fit.vutbr.cz)
 * Martin Mares (mailto:xmares04@stud.fit.vutbr.cz)
 * Jan Plusal (mailto:xplusk03@stud.fit.vutbr.cz)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Netfox.Framework.Models.PmLib.Captures;
using Netfox.Framework.Models.PmLib.SupportedTypes;

namespace Netfox.Framework.Models.PmLib.Frames
{
    [Serializable]
    [DataContract]
    public class PmFramePcapNg : PmFrameBase
    {
        private FrameBLockType _blockType; // holds this packet data
        private PcapNgInterface _iface; // holds link to interface to which frame belongs

        /// <summary>
        ///     Constructor used when indexing
        /// </summary>
        public PmFramePcapNg(
            PmCaptureBase pmCapture,
            Int64 fraIndex,
            Int64 fraOffset,
            PmLinkType pmLinkType,
            DateTime timeStamp,
            Int64 oriLength,
            Int64 incLength,
            FrameBLockType type,
            PcapNgInterface iface) : base(pmCapture, pmLinkType, timeStamp, incLength)
        {
            this.FrameIndex = fraIndex;
            this.FrameOffset = fraOffset;
            this.OriginalLength = oriLength;
            this._blockType = type;
            this._iface = iface;
            this.PmFrameType = PmFrameType.PcapNg;

            UInt32 startOffset = 0;
            switch(this._blockType)
            {
                case FrameBLockType.EnhancedPacket:
                    startOffset = 28;
                    break;
                case FrameBLockType.SimplePacket:
                    startOffset = 12;
                    break;
                case FrameBLockType.PacketBlock:
                    startOffset = 28;
                    break;
            }
            this.L2Offset = this.FrameOffset + startOffset;
        }

        public PmFramePcapNg() : base() { }
        
        // optional values :
        [NotMapped]
        public Byte[] EpbFlags { get; set; }
        [NotMapped]
        public Byte[] EpbHash { get; set; }
        public UInt64 Dropcount { get; set; }

        /// <summary>
        ///     Block type must be known to get raw packet data
        /// </summary>
        public enum FrameBLockType
        {
            PacketBlock,
            SimplePacket,
            EnhancedPacket
        }
    }
}